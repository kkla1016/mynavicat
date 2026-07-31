using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyNavicat.Api.Models.Entities
{
    /// <summary>
    /// 備份歷史記錄實體類別
    /// </summary>
    [Table("BackupHistories")]
    public class BackupHistory
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 連線 ID
        /// </summary>
        [Required]
        public int ConnectionId { get; set; }

        /// <summary>
        /// 父備份 ID (用於分片備份)
        /// </summary>
        public int? ParentBackupId { get; set; }

        /// <summary>
        /// 備份的資料庫名稱
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string DatabaseName { get; set; } = string.Empty;

        /// <summary>
        /// 備份的資料表 JSON
        /// </summary>
        public string? Tables { get; set; }

        /// <summary>
        /// 備份類型 (Full / Partial / Chunked)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string BackupType { get; set; } = "Full";

        /// <summary>
        /// 備份檔案路徑
        /// </summary>
        [Required]
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 檔案大小 (bytes)
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 是否已壓縮
        /// </summary>
        public bool IsCompressed { get; set; }

        /// <summary>
        /// 壓縮類型 (gz / zip)
        /// </summary>
        [MaxLength(20)]
        public string? CompressionType { get; set; }

        /// <summary>
        /// 分片索引 (0-based)
        /// </summary>
        public int? ChunkIndex { get; set; }

        /// <summary>
        /// 分片總數
        /// </summary>
        public int? TotalChunks { get; set; }

        /// <summary>
        /// 狀態 (Success / Failed / InProgress)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "InProgress";

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 來源排程 ID
        /// </summary>
        public int? ScheduleId { get; set; }

        /// <summary>
        /// 開始時間
        /// </summary>
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 完成時間
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// 備份耗時 (秒)
        /// </summary>
        public double DurationSeconds { get; set; }

        // 導覽屬性
        [ForeignKey(nameof(ConnectionId))]
        public virtual Connection? Connection { get; set; }

        [ForeignKey(nameof(ParentBackupId))]
        public virtual BackupHistory? ParentBackup { get; set; }

        public virtual ICollection<BackupHistory> Chunks { get; set; } = new List<BackupHistory>();
    }
}
