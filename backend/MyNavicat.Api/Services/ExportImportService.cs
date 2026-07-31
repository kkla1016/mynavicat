using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MySqlConnector;
using MyNavicat.Api.Data;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Providers;

namespace MyNavicat.Api.Services
{
    public interface IExportImportService
    {
        Task<(byte[] fileBytes, string contentType, string fileName)> ExportTableDataAsync(ExportRequestDto request);
        Task<ImportResultDto> ImportTableDataAsync(int connectionId, string database, string table, Stream fileStream, string fileName);
    }

    public class ExportImportService : IExportImportService
    {
        private readonly AppDbContext _db;
        private readonly IDbProviderFactory _providerFactory;

        public ExportImportService(AppDbContext db, IDbProviderFactory providerFactory)
        {
            _db = db;
            _providerFactory = providerFactory;
        }

        public async Task<(byte[] fileBytes, string contentType, string fileName)> ExportTableDataAsync(ExportRequestDto request)
        {
            var conn = await _db.Connections.FindAsync(request.ConnectionId);
            if (conn == null) throw new KeyNotFoundException("Connection not found.");

            var provider = _providerFactory.GetProvider(conn.DbType);
            var pagedData = await provider.GetTableDataAsync(conn, request.DatabaseName, request.TableName, 1, 10000);
            var rows = pagedData.Items;

            var format = request.Format.ToUpper();
            byte[] fileBytes;
            string contentType;
            string fileName;

            switch (format)
            {
                case "JSON":
                    var jsonStr = JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true });
                    fileBytes = Encoding.UTF8.GetBytes(jsonStr);
                    contentType = "application/json";
                    fileName = $"{request.TableName}.json";
                    break;

                case "SQL":
                    var sqlSb = new StringBuilder();
                    if (rows.Count > 0)
                    {
                        var cols = ObjectKeys(rows[0]);
                        var colList = string.Join(", ", cols.Select(c => $"`{c}`"));

                        foreach (var row in rows)
                        {
                            var valList = string.Join(", ", cols.Select(c => FormatSqlValue(row.ContainsKey(c) ? row[c] : null)));
                            sqlSb.AppendLine($"INSERT INTO `{request.TableName}` ({colList}) VALUES ({valList});");
                        }
                    }
                    fileBytes = Encoding.UTF8.GetBytes(sqlSb.ToString());
                    contentType = "application/sql";
                    fileName = $"{request.TableName}.sql";
                    break;

                case "CSV":
                default:
                    var csvSb = new StringBuilder();
                    if (rows.Count > 0)
                    {
                        var cols = ObjectKeys(rows[0]);
                        csvSb.AppendLine(string.Join(",", cols.Select(EscapeCsvField)));

                        foreach (var row in rows)
                        {
                            var vals = cols.Select(c => EscapeCsvField(row.ContainsKey(c) ? row[c]?.ToString() : null));
                            csvSb.AppendLine(string.Join(",", vals));
                        }
                    }
                    fileBytes = Encoding.UTF8.GetBytes(csvSb.ToString());
                    contentType = "text/csv";
                    fileName = $"{request.TableName}.csv";
                    break;
            }

            return (fileBytes, contentType, fileName);
        }

        public async Task<ImportResultDto> ImportTableDataAsync(int connectionId, string database, string table, Stream fileStream, string fileName)
        {
            var conn = await _db.Connections.FindAsync(connectionId);
            if (conn == null) throw new KeyNotFoundException("Connection not found.");

            int success = 0;
            int failed = 0;

            using var reader = new StreamReader(fileStream, Encoding.UTF8);
            var content = await reader.ReadToEndAsync();

            if (fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var items = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(content);
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            var dict = item.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value.ToString());
                            bool ok = await InsertRowAsync(conn, database, table, dict);
                            if (ok) success++; else failed++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new ImportResultDto { SuccessRows = success, FailedRows = failed + 1, ErrorSummary = ex.Message };
                }
            }
            else // 預設處理 CSV
            {
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 1)
                {
                    var headers = lines[0].Split(',').Select(h => h.Trim('"', ' ')).ToArray();
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var values = lines[i].Split(',').Select(v => v.Trim('"', ' ')).ToArray();
                        var row = new Dictionary<string, object?>();
                        for (int j = 0; j < headers.Length && j < values.Length; j++)
                        {
                            row[headers[j]] = values[j];
                        }
                        bool ok = await InsertRowAsync(conn, database, table, row);
                        if (ok) success++; else failed++;
                    }
                }
            }

            return new ImportResultDto { SuccessRows = success, FailedRows = failed };
        }

        private async Task<bool> InsertRowAsync(Models.Entities.Connection conn, string database, string table, Dictionary<string, object?> row)
        {
            try
            {
                if (conn.DbType.Equals("MySQL", StringComparison.OrdinalIgnoreCase) || conn.DbType.Equals("MariaDB", StringComparison.OrdinalIgnoreCase))
                {
                    var provider = new MySqlProvider(new CryptoService());
                    var connStr = provider.BuildConnectionString(conn, database);
                    using var myConn = new MySqlConnection(connStr);
                    await myConn.OpenAsync();

                    var cols = row.Keys.ToList();
                    var colNames = string.Join(", ", cols.Select(c => $"`{c}`"));
                    var paramNames = string.Join(", ", cols.Select(c => $"@{c}"));

                    string query = $"INSERT INTO `{table}` ({colNames}) VALUES ({paramNames});";
                    using var cmd = new MySqlCommand(query, myConn);
                    foreach (var col in cols)
                    {
                        cmd.Parameters.AddWithValue($"@{col}", row[col] ?? DBNull.Value);
                    }
                    await cmd.ExecuteNonQueryAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private List<string> ObjectKeys(Dictionary<string, object?> dict)
        {
            return dict.Keys.ToList();
        }

        private string EscapeCsvField(string? field)
        {
            if (string.IsNullOrEmpty(field)) return "\"\"";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }

        private string FormatSqlValue(object? value)
        {
            if (value == null || value == DBNull.Value) return "NULL";
            if (value is bool b) return b ? "1" : "0";
            if (value is int || value is long || value is double || value is decimal) return value.ToString()!;
            return $"'{value.ToString()?.Replace("'", "''")}'";
        }
    }
}
