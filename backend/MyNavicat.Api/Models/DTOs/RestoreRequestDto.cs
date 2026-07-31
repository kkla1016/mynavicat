using System.ComponentModel.DataAnnotations;

namespace MyNavicat.Api.Models.DTOs
{
    public class RestoreRequestDto
    {
        public int? BackupHistoryId { get; set; }

        [Required]
        public int TargetConnectionId { get; set; }

        [Required]
        public string TargetDatabaseName { get; set; } = string.Empty;
    }
}
