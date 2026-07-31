using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using MyNavicat.Api.Models.Common;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Models.Entities;

namespace MyNavicat.Api.Providers
{
    public class SqliteProvider : IDbProvider
    {
        public string DbType => "SQLite";

        public string BuildConnectionString(Connection connection, string? databaseOverride = null)
        {
            var dbPath = databaseOverride ?? connection.DatabaseName;
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = string.IsNullOrEmpty(dbPath) ? "app.db" : dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate
            };

            return builder.ConnectionString;
        }

        public async Task<bool> TestConnectionAsync(Connection connection)
        {
            try
            {
                using var conn = new SqliteConnection(BuildConnectionString(connection));
                await conn.OpenAsync();
                return conn.State == ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }

        public Task<List<string>> GetDatabasesAsync(Connection connection)
        {
            var name = string.IsNullOrEmpty(connection.DatabaseName) ? "main" : Path.GetFileName(connection.DatabaseName);
            return Task.FromResult(new List<string> { name });
        }

        public async Task<List<TableInfoDto>> GetTablesAsync(Connection connection, string database)
        {
            var tables = new List<TableInfoDto>();
            using var conn = new SqliteConnection(BuildConnectionString(connection));
            await conn.OpenAsync();

            string query = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";
            using var cmd = new SqliteCommand(query, conn);
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
            using var conn = new SqliteConnection(BuildConnectionString(connection));
            await conn.OpenAsync();

            string query = $"PRAGMA table_info('{table}');";
            using var cmd = new SqliteCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                schema.Columns.Add(new ColumnSchemaDto
                {
                    Name = reader.GetString(1),
                    DataType = reader.GetString(2),
                    IsNullable = reader.GetInt32(3) == 0,
                    IsPrimaryKey = reader.GetInt32(5) == 1
                });
            }

            return schema;
        }

        public async Task<PagedResult<Dictionary<string, object?>>> GetTableDataAsync(Connection connection, string database, string table, int page, int pageSize)
        {
            var rows = new List<Dictionary<string, object?>>();
            long totalCount = await GetTableCountAsync(connection, database, table);
            int offset = (page - 1) * pageSize;

            using var conn = new SqliteConnection(BuildConnectionString(connection));
            await conn.OpenAsync();

            string query = $"SELECT * FROM \"{table}\" LIMIT @pageSize OFFSET @offset;";
            using var cmd = new SqliteCommand(query, conn);
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
            using var conn = new SqliteConnection(BuildConnectionString(connection));
            await conn.OpenAsync();

            string query = $"SELECT COUNT(*) FROM \"{table}\";";
            using var cmd = new SqliteCommand(query, conn);
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt64(result);
        }

        public BackupCommand GetBackupCommand(Connection connection, string database, List<string>? tables, string outputPath)
        {
            var dbPath = connection.DatabaseName;
            var args = $"\"{dbPath}\" \".dump\" > \"{outputPath}\"";

            return new BackupCommand
            {
                Executable = "sqlite3",
                Arguments = args,
                OutputFilePath = outputPath
            };
        }

        public BackupCommand GetRestoreCommand(Connection connection, string database, string inputPath)
        {
            var dbPath = connection.DatabaseName;
            var args = $"\"{dbPath}\" \".read '{inputPath}'\"";

            return new BackupCommand
            {
                Executable = "sqlite3",
                Arguments = args,
                OutputFilePath = inputPath
            };
        }
    }
}
