using System;
using System.Security.Cryptography;
using System.Text;

namespace AppBoot.Security.Crypto
{
    // ReSharper disable once InconsistentNaming
    public static class TripleDESUtil
    {
        /// <summary> 加密字符串. </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥, 当密钥为 <c>null</c> 或 <see cref="String.Empty"/> 时返回明文</param>
        /// <returns>密文的 Base64 编码</returns>
        /// <remarks>根据明文加密得到的密文会再以 Base64 加以编码.</remarks>
        public static String EncryptText(String plainText, String key)
        {
            if (String.IsNullOrEmpty(plainText)) return plainText;
            if (String.IsNullOrEmpty(key)) return plainText;

            var provider = CreateProvider(key);

            byte[] buffer = CryptoUtil.Encrypt(plainText, provider.CreateEncryptor());
            return Convert.ToBase64String(buffer);
        }

        /// <summary> 解密字符串. </summary>
        /// <param name="cipherText">密文</param>
        /// <param name="key">密钥, 当密钥为 <c>null</c> 或 <see cref="String.Empty"/> 时返回密文</param>
        /// <returns>明文</returns>
        /// <remarks><paramref name="cipherText"/> 应当是由 <see cref="EncryptText"/> 输出的密文.</remarks>
        public static String DecryptText(String cipherText, String key)
        {
            if (String.IsNullOrEmpty(cipherText)) return cipherText;
            if (String.IsNullOrEmpty(key)) return cipherText;

            var provider = CreateProvider(key);

            byte[] buffer = Convert.FromBase64String(cipherText);
            return CryptoUtil.Decrypt(buffer, provider.CreateDecryptor());
        }

        /// <summary> for 3DES, LegalSizes: MinSize 128-bit, MaxSize 192-bit, SkipSize 64. </summary>
        private const int m_KeyBytes = 16;

        /// <summary> for 3DES, LeagalBlockSize: MinSize 64-bit, MaxSize 64-bit, SkipSize 0. </summary>
        private const int m_IVBytes = 8;

        public static TripleDESCryptoServiceProvider CreateProvider(String key)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException("key");

            Byte[] bytes = Encoding.ASCII.GetBytes(key);

            var result = new TripleDESCryptoServiceProvider();

            Array.Resize(ref bytes, m_KeyBytes);
            result.Key = bytes;

            Array.Resize(ref bytes, m_IVBytes);
            result.IV = bytes;

            return result;
        }
    }
}