using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace MyNavicat.Api.Hubs
{
    /// <summary>
    /// SignalR 通知 Hub，用於即時推送備份完成與失敗訊息至前端
    /// </summary>
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
