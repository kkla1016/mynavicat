using FluentAssertions;
using MyNavicat.Api.Services;
using Xunit;

namespace MyNavicat.Api.Tests.Services
{
    public class CryptoServiceTests
    {
        private readonly CryptoService _cryptoService;

        public CryptoServiceTests()
        {
            _cryptoService = new CryptoService();
        }

        [Theory]
        [InlineData("SimplePassword123")]
        [InlineData("P@ssw0rd!#$&*()_+")]
        [InlineData("中文測試密碼123!")]
        [InlineData("VeryLongPasswordWithMultipleWordsAndSymbols!@#$%^&*()_+1234567890")]
        public void Encrypt_And_Decrypt_ShouldReturnOriginalText(string originalText)
        {
            // Act
            var encrypted = _cryptoService.Encrypt(originalText);
            var decrypted = _cryptoService.Decrypt(encrypted);

            // Assert
            encrypted.Should().NotBeNullOrEmpty();
            encrypted.Should().NotBe(originalText);
            decrypted.Should().Be(originalText);
        }

        [Fact]
        public void Encrypt_EmptyString_ShouldReturnEmpty()
        {
            // Act
            var encrypted = _cryptoService.Encrypt(string.Empty);
            var decrypted = _cryptoService.Decrypt(string.Empty);

            // Assert
            encrypted.Should().BeEmpty();
            decrypted.Should().BeEmpty();
        }

        [Fact]
        public void Decrypt_InvalidBase64_ShouldReturnEmpty()
        {
            // Act
            var decrypted = _cryptoService.Decrypt("InvalidBase64String!!!");

            // Assert
            decrypted.Should().BeEmpty();
        }
    }
}
