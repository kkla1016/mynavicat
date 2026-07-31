using System.Collections.Generic;
using System.Threading.Tasks;
using MyNavicat.Api.Data;
using MyNavicat.Api.Models.Common;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Providers;

namespace MyNavicat.Api.Services
{
    public interface IDatabaseBrowseService
    {
        Task<List<string>> GetDatabasesAsync(int connectionId);
        Task<List<TableInfoDto>> GetTablesAsync(int connectionId, string database);
        Task<TableSchemaDto> GetTableSchemaAsync(int connectionId, string database, string table);
        Task<PagedResult<Dictionary<string, object?>>> GetTableDataAsync(int connectionId, string database, string table, int page, int pageSize);
        Task<long> GetTableCountAsync(int connectionId, string database, string table);
    }

    /// <summary>
    /// 資料庫結構與內容瀏覽服務實作
    /// </summary>
    public class DatabaseBrowseService : IDatabaseBrowseService
    {
        private readonly AppDbContext _db;
        private readonly IDbProviderFactory _providerFactory;

        public DatabaseBrowseService(AppDbContext db, IDbProviderFactory providerFactory)
        {
            _db = db;
            _providerFactory = providerFactory;
        }

        public async Task<List<string>> GetDatabasesAsync(int connectionId)
        {
            var conn = await GetConnectionEntityAsync(connectionId);
            var provider = _providerFactory.GetProvider(conn.DbType);
            return await provider.GetDatabasesAsync(conn);
        }

        public async Task<List<TableInfoDto>> GetTablesAsync(int connectionId, string database)
        {
            var conn = await GetConnectionEntityAsync(connectionId);
            var provider = _providerFactory.GetProvider(conn.DbType);
            return await provider.GetTablesAsync(conn, database);
        }

        public async Task<TableSchemaDto> GetTableSchemaAsync(int connectionId, string database, string table)
        {
            var conn = await GetConnectionEntityAsync(connectionId);
            var provider = _providerFactory.GetProvider(conn.DbType);
            return await provider.GetTableSchemaAsync(conn, database, table);
        }

        public async Task<PagedResult<Dictionary<string, object?>>> GetTableDataAsync(int connectionId, string database, string table, int page, int pageSize)
        {
            var conn = await GetConnectionEntityAsync(connectionId);
            var provider = _providerFactory.GetProvider(conn.DbType);
            return await provider.GetTableDataAsync(conn, database, table, page, pageSize);
        }

        public async Task<long> GetTableCountAsync(int connectionId, string database, string table)
        {
            var conn = await GetConnectionEntityAsync(connectionId);
            var provider = _providerFactory.GetProvider(conn.DbType);
            return await provider.GetTableCountAsync(conn, database, table);
        }

        private async Task<Models.Entities.Connection> GetConnectionEntityAsync(int connectionId)
        {
            var conn = await _db.Connections.FindAsync(connectionId);
            if (conn == null)
            {
                throw new KeyNotFoundException($"Connection with ID {connectionId} not found.");
            }
            return conn;
        }
    }
}
