using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MyNavicat.Api.Data;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Models.Entities;
using MyNavicat.Api.Providers;
using MyNavicat.Api.Services;
using Xunit;

namespace MyNavicat.Api.Tests.Services
{
    public class BackupServiceTests
    {
        private readonly AppDbContext _db;
        private readonly Mock<IDbProviderFactory> _factoryMock;
        private readonly Mock<IDbProvider> _providerMock;
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly BackupService _service;

        public BackupServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new AppDbContext(options);
            _providerMock = new Mock<IDbProvider>();
            _factoryMock = new Mock<IDbProviderFactory>();
            _loggerMock = new Mock<ILogger<BackupService>>();

            _factoryMock.Setup(f => f.GetProvider(It.IsAny<string>())).Returns(_providerMock.Object);

            _service = new BackupService(_db, _factoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetHistoryAsync_ShouldReturnPagedBackupHistories()
        {
            // Arrange
            var conn = new Connection { Name = "Local DB", DbType = "MySQL" };
            _db.Connections.Add(conn);
            await _db.SaveChangesAsync();

            _db.BackupHistories.Add(new BackupHistory
            {
                ConnectionId = conn.Id,
                DatabaseName = "db1",
                FilePath = "C:/test/file1.sql",
                Status = "Success"
            });
            _db.BackupHistories.Add(new BackupHistory
            {
                ConnectionId = conn.Id,
                DatabaseName = "db2",
                FilePath = "C:/test/file2.sql",
                Status = "Failed"
            });
            await _db.SaveChangesAsync();

            // Act
            var history = await _service.GetHistoryAsync(conn.Id, null, 1, 10);

            // Assert
            history.Items.Should().HaveCount(2);
            history.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task DeleteHistoryAsync_ShouldRemoveRecord()
        {
            // Arrange
            var conn = new Connection { Name = "Local DB", DbType = "MySQL" };
            _db.Connections.Add(conn);
            await _db.SaveChangesAsync();

            var historyItem = new BackupHistory
            {
                ConnectionId = conn.Id,
                DatabaseName = "db1",
                FilePath = "C:/non_existent_file.sql",
                Status = "Success"
            };
            _db.BackupHistories.Add(historyItem);
            await _db.SaveChangesAsync();

            // Act
            var success = await _service.DeleteHistoryAsync(historyItem.Id);
            var remaining = await _db.BackupHistories.ToListAsync();

            // Assert
            success.Should().BeTrue();
            remaining.Should().BeEmpty();
        }
    }
}
