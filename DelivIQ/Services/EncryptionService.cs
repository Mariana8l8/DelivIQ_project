using System.Security.Cryptography;
using System.Text;

namespace DelivIQ.Services
{
    public static class EncryptionService
    {
        private static string _key = ""; 

        public static void Configure(string key)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
        }

        public static string Encrypt(string? plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText ?? "";

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_key.PadRight(32).Substring(0, 32));
            aes.IV = new byte[16];

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var bytes = Encoding.UTF8.GetBytes(plainText);

            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string? cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText ?? "";

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_key.PadRight(32).Substring(0, 32));
            aes.IV = new byte[16];

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var bytes = Convert.FromBase64String(cipherText);

            var decrypted = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
