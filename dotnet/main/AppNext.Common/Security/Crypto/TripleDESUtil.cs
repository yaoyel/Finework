using System;
using System.Security.Cryptography;
using System.Text;

namespace AppBoot.Security.Crypto
{
    // ReSharper disable once InconsistentNaming
    public static class TripleDESUtil
    {
        /// <summary> �����ַ���. </summary>
        /// <param name="plainText">����</param>
        /// <param name="key">��Կ, ����ԿΪ <c>null</c> �� <see cref="String.Empty"/> ʱ��������</param>
        /// <returns>���ĵ� Base64 ����</returns>
        /// <remarks>�������ļ��ܵõ������Ļ����� Base64 ���Ա���.</remarks>
        public static String EncryptText(String plainText, String key)
        {
            if (String.IsNullOrEmpty(plainText)) return plainText;
            if (String.IsNullOrEmpty(key)) return plainText;

            var provider = CreateProvider(key);

            byte[] buffer = CryptoUtil.Encrypt(plainText, provider.CreateEncryptor());
            return Convert.ToBase64String(buffer);
        }

        /// <summary> �����ַ���. </summary>
        /// <param name="cipherText">����</param>
        /// <param name="key">��Կ, ����ԿΪ <c>null</c> �� <see cref="String.Empty"/> ʱ��������</param>
        /// <returns>����</returns>
        /// <remarks><paramref name="cipherText"/> Ӧ������ <see cref="EncryptText"/> ���������.</remarks>
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