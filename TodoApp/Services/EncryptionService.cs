using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TodoApp.Services
{
    public static class EncryptionService
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("TodoAppStorageKeyForAes256Data!!");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("TodoAppInitVect!");

        public static byte[] Encrypt(string value)
        {
            using var output = new MemoryStream();
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            using (var cryptoStream = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var writer = new StreamWriter(cryptoStream, Encoding.UTF8))
            {
                writer.Write(value);
            }

            return output.ToArray();
        }

        public static string Decrypt(byte[] value)
        {
            using var input = new MemoryStream(value);
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            using var cryptoStream = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var reader = new StreamReader(cryptoStream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}
