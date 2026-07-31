using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MyNavicat.Api.Models.Common;

namespace MyNavicat.Api.Services
{
    public interface IToolDetectionService
    {
        Task<List<ToolStatus>> DetectToolsAsync();
        bool IsToolAvailable(string toolName);
    }

    /// <summary>
    /// 資料庫 CLI 工具偵測服務實作
    /// </summary>
    public class ToolDetectionService : IToolDetectionService
    {
        private static readonly List<(string Name, string DisplayName, string TargetDb, string VersionArg, string InstallUrl)> RegisteredTools = new()
        {
            ("mysqldump", "MySQL Dump Tool", "MySQL", "--version", "https://dev.mysql.com/doc/refman/8.0/en/mysqldump.html"),
            ("pg_dump", "PostgreSQL Dump Tool", "PostgreSQL", "--version", "https://www.postgresql.org/docs/current/app-pgdump.html"),
            ("sqlcmd", "SQL Server Command Line Utility", "SqlServer", "-?", "https://learn.microsoft.com/en-us/sql/tools/sqlcmd/sqlcmd-utility"),
            ("sqlite3", "SQLite3 Command Line Interface", "SQLite", "-version", "https://www.sqlite.org/cli.html"),
            ("exp", "Oracle Export Utility", "Oracle", "-help", "https://docs.oracle.com/en/database/oracle/oracle-database/19/xeaiw/index.html")
        };

        public async Task<List<ToolStatus>> DetectToolsAsync()
        {
            var results = new List<ToolStatus>();

            foreach (var tool in RegisteredTools)
            {
                var status = await CheckToolAsync(tool.Name, tool.DisplayName, tool.TargetDb, tool.VersionArg, tool.InstallUrl);
                results.Add(status);
            }

            return results;
        }

        public bool IsToolAvailable(string toolName)
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = toolName,
                        Arguments = "--version",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                return process.Start();
            }
            catch
            {
                return false;
            }
        }

        private async Task<ToolStatus> CheckToolAsync(string name, string displayName, string targetDb, string versionArg, string installUrl)
        {
            var status = new ToolStatus
            {
                Name = name,
                DisplayName = displayName,
                TargetDbType = targetDb,
                InstallGuideUrl = installUrl,
                IsAvailable = false,
                Version = null
            };

            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = name,
                        Arguments = versionArg,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                if (process.Start())
                {
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    string fullText = string.IsNullOrWhiteSpace(output) ? error : output;

                    if (!string.IsNullOrWhiteSpace(fullText))
                    {
                        var firstLine = fullText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0];
                        status.Version = firstLine.Length > 100 ? firstLine.Substring(0, 100) : firstLine;
                        status.IsAvailable = true;
                    }
                    else
                    {
                        status.IsAvailable = true;
                        status.Version = "Installed (Version unknown)";
                    }
                }
            }
            catch
            {
                status.IsAvailable = false;
                status.Version = null;
            }

            return status;
        }
    }
}
