using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyNavicat.Api.Models.Entities
{
    /// <summary>
    /// 備份排程實體類別
    /// </summary>
    [Table("BackupSchedules")]
    public class BackupSchedule
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 連線 ID
        /// </summary>
        [Required]
        public int ConnectionId { get; set; }

        /// <summary>
        /// 要備份的資料庫名稱
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string DatabaseName { get; set; } = string.Empty;

        /// <summary>
        /// 要備份的資料表 JSON (null 表示全庫)
        /// </summary>
        public string? Tables { get; set; }

        /// <summary>
        /// Cron 表達式
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string CronExpression { get; set; } = string.Empty;

        /// <summary>
        /// 排程描述
        /// </summary>
        [MaxLength(255)]
        public string? Description { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 是否壓縮備份檔
        /// </summary>
        public bool CompressBackup { get; set; } = true;

        /// <summary>
        /// 壓縮類型 (gz / zip)
        /// </summary>
        [MaxLength(20)]
        public string CompressionType { get; set; } = "gz";

        /// <summary>
        /// 保留份數 (0 表示無限)
        /// </summary>
        public int RetainCount { get; set; } = 10;

        /// <summary>
        /// HangfireJobId
        /// </summary>
        [MaxLength(100)]
        public string? HangfireJobId { get; set; }

        /// <summary>
        /// 上次執行時間
        /// </summary>
        public DateTime? LastRunAt { get; set; }

        /// <summary>
        /// 下次預計執行時間
        /// </summary>
        public DateTime? NextRunAt { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 導覽屬性
        [ForeignKey(nameof(ConnectionId))]
        public virtual Connection? Connection { get; set; }
    }
}
