using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using MyNavicat.Api.Models.Common;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Models.Entities;
using MyNavicat.Api.Services;

namespace MyNavicat.Api.Providers
{
    /// <summary>
    /// MySQL 資料庫 Provider 實作
    /// </summary>
    public class MySqlProvider : IDbProvider
    {
        private readonly ICryptoService _cryptoService;

        public MySqlProvider(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }

        public string DbType => "MySQL";

        public string BuildConnectionString(Connection connection, string? databaseOverride = null)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = connection.Host,
                Port = (uint)(connection.Port > 0 ? connection.Port : 3306),
                UserID = connection.Username,
                Password = _cryptoService.Decrypt(connection.EncryptedPassword),
                Database = databaseOverride ?? connection.DatabaseName,
                ConnectionTimeout = 10
            };

            return builder.ConnectionString;
        }

        public async Task<bool> TestConnectionAsync(Connection connection)
        {
            try
            {
                using var conn = new MySqlConnection(BuildConnectionString(connection));
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
            using var conn = new MySqlConnection(BuildConnectionString(connection, "information_schema"));
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SHOW DATABASES;", conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var dbName = reader.GetString(0);
                if (dbName != "information_schema" && dbName != "performance_schema" && dbName != "mysql" && dbName != "sys")
                {
                    databases.Add(dbName);
                }
            }

            return databases;
        }

        public async Task<List<TableInfoDto>> GetTablesAsync(Connection connection, string database)
        {
            var tables = new List<TableInfoDto>();
            using var conn = new MySqlConnection(BuildConnectionString(connection, database));
            await conn.OpenAsync();

            string query = @"
                SELECT TABLE_NAME, TABLE_COMMENT, TABLE_ROWS, DATA_LENGTH
                FROM information_schema.TABLES 
                WHERE TABLE_SCHEMA = @dbName AND TABLE_TYPE = 'BASE TABLE';";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@dbName", database);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tables.Add(new TableInfoDto
                {
                    Name = reader.GetString(0),
                    Comment = reader.IsDBNull(1) ? null : reader.GetString(1),
                    RowCount = reader.IsDBNull(2) ? 0 : reader.GetInt64(2),
                    DataLengthBytes = reader.IsDBNull(3) ? 0 : reader.GetInt64(3)
                });
            }

            return tables;
        }

        public async Task<TableSchemaDto> GetTableSchemaAsync(Connection connection, string database, string table)
        {
            var schema = new TableSchemaDto { TableName = table };
            using var conn = new MySqlConnection(BuildConnectionString(connection, database));
            await conn.OpenAsync();

            // 查詢欄位
            string colQuery = @"
                SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_KEY, COLUMN_DEFAULT, COLUMN_COMMENT
                FROM information_schema.COLUMNS
                WHERE TABLE_SCHEMA = @dbName AND TABLE_NAME = @tableName
                ORDER BY ORDINAL_POSITION;";

            using (var cmd = new MySqlCommand(colQuery, conn))
            {
                cmd.Parameters.AddWithValue("@dbName", database);
                cmd.Parameters.AddWithValue("@tableName", table);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    schema.Columns.Add(new ColumnSchemaDto
                    {
                        Name = reader.GetString(0),
                        DataType = reader.GetString(1),
                        IsNullable = reader.GetString(2).Equals("YES", StringComparison.OrdinalIgnoreCase),
                        IsPrimaryKey = reader.GetString(3).Equals("PRI", StringComparison.OrdinalIgnoreCase),
                        DefaultValue = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Comment = reader.IsDBNull(5) ? null : reader.GetString(5)
                    });
                }
            }

            return schema;
        }

        public async Task<PagedResult<Dictionary<string, object?>>> GetTableDataAsync(Connection connection, string database, string table, int page, int pageSize)
        {
            var rows = new List<Dictionary<string, object?>>();
            long totalCount = await GetTableCountAsync(connection, database, table);

            int offset = (page - 1) * pageSize;
            using var conn = new MySqlConnection(BuildConnectionString(connection, database));
            await conn.OpenAsync();

            string query = $"SELECT * FROM `{table}` LIMIT @pageSize OFFSET @offset;";
            using var cmd = new MySqlCommand(query, conn);
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
            using var conn = new MySqlConnection(BuildConnectionString(connection, database));
            await conn.OpenAsync();

            string query = $"SELECT COUNT(*) FROM `{table}`;";
            using var cmd = new MySqlCommand(query, conn);
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt64(result);
        }

        public BackupCommand GetBackupCommand(Connection connection, string database, List<string>? tables, string outputPath)
        {
            var rawPassword = _cryptoService.Decrypt(connection.EncryptedPassword);
            var port = connection.Port > 0 ? connection.Port : 3306;

            var sb = new StringBuilder();
            sb.Append($"-h \"{connection.Host}\" -P {port} -u \"{connection.Username}\" ");

            if (tables != null && tables.Count > 0)
            {
                sb.Append($"\"{database}\" ");
                foreach (var t in tables)
                {
                    sb.Append($"\"{t}\" ");
                }
            }
            else
            {
                sb.Append($"\"{database}\" ");
            }

            sb.Append($"--result-file=\"{outputPath}\"");

            var envVars = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(rawPassword))
            {
                envVars["MYSQL_PWD"] = rawPassword;
            }

            return new BackupCommand
            {
                Executable = "mysqldump",
                Arguments = sb.ToString(),
                EnvironmentVariables = envVars,
                OutputFilePath = outputPath
            };
        }

        public BackupCommand GetRestoreCommand(Connection connection, string database, string inputPath)
        {
            var rawPassword = _cryptoService.Decrypt(connection.EncryptedPassword);
            var port = connection.Port > 0 ? connection.Port : 3306;

            var args = $"-h \"{connection.Host}\" -P {port} -u \"{connection.Username}\" \"{database}\" -e \"source {inputPath}\"";

            var envVars = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(rawPassword))
            {
                envVars["MYSQL_PWD"] = rawPassword;
            }

            return new BackupCommand
            {
                Executable = "mysql",
                Arguments = args,
                EnvironmentVariables = envVars,
                OutputFilePath = inputPath
            };
        }
    }
}
