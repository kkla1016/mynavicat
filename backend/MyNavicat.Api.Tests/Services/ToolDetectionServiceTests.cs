using System.Threading.Tasks;
using FluentAssertions;
using MyNavicat.Api.Services;
using Xunit;

namespace MyNavicat.Api.Tests.Services
{
    public class ToolDetectionServiceTests
    {
        private readonly ToolDetectionService _toolDetectionService;

        public ToolDetectionServiceTests()
        {
            _toolDetectionService = new ToolDetectionService();
        }

        [Fact]
        public async Task DetectToolsAsync_ShouldReturnStatusListForRegisteredTools()
        {
            // Act
            var tools = await _toolDetectionService.DetectToolsAsync();

            // Assert
            tools.Should().NotBeNull();
            tools.Should().HaveCount(5); // mysqldump, pg_dump, sqlcmd, sqlite3, exp
            tools.Should().Contain(t => t.Name == "mysqldump");
            tools.Should().Contain(t => t.Name == "pg_dump");
            tools.Should().Contain(t => t.Name == "sqlcmd");
            tools.Should().Contain(t => t.Name == "sqlite3");
            tools.Should().Contain(t => t.Name == "exp");
        }

        [Theory]
        [InlineData("non_existent_tool_xyz_123")]
        public void IsToolAvailable_NonExistentTool_ShouldReturnFalse(string toolName)
        {
            // Act
            var result = _toolDetectionService.IsToolAvailable(toolName);

            // Assert
            result.Should().BeFalse();
        }
    }
}
