using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MyNavicat.Api.Models.Common;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Models.Entities;

namespace MyNavicat.Api.Providers
{
    /// <summary>
    /// 各資料庫 Provider 統一介面
    /// </summary>
    public interface IDbProvider
    {
        /// <summary>
        /// 支援的資料庫類型
        /// </summary>
        string DbType { get; }

        /// <summary>
        /// 測試資料庫連線
        /// </summary>
        Task<bool> TestConnectionAsync(Connection connection);

        /// <summary>
        /// 取得所有資料庫清單
        /// </summary>
        Task<List<string>> GetDatabasesAsync(Connection connection);

        /// <summary>
        /// 取得資料庫中的所有資料表資訊
        /// </summary>
        Task<List<TableInfoDto>> GetTablesAsync(Connection connection, string database);

        /// <summary>
        /// 取得資料表 Schema 結構
        /// </summary>
        Task<TableSchemaDto> GetTableSchemaAsync(Connection connection, string database, string table);

        /// <summary>
        /// 取得資料表資料 (分頁)
        /// </summary>
        Task<PagedResult<Dictionary<string, object?>>> GetTableDataAsync(Connection connection, string database, string table, int page, int pageSize);

        /// <summary>
        /// 取得資料表筆數
        /// </summary>
        Task<long> GetTableCountAsync(Connection connection, string database, string table);

        /// <summary>
        /// 產生備份 CLI 命令資訊
        /// </summary>
        BackupCommand GetBackupCommand(Connection connection, string database, List<string>? tables, string outputPath);

        /// <summary>
        /// 產生還原 CLI 命令資訊
        /// </summary>
        BackupCommand GetRestoreCommand(Connection connection, string database, string inputPath);
    }
}
