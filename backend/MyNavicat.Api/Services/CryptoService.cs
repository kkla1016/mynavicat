using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MyNavicat.Api.Services
{
    public interface ICryptoService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }

    /// <summary>
    /// AES-256 加解密服務實作
    /// </summary>
    public class CryptoService : ICryptoService
    {
        // 確保 Key 為 32 bytes (256 bits), IV 為 16 bytes (128 bits)
        private static readonly byte[] Key = SHA256.HashData(Encoding.UTF8.GetBytes("MyNavicatPassKeySecret2026"));
        private static readonly byte[] Iv = MD5.HashData(Encoding.UTF8.GetBytes("MyNavicatIVSecret2026"));

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = Iv;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                using var sw = new StreamWriter(cs, Encoding.UTF8);
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            try
            {
                var buffer = Convert.FromBase64String(cipherText);

                using var aes = Aes.Create();
                aes.Key = Key;
                aes.IV = Iv;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using var ms = new MemoryStream(buffer);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs, Encoding.UTF8);

                return sr.ReadToEnd();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
