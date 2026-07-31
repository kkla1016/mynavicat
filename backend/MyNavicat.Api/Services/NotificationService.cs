using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MyNavicat.Api.Hubs;
using MyNavicat.Api.Models.Entities;

namespace MyNavicat.Api.Services
{
    public interface INotificationService
    {
        Task SendBackupCompletedAsync(int scheduleId, BackupHistory history);
        Task SendBackupFailedAsync(int scheduleId, string errorMessage);
    }

    /// <summary>
    /// 即時通知服務實作 (SignalR Hub 廣播)
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendBackupCompletedAsync(int scheduleId, BackupHistory history)
        {
            var payload = new
            {
                scheduleId,
                historyId = history.Id,
                databaseName = history.DatabaseName,
                fileSize = history.FileSize,
                completedAt = history.CompletedAt,
                durationSeconds = history.DurationSeconds,
                message = $"排程備份成功！資料庫: {history.DatabaseName}"
            };

            await _hubContext.Clients.All.SendAsync("BackupCompleted", payload);
        }

        public async Task SendBackupFailedAsync(int scheduleId, string errorMessage)
        {
            var payload = new
            {
                scheduleId,
                errorMessage,
                message = $"排程備份失敗: {errorMessage}"
            };

            await _hubContext.Clients.All.SendAsync("BackupFailed", payload);
        }
    }
}
