using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MyNavicat.Api.Models.Common;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Models.Entities;
using MyNavicat.Api.Services;

namespace MyNavicat.Api.Providers
{
    public class SqlServerProvider : IDbProvider
    {
        private readonly ICryptoService _cryptoService;

        public SqlServerProvider(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }

        public string DbType => "SqlServer";

        public string BuildConnectionString(Connection connection, string? databaseOverride = null)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = $"{connection.Host},{connection.Port}",
                UserID = connection.Username,
                Password = _cryptoService.Decrypt(connection.EncryptedPassword),
                InitialCatalog = databaseOverride ?? (string.IsNullOrEmpty(connection.DatabaseName) ? "master" : connection.DatabaseName),
                TrustServerCertificate = true,
                ConnectTimeout = 10
            };

            return builder.ConnectionString;
        }

        public async Task<bool> TestConnectionAsync(Connection connection)
        {
            try
            {
                using var conn = new SqlConnection(BuildConnectionString(connection));
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
            var databases = new List<string>();
            using var conn = new SqlConnection(BuildConnectionString(connection, "master"));
            await conn.OpenAsync();

            string query = "SELECT name FROM sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb');";
            using var cmd = new SqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                databases.Add(reader.GetString(0));
            }

            return databases;
        }

        public async Task<List<TableInfoDto>> GetTablesAsync(Connection connection, string database)
        {
            var tables = new List<TableInfoDto>();
            using var conn = new SqlConnection(BuildConnectionString(connection, database));
            await conn.OpenAsync();

            string query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';";
            using var cmd = new SqlCommand(query, conn);
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
            using var conn = new SqlConnection(BuildConnectionString(connection, database));
            await conn.OpenAsync();

            string query = @"
                SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = @tableName;";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@tableName", table);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                schema.Columns.Add(new ColumnSchemaDto
                {
                    Name = reader.GetString(0),
                    DataType = reader.GetString(1),
                    IsNullable = reader.GetString(2).Equals("YES", StringComparison.OrdinalIgnoreCase)
                });
            }

            return schema;
        }

        public async Task<PagedResult<Dictionary<string, object?>>> GetTableDataAsync(Connection connection, string database, string table, int page, int pageSize)
        {
            var rows = new List<Dictionary<string, object?>>();
            long totalCount = await GetTableCountAsync(connection, database, table);
            int offset = (page - 1) * pageSize;

            using var conn = new SqlConnection(BuildConnectionString(connection, database));
            await conn.OpenAsync();

            string query = $"SELECT * FROM [{table}] ORDER BY (SELECT NULL) OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);

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
            using var conn = new SqlConnection(BuildConnectionString(connection, database));
            await conn.OpenAsync();

            string query = $"SELECT COUNT(*) FROM [{table}];";
            using var cmd = new SqlCommand(query, conn);
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt64(result);
        }

        public BackupCommand GetBackupCommand(Connection connection, string database, List<string>? tables, string outputPath)
        {
            var rawPassword = _cryptoService.Decrypt(connection.EncryptedPassword);
            var port = connection.Port > 0 ? connection.Port : 1433;

            var args = $"-S \"{connection.Host},{port}\" -U \"{connection.Username}\" -P \"{rawPassword}\" -Q \"BACKUP DATABASE [{database}] TO DISK = N'{outputPath}' WITH FORMAT, INIT;\"";

            return new BackupCommand
            {
                Executable = "sqlcmd",
                Arguments = args,
                OutputFilePath = outputPath
            };
        }

        public BackupCommand GetRestoreCommand(Connection connection, string database, string inputPath)
        {
            var rawPassword = _cryptoService.Decrypt(connection.EncryptedPassword);
            var port = connection.Port > 0 ? connection.Port : 1433;

            var args = $"-S \"{connection.Host},{port}\" -U \"{connection.Username}\" -P \"{rawPassword}\" -Q \"RESTORE DATABASE [{database}] FROM DISK = N'{inputPath}' WITH REPLACE;\"";

            return new BackupCommand
            {
                Executable = "sqlcmd",
                Arguments = args,
                OutputFilePath = inputPath
            };
        }
    }
}
