using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using MyNavicat.Api.Models.Common;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Models.Entities;
using MyNavicat.Api.Services;

namespace MyNavicat.Api.Providers
{
    public class OracleProvider : IDbProvider
    {
        private readonly ICryptoService _cryptoService;

        public OracleProvider(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }

        public string DbType => "Oracle";

        public string BuildConnectionString(Connection connection, string? databaseOverride = null)
        {
            var port = connection.Port > 0 ? connection.Port : 1521;
            var sidOrService = string.IsNullOrEmpty(connection.DatabaseName) ? "XE" : connection.DatabaseName;
            var password = _cryptoService.Decrypt(connection.EncryptedPassword);

            var builder = new OracleConnectionStringBuilder
            {
                DataSource = $"{connection.Host}:{port}/{sidOrService}",
                UserID = connection.Username,
                Password = password,
                ConnectionTimeout = 10
            };

            return builder.ConnectionString;
        }

        public async Task<bool> TestConnectionAsync(Connection connection)
        {
            try
            {
                using var conn = new OracleConnection(BuildConnectionString(connection));
                await conn.OpenAsync();
                return conn.State == ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> GetDatabasesAsync(Connection connection)
        {
            var sid = string.IsNullOrEmpty(connection.DatabaseName) ? "XE" : connection.DatabaseName;
            return await Task.FromResult(new List<string> { sid });
        }

        public async Task<List<TableInfoDto>> GetTablesAsync(Connection connection, string database)
        {
            var tables = new List<TableInfoDto>();
            using var conn = new OracleConnection(BuildConnectionString(connection, database));
            await conn.OpenAsync();

            string query = "SELECT table_name FROM user_tables;";
            using var cmd = new OracleCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tables.Add(new TableInfoDto { Name = reader.GetString(0) });
            }

            return tables;
        }

        public async Task<TableSchemaDto> GetTableSchemaAsync(Connection connection, string database, string table)
        {
            var schema = new TableSchemaDto { TableName = table };
            using var conn = new OracleConnection(BuildConnectionString(connection, database));
            await conn.OpenAsync();

            string query = @"
                SELECT column_name, data_type, nullable
                FROM user_tab_columns
                WHERE table_name = :tableName
                ORDER BY column_id;";

            using var cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("tableName", table.ToUpper()));
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                schema.Columns.Add(new ColumnSchemaDto
                {
                    Name = reader.GetString(0),
                    DataType = reader.GetString(1),
                    IsNullable = reader.GetString(2).Equals("Y", StringComparison.OrdinalIgnoreCase)
                });
            }

            return schema;
        }

        public async Task<PagedResult<Dictionary<string, object?>>> GetTableDataAsync(Connection connection, string database, string table, int page, int pageSize)
        {
            var rows = new List<Dictionary<string, object?>>();
            long totalCount = await GetTableCountAsync(connection, database, table);
            int offset = (page - 1) * pageSize;

            using var conn = new OracleConnection(BuildConnectionString(connection, database));
            await conn.OpenAsync();

            string query = $"SELECT * FROM \"{table.ToUpper()}\" OFFSET :offset ROWS FETCH NEXT :pageSize ROWS ONLY;";
            using var cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add(new OracleParameter("offset", offset));
            cmd.Parameters.Add(new OracleParameter("pageSize", pageSize));

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                rows.Add(row);
            }

            return new PagedResult<Dictionary<string, object?>>(rows, page, pageSize, totalCount);
        }

        public async Task<long> GetTableCountAsync(Connection connection, string database, string table)
        {
            using var conn = new OracleConnection(BuildConnectionString(connection, database));
            await conn.OpenAsync();

            string query = $"SELECT COUNT(*) FROM \"{table.ToUpper()}\";";
            using var cmd = new OracleCommand(query, conn);
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt64(result);
        }

        public BackupCommand GetBackupCommand(Connection connection, string database, List<string>? tables, string outputPath)
        {
            var rawPassword = _cryptoService.Decrypt(connection.EncryptedPassword);
            var port = connection.Port > 0 ? connection.Port : 1521;
            var sid = string.IsNullOrEmpty(connection.DatabaseName) ? "XE" : connection.DatabaseName;

            var args = $"userid={connection.Username}/{rawPassword}@{connection.Host}:{port}/{sid} file=\"{outputPath}\" log=\"{outputPath}.log\"";

            return new BackupCommand
            {
                Executable = "exp",
                Arguments = args,
                OutputFilePath = outputPath
            };
        }

        public BackupCommand GetRestoreCommand(Connection connection, string database, string inputPath)
        {
            var rawPassword = _cryptoService.Decrypt(connection.EncryptedPassword);
            var port = connection.Port > 0 ? connection.Port : 1521;
            var sid = string.IsNullOrEmpty(connection.DatabaseName) ? "XE" : connection.DatabaseName;

            var args = $"userid={connection.Username}/{rawPassword}@{connection.Host}:{port}/{sid} file=\"{inputPath}\" full=y";

            return new BackupCommand
            {
                Executable = "imp",
                Arguments = args,
                OutputFilePath = inputPath
            };
        }
    }
}
