using System.Collections.Generic;
using System.Threading.Tasks;
using MyNavicat.Api.Models.Common;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Models.Entities;
using MyNavicat.Api.Services;

namespace MyNavicat.Api.Providers
{
    public class MariaDbProvider : IDbProvider
    {
        private readonly MySqlProvider _innerProvider;

        public MariaDbProvider(ICryptoService cryptoService)
        {
            _innerProvider = new MySqlProvider(cryptoService);
        }

        public string DbType => "MariaDB";

        public Task<bool> TestConnectionAsync(Connection connection) => _innerProvider.TestConnectionAsync(connection);

        public Task<List<string>> GetDatabasesAsync(Connection connection) => _innerProvider.GetDatabasesAsync(connection);

        public Task<List<TableInfoDto>> GetTablesAsync(Connection connection, string database) => _innerProvider.GetTablesAsync(connection, database);

        public Task<TableSchemaDto> GetTableSchemaAsync(Connection connection, string database, string table) => _innerProvider.GetTableSchemaAsync(connection, database, table);

        public Task<PagedResult<Dictionary<string, object?>>> GetTableDataAsync(Connection connection, string database, string table, int page, int pageSize) => _innerProvider.GetTableDataAsync(connection, database, table, page, pageSize);

        public Task<long> GetTableCountAsync(Connection connection, string database, string table) => _innerProvider.GetTableCountAsync(connection, database, table);

        public BackupCommand GetBackupCommand(Connection connection, string database, List<string>? tables, string outputPath)
        {
            var cmd = _innerProvider.GetBackupCommand(connection, database, tables, outputPath);
            cmd.Executable = "mariadb-dump"; // 預設使用 mariadb-dump
            return cmd;
        }

        public BackupCommand GetRestoreCommand(Connection connection, string database, string inputPath)
        {
            var cmd = _innerProvider.GetRestoreCommand(connection, database, inputPath);
            cmd.Executable = "mariadb";
            return cmd;
        }
    }
}
