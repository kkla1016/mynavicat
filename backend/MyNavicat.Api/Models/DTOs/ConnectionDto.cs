using System;
using System.ComponentModel.DataAnnotations;

namespace MyNavicat.Api.Models.DTOs
{
    public class ConnectionDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "連線名稱為必填")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "資料庫類型為必填")]
        public string DbType { get; set; } = string.Empty;

        public string Host { get; set; } = "localhost";
        public int Port { get; set; }
        public string DatabaseName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// 密碼 (僅在建立/更新時傳入，GET 讀取時遮蔽為 ********)
        /// </summary>
        public string Password { get; set; } = string.Empty;

        public string? SshHost { get; set; }
        public int? SshPort { get; set; }
        public string? SshUsername { get; set; }
        public string? SshKeyPath { get; set; }
        public string? ExtraParams { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
