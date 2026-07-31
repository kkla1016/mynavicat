using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyNavicat.Api.Data;
using MyNavicat.Api.Models.Common;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Models.Entities;
using MyNavicat.Api.Providers;
using MyNavicat.Api.Services;
using Xunit;

namespace MyNavicat.Api.Tests.Services
{
    public class ExportImportServiceTests
    {
        private readonly AppDbContext _db;
        private readonly Mock<IDbProviderFactory> _factoryMock;
        private readonly Mock<IDbProvider> _providerMock;
        private readonly ExportImportService _service;

        public ExportImportServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new AppDbContext(options);
            _providerMock = new Mock<IDbProvider>();
            _factoryMock = new Mock<IDbProviderFactory>();

            _factoryMock.Setup(f => f.GetProvider(It.IsAny<string>())).Returns(_providerMock.Object);

            _service = new ExportImportService(_db, _factoryMock.Object);
        }

        [Fact]
        public async Task ExportTableDataAsync_CSVFormat_ShouldReturnCsvBytes()
        {
            // Arrange
            var conn = new Connection { Name = "Test DB", DbType = "MySQL" };
            _db.Connections.Add(conn);
            await _db.SaveChangesAsync();

            var rows = new List<Dictionary<string, object?>>
            {
                new() { { "id", 1 }, { "name", "Alice" } },
                new() { { "id", 2 }, { "name", "Bob" } }
            };

            var pagedResult = new PagedResult<Dictionary<string, object?>>(rows, 1, 10, 2);
            _providerMock.Setup(p => p.GetTableDataAsync(It.IsAny<Connection>(), "app_db", "users", 1, 10000))
                         .ReturnsAsync(pagedResult);

            var req = new ExportRequestDto
            {
                ConnectionId = conn.Id,
                DatabaseName = "app_db",
                TableName = "users",
                Format = "CSV"
            };

            // Act
            var (bytes, contentType, fileName) = await _service.ExportTableDataAsync(req);
            var content = Encoding.UTF8.GetString(bytes);

            // Assert
            contentType.Should().Be("text/csv");
            fileName.Should().Be("users.csv");
            content.Should().Contain("id,name");
            content.Should().Contain("Alice");
            content.Should().Contain("Bob");
        }

        [Fact]
        public async Task ExportTableDataAsync_JSONFormat_ShouldReturnJsonBytes()
        {
            // Arrange
            var conn = new Connection { Name = "Test DB", DbType = "MySQL" };
            _db.Connections.Add(conn);
            await _db.SaveChangesAsync();

            var rows = new List<Dictionary<string, object?>>
            {
                new() { { "id", 1 }, { "name", "Alice" } }
            };

            var pagedResult = new PagedResult<Dictionary<string, object?>>(rows, 1, 10, 1);
            _providerMock.Setup(p => p.GetTableDataAsync(It.IsAny<Connection>(), "app_db", "users", 1, 10000))
                         .ReturnsAsync(pagedResult);

            var req = new ExportRequestDto
            {
                ConnectionId = conn.Id,
                DatabaseName = "app_db",
                TableName = "users",
                Format = "JSON"
            };

            // Act
            var (bytes, contentType, fileName) = await _service.ExportTableDataAsync(req);
            var content = Encoding.UTF8.GetString(bytes);

            // Assert
            contentType.Should().Be("application/json");
            fileName.Should().Be("users.json");
            content.Should().Contain("\"Alice\"");
        }
    }
}
