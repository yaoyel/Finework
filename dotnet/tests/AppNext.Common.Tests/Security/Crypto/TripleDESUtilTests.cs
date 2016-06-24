using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.TestCommon;
using NUnit.Framework;

namespace AppBoot.Security.Crypto
{
    internal static class CryptoData
    {
        public static readonly String Text = "abcd3中文2314002340adfkqjweiru";
        public static readonly String Key = "321中文nsfw234#$@#^^&ijid";
        public static readonly String Cipher = "RsIHBF94AasG5Z4ZpBeNsrK14CCOAYUh0xWGXzTDKqASOaFgWf4wlpaeOzPJCp1ZP7Osewl2VKPrbQwin5KvYw==";
    }

    // ReSharper disable once InconsistentNaming
    [TestFixture]
    public class TripleDESUtilTests
    {
        [Test]
        public void EncryptText_DescriptText_are_symmetric()
        {
            String cipher = TripleDESUtil.EncryptText(CryptoData.Text, CryptoData.Key);
            Console.WriteLine($"Cipher: \n{cipher}");
            String decripted = TripleDESUtil.DecryptText(cipher, CryptoData.Key);
            Assert.AreEqual(CryptoData.Text, decripted);
        }

        [Test]
        public void EncryptText_returns_null_when_plainText_is_null()
        {
            Assert.IsNull(TripleDESUtil.EncryptText(null, CryptoData.Key));
            Assert.IsNull(TripleDESUtil.EncryptText(null, null));
        }

        [Test]
        public void EncryptText_returns_Empty_when_plainText_is_Empty()
        {
            Assert.IsEmpty(TripleDESUtil.EncryptText(String.Empty, CryptoData.Key));
            Assert.IsEmpty(TripleDESUtil.EncryptText(String.Empty, null));
        }

        [Test]
        public void EncryptText_returns_plainText_when_key_is_null_or_empty()
        {
            Assert.AreEqual(CryptoData.Text, TripleDESUtil.EncryptText(CryptoData.Text, null));
            Assert.AreEqual(CryptoData.Text, TripleDESUtil.EncryptText(CryptoData.Text, String.Empty));
        }

        [Test]
        public void DecryptTest_returns_null_when_cipherText_is_null()
        {
            Assert.IsNull(TripleDESUtil.DecryptText(null, CryptoData.Key));
            Assert.IsNull(TripleDESUtil.DecryptText(null, null));
        }

        [Test]
        public void EncryptTest_returns_Empty_when_cipherText_is_Empty()
        {
            Assert.IsEmpty(TripleDESUtil.DecryptText(String.Empty, CryptoData.Key));
            Assert.IsEmpty(TripleDESUtil.DecryptText(String.Empty, null));
        }

        [Test]
        public void EncryptTest_returns_cipherText_when_key_is_null_or_empty()
        {
            Assert.AreEqual(CryptoData.Cipher, TripleDESUtil.DecryptText(CryptoData.Cipher, null));
            Assert.AreEqual(CryptoData.Cipher, TripleDESUtil.DecryptText(CryptoData.Cipher, String.Empty));
        }

        [Test]
        public void CreateProvider_throws_when_key_is_null_or_empty()
        {
            Assert.Throws<ArgumentNullException>(() => TripleDESUtil.CreateProvider(null)).ForArg("key");
            Assert.Throws<ArgumentNullException>(() => TripleDESUtil.CreateProvider(String.Empty)).ForArg("key");
        }
    }
}
