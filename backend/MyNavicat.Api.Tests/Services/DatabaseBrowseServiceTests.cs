using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyNavicat.Api.Data;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Models.Entities;
using MyNavicat.Api.Providers;
using MyNavicat.Api.Services;
using Xunit;

namespace MyNavicat.Api.Tests.Services
{
    public class DatabaseBrowseServiceTests
    {
        private readonly AppDbContext _db;
        private readonly Mock<IDbProviderFactory> _factoryMock;
        private readonly Mock<IDbProvider> _providerMock;
        private readonly DatabaseBrowseService _service;

        public DatabaseBrowseServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new AppDbContext(options);
            _providerMock = new Mock<IDbProvider>();
            _factoryMock = new Mock<IDbProviderFactory>();

            _factoryMock.Setup(f => f.GetProvider(It.IsAny<string>())).Returns(_providerMock.Object);

            _service = new DatabaseBrowseService(_db, _factoryMock.Object);
        }

        [Fact]
        public async Task GetDatabasesAsync_ValidConnection_ShouldReturnDatabasesFromProvider()
        {
            // Arrange
            var conn = new Connection { Name = "Test DB", DbType = "MySQL", Host = "localhost" };
            _db.Connections.Add(conn);
            await _db.SaveChangesAsync();

            var expectedDbs = new List<string> { "app_db", "sales_db" };
            _providerMock.Setup(p => p.GetDatabasesAsync(It.IsAny<Connection>())).ReturnsAsync(expectedDbs);

            // Act
            var result = await _service.GetDatabasesAsync(conn.Id);

            // Assert
            result.Should().BeEquivalentTo(expectedDbs);
        }

        [Fact]
        public async Task GetTablesAsync_ValidConnection_ShouldReturnTablesFromProvider()
        {
            // Arrange
            var conn = new Connection { Name = "Test DB", DbType = "MySQL" };
            _db.Connections.Add(conn);
            await _db.SaveChangesAsync();

            var expectedTables = new List<TableInfoDto>
            {
                new TableInfoDto { Name = "users", RowCount = 100 },
                new TableInfoDto { Name = "orders", RowCount = 500 }
            };
            _providerMock.Setup(p => p.GetTablesAsync(It.IsAny<Connection>(), "my_db")).ReturnsAsync(expectedTables);

            // Act
            var result = await _service.GetTablesAsync(conn.Id, "my_db");

            // Assert
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("users");
        }

        [Fact]
        public async Task GetDatabasesAsync_InvalidConnection_ShouldThrowKeyNotFoundException()
        {
            // Act
            Func<Task> act = async () => await _service.GetDatabasesAsync(999);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }
    }
}
