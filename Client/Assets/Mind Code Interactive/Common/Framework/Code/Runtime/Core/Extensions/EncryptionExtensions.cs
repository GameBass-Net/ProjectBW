/// <summary>
/// Project : Mind Code Interactive
/// Class : EncryptionExtensions.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions
{
    public static class EncryptionExtensions
    {
        public static byte[] Encrypt(this byte[] data, string key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = DeriveKey(key);
                aes.GenerateIV();

                using (MemoryStream output = new MemoryStream())
                {
                    output.Write(aes.IV, 0, aes.IV.Length);

                    using (CryptoStream crypto = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        crypto.Write(data, 0, data.Length);
                        crypto.FlushFinalBlock();
                    }

                    return output.ToArray();
                }
            }
        }

        public static byte[] Decrypt(this byte[] data, string key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = DeriveKey(key);

                byte[] iv = new byte[16];
                Array.Copy(data, 0, iv, 0, 16);
                aes.IV = iv;

                using (MemoryStream input = new MemoryStream(data, 16, data.Length - 16))
                using (CryptoStream crypto = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (MemoryStream output = new MemoryStream())
                {
                    crypto.CopyTo(output);
                    return output.ToArray();
                }
            }
        }

        private static byte[] DeriveKey(string key)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return sha.ComputeHash(Encoding.UTF8.GetBytes(key));
            }
        }
    }
}
