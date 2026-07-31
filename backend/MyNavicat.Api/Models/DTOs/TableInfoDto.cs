namespace MyNavicat.Api.Models.DTOs
{
    public class TableInfoDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public long RowCount { get; set; }
        public long DataLengthBytes { get; set; }
    }
}
