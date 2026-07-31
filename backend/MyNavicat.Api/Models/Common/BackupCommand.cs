using System.Collections.Generic;

namespace MyNavicat.Api.Models.Common
{
    /// <summary>
    /// CLI 備份/還原命令模型
    /// </summary>
    public class BackupCommand
    {
        public string Executable { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new();
        public string OutputFilePath { get; set; } = string.Empty;
    }
}
