using System.ComponentModel.DataAnnotations;

namespace MyNavicat.Api.Models.DTOs
{
    public class ExportRequestDto
    {
        [Required]
        public int ConnectionId { get; set; }

        [Required]
        public string DatabaseName { get; set; } = string.Empty;

        [Required]
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// 匯出格式: CSV / JSON / SQL
        /// </summary>
        [Required]
        public string Format { get; set; } = "CSV";
    }

    public class ImportResultDto
    {
        public int SuccessRows { get; set; }
        public int FailedRows { get; set; }
        public string? ErrorSummary { get; set; }
    }
}
