using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyNavicat.Api.Models.Entities
{
    /// <summary>
    /// 資料庫連線實體類別
    /// </summary>
    [Table("Connections")]
    public class Connection
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 連線名稱
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 資料庫類型 (MySQL, PostgreSQL, SqlServer, MariaDB, SQLite, Oracle)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string DbType { get; set; } = string.Empty;

        /// <summary>
        /// 主機 IP 或位址
        /// </summary>
        [MaxLength(255)]
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// 連接埠
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 預設資料庫名稱
        /// </summary>
        [MaxLength(100)]
        public string DatabaseName { get; set; } = string.Empty;

        /// <summary>
        /// 使用者名稱
        /// </summary>
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// AES 加密密碼
        /// </summary>
        public string EncryptedPassword { get; set; } = string.Empty;

        /// <summary>
        /// SSH 主機
        /// </summary>
        [MaxLength(255)]
        public string? SshHost { get; set; }

        /// <summary>
        /// SSH 連接埠
        /// </summary>
        public int? SshPort { get; set; }

        /// <summary>
        /// SSH 使用者名稱
        /// </summary>
        [MaxLength(100)]
        public string? SshUsername { get; set; }

        /// <summary>
        /// SSH 金鑰檔路徑
        /// </summary>
        public string? SshKeyPath { get; set; }

        /// <summary>
        /// 額外連線參數 JSON
        /// </summary>
        public string? ExtraParams { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
