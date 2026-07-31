using System.Collections.Generic;
using FluentAssertions;
using Moq;
using MyNavicat.Api.Providers;
using MyNavicat.Api.Services;
using Xunit;

namespace MyNavicat.Api.Tests.Providers
{
    public class DbProviderFactoryTests
    {
        private readonly DbProviderFactory _factory;

        public DbProviderFactoryTests()
        {
            var cryptoMock = new Mock<ICryptoService>();
            var providers = new List<IDbProvider>
            {
                new MySqlProvider(cryptoMock.Object),
                new PostgreSqlProvider(cryptoMock.Object),
                new SqlServerProvider(cryptoMock.Object),
                new MariaDbProvider(cryptoMock.Object),
                new SqliteProvider(),
                new OracleProvider(cryptoMock.Object)
            };

            _factory = new DbProviderFactory(providers);
        }

        [Theory]
        [InlineData("MySQL", typeof(MySqlProvider))]
        [InlineData("PostgreSQL", typeof(PostgreSqlProvider))]
        [InlineData("SqlServer", typeof(SqlServerProvider))]
        [InlineData("MariaDB", typeof(MariaDbProvider))]
        [InlineData("SQLite", typeof(SqliteProvider))]
        [InlineData("Oracle", typeof(OracleProvider))]
        public void GetProvider_ShouldReturnCorrectProviderInstance(string dbType, System.Type expectedType)
        {
            // Act
            var provider = _factory.GetProvider(dbType);

            // Assert
            provider.Should().BeOfType(expectedType);
        }
    }
}
