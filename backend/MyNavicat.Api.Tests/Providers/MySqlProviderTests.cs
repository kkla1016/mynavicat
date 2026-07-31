using System.Collections.Generic;
using FluentAssertions;
using Moq;
using MyNavicat.Api.Models.Entities;
using MyNavicat.Api.Providers;
using MyNavicat.Api.Services;
using Xunit;

namespace MyNavicat.Api.Tests.Providers
{
    public class MySqlProviderTests
    {
        private readonly Mock<ICryptoService> _cryptoMock;
        private readonly MySqlProvider _provider;

        public MySqlProviderTests()
        {
            _cryptoMock = new Mock<ICryptoService>();
            _cryptoMock.Setup(c => c.Decrypt(It.IsAny<string>())).Returns("decrypted_pwd123");
            _provider = new MySqlProvider(_cryptoMock.Object);
        }

        [Fact]
        public void DbType_ShouldBeMySQL()
        {
            _provider.DbType.Should().Be("MySQL");
        }

        [Fact]
        public void BuildConnectionString_ShouldContainHostAndUser()
        {
            var conn = new Connection
            {
                Host = "192.168.1.100",
                Port = 3306,
                Username = "root",
                EncryptedPassword = "encrypted_pwd",
                DatabaseName = "test_db"
            };

            var cs = _provider.BuildConnectionString(conn);

            cs.Should().Contain("Server=192.168.1.100");
            cs.Should().Contain("User ID=root");
            cs.Should().Contain("Password=decrypted_pwd123");
            cs.Should().Contain("Database=test_db");
        }

        [Fact]
        public void GetBackupCommand_FullBackup_ShouldGenerateCorrectExecutableAndArgs()
        {
            var conn = new Connection
            {
                Host = "localhost",
                Port = 3306,
                Username = "admin",
                EncryptedPassword = "encrypted_pwd"
            };

            var cmd = _provider.GetBackupCommand(conn, "my_database", null, "C:/backups/backup.sql");

            cmd.Executable.Should().Be("mysqldump");
            cmd.Arguments.Should().Contain("-h \"localhost\"");
            cmd.Arguments.Should().Contain("-u \"admin\"");
            cmd.Arguments.Should().Contain("\"my_database\"");
            cmd.EnvironmentVariables.Should().ContainKey("MYSQL_PWD");
            cmd.EnvironmentVariables["MYSQL_PWD"].Should().Be("decrypted_pwd123");
        }

        [Fact]
        public void GetBackupCommand_TablePartialBackup_ShouldIncludeTableNames()
        {
            var conn = new Connection
            {
                Host = "localhost",
                Port = 3306,
                Username = "admin",
                EncryptedPassword = "encrypted_pwd"
            };
            var tables = new List<string> { "users", "orders" };

            var cmd = _provider.GetBackupCommand(conn, "my_database", tables, "C:/backups/backup.sql");

            cmd.Arguments.Should().Contain("\"users\"");
            cmd.Arguments.Should().Contain("\"orders\"");
        }
    }
}
