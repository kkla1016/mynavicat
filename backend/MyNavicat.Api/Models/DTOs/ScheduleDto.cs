using System;
using System.ComponentModel.DataAnnotations;

namespace MyNavicat.Api.Models.DTOs
{
    public class ScheduleDto
    {
        public int Id { get; set; }

        [Required]
        public int ConnectionId { get; set; }

        [Required]
        public string DatabaseName { get; set; } = string.Empty;

        public string? Tables { get; set; }

        [Required]
        public string CronExpression { get; set; } = "0 0 * * *"; // 預設每天零點

        public string? Description { get; set; }

        public bool IsEnabled { get; set; } = true;

        public bool CompressBackup { get; set; } = true;

        public string CompressionType { get; set; } = "gz";

        public int RetainCount { get; set; } = 10;

        public string? HangfireJobId { get; set; }

        public DateTime? LastRunAt { get; set; }

        public DateTime? NextRunAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string? ConnectionName { get; set; }
    }
}
