using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyNavicat.Api.Models.DTOs
{
    public class BackupRequestDto
    {
        [Required]
        public int ConnectionId { get; set; }

        [Required]
        public string DatabaseName { get; set; } = string.Empty;

        public List<string>? Tables { get; set; }

        public bool Compress { get; set; } = true;

        public string CompressionType { get; set; } = "gz"; // gz / zip
    }
}
