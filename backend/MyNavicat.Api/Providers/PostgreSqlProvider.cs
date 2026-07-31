using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using MyNavicat.Api.Models.Common;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Models.Entities;
using MyNavicat.Api.Services;

namespace MyNavicat.Api.Providers
{
    public class PostgreSqlProvider : IDbProvider
    {
        private readonly ICryptoService _cryptoService;

        public PostgreSqlProvider(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }

        public string DbType => "PostgreSQL";

        public string BuildConnectionString(Connection connection, string? databaseOverride = null)
        {
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = connection.Host,
                Port = connection.Port > 0 ? connection.Port : 5432,
                Username = connection.Username,
                Password = _cryptoService.Decrypt(connection.EncryptedPassword),
                Database = databaseOverride ?? (string.IsNullOrEmpty(connection.DatabaseName) ? "postgres" : connection.DatabaseName),
                Timeout = 10
            };

            return builder.ConnectionString;
        }

        public async Task<bool> TestConnectionAsync(Connection connection)
        {
            try
            {
                using var conn = new NpgsqlConnection(BuildConnectionString(connection));
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
            using var conn = new NpgsqlConnection(BuildConnectionString(connection, "postgres"));
            await conn.OpenAsync();

            string query = "SELECT datname FROM pg_database WHERE datistemplate = false AND datname NOT IN ('postgres');";
            using var cmd = new NpgsqlCommand(query, conn);
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
            using var conn = new NpgsqlConnection(BuildConnectionString(connection, database));
            await conn.OpenAsync();

            string query = @"
                SELECT table_name
                FROM information_schema.tables
                WHERE table_schema = 'public' AND table_type = 'BASE TABLE';";

            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tables.Add(new TableInfoDto
                {
                    Name = reader.GetString(0),
                    RowCount = 0
                });
            }

            return tables;
        }

        public async Task<TableSchemaDto> GetTableSchemaAsync(Connection connection, string database, string table)
        {
            var schema = new TableSchemaDto { TableName = table };
            using var conn = new NpgsqlConnection(BuildConnectionString(connection, database));
            await conn.OpenAsync();

            string query = @"
                SELECT column_name, data_type, is_nullable
                FROM information_schema.columns
                WHERE table_schema = 'public' AND table_name = @tableName
                ORDER BY ordinal_position;";

            using var cmd = new NpgsqlCommand(query, conn);
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

            using var conn = new NpgsqlConnection(BuildConnectionString(connection, database));
            await conn.OpenAsync();

            string query = $"SELECT * FROM \"{table}\" LIMIT @pageSize OFFSET @offset;";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);
            cmd.Parameters.AddWithValue("@offset", offset);

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
            using var conn = new NpgsqlConnection(BuildConnectionString(connection, database));
            await conn.OpenAsync();

            string query = $"SELECT COUNT(*) FROM \"{table}\";";
            using var cmd = new NpgsqlCommand(query, conn);
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt64(result);
        }

        public BackupCommand GetBackupCommand(Connection connection, string database, List<string>? tables, string outputPath)
        {
            var rawPassword = _cryptoService.Decrypt(connection.EncryptedPassword);
            var port = connection.Port > 0 ? connection.Port : 5432;

            var sb = new StringBuilder();
            sb.Append($"-h \"{connection.Host}\" -p {port} -U \"{connection.Username}\" -F p -f \"{outputPath}\" ");

            if (tables != null && tables.Count > 0)
            {
                foreach (var t in tables)
                {
                    sb.Append($"-t \"{t}\" ");
                }
            }

            sb.Append($"\"{database}\"");

            var envVars = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(rawPassword))
            {
                envVars["PGPASSWORD"] = rawPassword;
            }

            return new BackupCommand
            {
                Executable = "pg_dump",
                Arguments = sb.ToString(),
                EnvironmentVariables = envVars,
                OutputFilePath = outputPath
            };
        }

        public BackupCommand GetRestoreCommand(Connection connection, string database, string inputPath)
        {
            var rawPassword = _cryptoService.Decrypt(connection.EncryptedPassword);
            var port = connection.Port > 0 ? connection.Port : 5432;

            var args = $"-h \"{connection.Host}\" -p {port} -U \"{connection.Username}\" -d \"{database}\" -f \"{inputPath}\"";

            var envVars = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(rawPassword))
            {
                envVars["PGPASSWORD"] = rawPassword;
            }

            return new BackupCommand
            {
                Executable = "psql",
                Arguments = args,
                EnvironmentVariables = envVars,
                OutputFilePath = inputPath
            };
        }
    }
}
