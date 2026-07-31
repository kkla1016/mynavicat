using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MyNavicat.Api.Data;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Models.Entities;
using MyNavicat.Api.Services;
using Xunit;

namespace MyNavicat.Api.Tests.Services
{
    public class ScheduleServiceTests
    {
        private readonly AppDbContext _db;
        private readonly Mock<IRecurringJobManager> _jobManagerMock;
        private readonly Mock<IBackupService> _backupServiceMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<ILogger<ScheduleService>> _loggerMock;
        private readonly ScheduleService _service;

        public ScheduleServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new AppDbContext(options);
            _jobManagerMock = new Mock<IRecurringJobManager>();
            _backupServiceMock = new Mock<IBackupService>();
            _notificationServiceMock = new Mock<INotificationService>();
            _loggerMock = new Mock<ILogger<ScheduleService>>();

            // 建立測試用 Connection
            _db.Connections.Add(new Connection { Id = 1, Name = "Test Conn", DbType = "MySQL" });
            _db.SaveChanges();

            _service = new ScheduleService(
                _db,
                _jobManagerMock.Object,
                _backupServiceMock.Object,
                _notificationServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddScheduleToDatabase()
        {
            // Arrange
            var dto = new ScheduleDto
            {
                ConnectionId = 1,
                DatabaseName = "app_db",
                CronExpression = "0 2 * * *",
                IsEnabled = true
            };

            // Act
            var created = await _service.CreateAsync(dto);

            // Assert
            created.Should().NotBeNull();
            created.Id.Should().BeGreaterThan(0);
            created.CronExpression.Should().Be("0 2 * * *");
            created.HangfireJobId.Should().Be($"schedule-{created.Id}");

            var entity = await _db.BackupSchedules.FindAsync(created.Id);
            entity.Should().NotBeNull();
            entity!.DatabaseName.Should().Be("app_db");
        }

        [Fact]
        public async Task ToggleAsync_ShouldToggleIsEnabledState()
        {
            // Arrange
            var created = await _service.CreateAsync(new ScheduleDto
            {
                ConnectionId = 1,
                DatabaseName = "app_db",
                CronExpression = "0 0 * * *",
                IsEnabled = true
            });

            // Act
            var success = await _service.ToggleAsync(created.Id);
            var updated = await _service.GetByIdAsync(created.Id);

            // Assert
            success.Should().BeTrue();
            updated.Should().NotBeNull();
            updated!.IsEnabled.Should().BeFalse();
        }
    }
}
