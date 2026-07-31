using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyNavicat.Api.Data;
using MyNavicat.Api.Models.DTOs;
using MyNavicat.Api.Providers;
using MyNavicat.Api.Services;
using Xunit;

namespace MyNavicat.Api.Tests.Services
{
    public class ConnectionServiceTests
    {
        private readonly AppDbContext _db;
        private readonly Mock<ICryptoService> _cryptoMock;
        private readonly Mock<IDbProviderFactory> _factoryMock;
        private readonly ConnectionService _service;

        public ConnectionServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new AppDbContext(options);
            _cryptoMock = new Mock<ICryptoService>();
            _cryptoMock.Setup(c => c.Encrypt(It.IsAny<string>())).Returns<string>(s => "enc_" + s);
            _cryptoMock.Setup(c => c.Decrypt(It.IsAny<string>())).Returns<string>(s => s.Replace("enc_", ""));

            _factoryMock = new Mock<IDbProviderFactory>();

            _service = new ConnectionService(_db, _cryptoMock.Object, _factoryMock.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddConnectionToDatabase()
        {
            // Arrange
            var dto = new ConnectionDto
            {
                Name = "Local MySQL",
                DbType = "MySQL",
                Host = "localhost",
                Port = 3306,
                Username = "root",
                Password = "secret_password"
            };

            // Act
            var created = await _service.CreateAsync(dto);

            // Assert
            created.Should().NotBeNull();
            created.Id.Should().BeGreaterThan(0);
            created.Name.Should().Be("Local MySQL");
            created.Password.Should().Be("********"); // 遮蔽密碼

            var dbEntity = await _db.Connections.FindAsync(created.Id);
            dbEntity.Should().NotBeNull();
            dbEntity!.EncryptedPassword.Should().Be("enc_secret_password");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllConnections()
        {
            // Arrange
            await _service.CreateAsync(new ConnectionDto { Name = "DB 1", DbType = "MySQL" });
            await _service.CreateAsync(new ConnectionDto { Name = "DB 2", DbType = "PostgreSQL" });

            // Act
            var list = await _service.GetAllAsync();

            // Assert
            list.Should().HaveCount(2);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveConnection()
        {
            // Arrange
            var created = await _service.CreateAsync(new ConnectionDto { Name = "To Delete", DbType = "MySQL" });

            // Act
            var success = await _service.DeleteAsync(created.Id);
            var remaining = await _service.GetAllAsync();

            // Assert
            success.Should().BeTrue();
            remaining.Should().BeEmpty();
        }
    }
}
