using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Moq;
using MyNavicat.Api.Hubs;
using MyNavicat.Api.Models.Entities;
using MyNavicat.Api.Services;
using Xunit;

namespace MyNavicat.Api.Tests.Services
{
    public class NotificationServiceTests
    {
        private readonly Mock<IHubContext<NotificationHub>> _hubContextMock;
        private readonly Mock<IHubClients> _clientsMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            _hubContextMock = new Mock<IHubContext<NotificationHub>>();
            _clientsMock = new Mock<IHubClients>();
            _clientProxyMock = new Mock<IClientProxy>();

            _clientsMock.Setup(c => c.All).Returns(_clientProxyMock.Object);
            _hubContextMock.Setup(h => h.Clients).Returns(_clientsMock.Object);

            _service = new NotificationService(_hubContextMock.Object);
        }

        [Fact]
        public async Task SendBackupCompletedAsync_ShouldCallSendAsyncOnAllClients()
        {
            // Arrange
            var history = new BackupHistory
            {
                Id = 10,
                DatabaseName = "sales",
                FileSize = 1024
            };

            // Act
            await _service.SendBackupCompletedAsync(1, history);

            // Assert
            _clientProxyMock.Verify(
                p => p.SendCoreAsync("BackupCompleted", It.IsAny<object[]>(), default(CancellationToken)),
                Times.Once);
        }

        [Fact]
        public async Task SendBackupFailedAsync_ShouldCallSendAsyncOnAllClients()
        {
            // Act
            await _service.SendBackupFailedAsync(1, "Connection Error");

            // Assert
            _clientProxyMock.Verify(
                p => p.SendCoreAsync("BackupFailed", It.IsAny<object[]>(), default(CancellationToken)),
                Times.Once);
        }
    }
}
