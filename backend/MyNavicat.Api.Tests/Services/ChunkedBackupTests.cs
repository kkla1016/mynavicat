using System;
using System.IO;
using System.Linq;
using System.Text;
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
    public class ChunkedBackupTests
    {
        private readonly AppDbContext _db;
        private readonly Mock<IDbProviderFactory> _factoryMock;
        private readonly Mock<IDbProvider> _providerMock;
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly BackupService _service;

        public ChunkedBackupTests()
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
        public async Task ExecuteChunkedBackupAsync_LargeFile_ShouldSplitIntoMultipleChunks()
        {
            // Arrange
            var conn = new Connection { Name = "Big DB", DbType = "MySQL" };
            _db.Connections.Add(conn);
            await _db.SaveChangesAsync();

            // 動態捕獲 GetBackupCommand 中的 outputPath 並在 Process 執行前寫入 2.5MB 資料
            _providerMock.Setup(p => p.GetBackupCommand(It.IsAny<Connection>(), It.IsAny<string>(), null, It.IsAny<string>()))
                         .Returns<Connection, string, System.Collections.Generic.List<string>, string>((c, db, tbls, outputPath) =>
                         {
                             // 事先建立 2.5MB 資料至 outputPath
                             var dummyData = new string('A', 2500 * 1024);
                             File.WriteAllText(outputPath, dummyData, Encoding.UTF8);

                             return new Models.Common.BackupCommand
                             {
                                 Executable = "powershell",
                                 Arguments = "-Command Write-Output Ready",
                                 OutputFilePath = outputPath
                             };
                         });

            var request = new BackupRequestDto
            {
                ConnectionId = conn.Id,
                DatabaseName = "large_db",
                Compress = false
            };

            // Act (設定 ChunkSize 為 1MB = 1024 * 1024 bytes)
            var result = await _service.ExecuteChunkedBackupAsync(request, 1024 * 1024);

            // Assert
            result.BackupType.Should().Be("Chunked");
            result.TotalChunks.Should().BeGreaterThan(1);

            var chunkHistories = await _db.BackupHistories.Where(h => h.ParentBackupId == result.Id).ToListAsync();
            chunkHistories.Should().HaveCount(result.TotalChunks!.Value);

            // 清理測試檔案
            foreach (var chunk in chunkHistories)
            {
                if (File.Exists(chunk.FilePath)) File.Delete(chunk.FilePath);
            }
        }
    }
}
