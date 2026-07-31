namespace MyNavicat.Api.Models.Common
{
    /// <summary>
    /// CLI 工具狀態模型
    /// </summary>
    public class ToolStatus
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string TargetDbType { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string? Version { get; set; }
        public string InstallGuideUrl { get; set; } = string.Empty;
    }
}
