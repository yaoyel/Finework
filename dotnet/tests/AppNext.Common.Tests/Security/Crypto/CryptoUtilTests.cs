using System;
using System.Security.Cryptography;
using AppBoot.Common;
using AppBoot.TestCommon;
using NUnit.Framework;

namespace AppBoot.Security.Crypto
{
    [TestFixture]
    public class CryptoUtilTests
    {
        /// <summary> 验证 <see cref="TripleDESUtil.EncryptText"/> 加密得到的密文可以被 <see cref="TripleDESUtil.DecryptText"/>解密. </summary>
        [Test]
        public void EncryptText_can_be_DecryptText()
        {
            String text = "abcd3中文2314002340adfkqjweiru";
            String key = "321中文nsfw234#$@#^^&ijid";

            CheckEncryptedTextIsDecryptable(text, key);
            CheckEncryptedTextIsDecryptable("a", "b");
        }

        /// <summary> 验证当明文为 <c>null</c> 或 <see cref="string.Empty"/> 时, <see cref="TripleDESUtil.EncryptText"/> 直接返回明文. </summary>
        [Test]
        public void EncryptText_returns_PlainText_for_NullOrEmpty()
        {
            Assert.That(TripleDESUtil.EncryptText(null, null), Is.Null);
            Assert.That(TripleDESUtil.EncryptText(String.Empty, null), Is.EqualTo(String.Empty));
        }

        /// <summary> 验证当密文为 <c>null</c> 或 <see cref="string.Empty"/> 时, <see cref="TripleDESUtil.DecryptText"/> 直接返回密文. </summary>
        [Test]
        public void DecryptText_returns_PlainText_for_NullOrEmpty()
        {
            Assert.That(TripleDESUtil.DecryptText(null, null), Is.Null);
            Assert.That(TripleDESUtil.DecryptText(String.Empty, null), Is.EqualTo(String.Empty));
        }

        /// <summary> 验证解密后的文本和原文本相同. </summary>
        private void CheckEncryptedTextIsDecryptable(String text, String key)
        {
            String cipher = TripleDESUtil.EncryptText(text, key);
            String decripted = TripleDESUtil.DecryptText(cipher, key);
            Assert.That(decripted, Is.EqualTo(text));
        }

        /// <summary> 当 <see cref="CryptoUtil.CalculateHash"/> 的 <c>hashArgorithm</c> 为<c>null</c>时抛出异常. </summary>
        [Test]
        public void CalculateHash_throws_when_hashArgorithm_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => CryptoUtil.CalculateHash(null, "SomString"))
                .ForArg("hashAlgorithm");
        }

        /// <summary> 验证对于相同的字符串, <see cref="CryptoUtil.CalculateHash"/> 返回相同的结果. </summary>
        [Test]
        public void CalculateHash_returns_SameHash_for_SameText()
        {
            String text = "132401ajdfjq3nnfa&*&^#(#)";

            CompareHash(SHA1.Create(), text);
            CompareHash(MD5.Create(), text);
            CompareHash(SHA256.Create(), text);
        }

        /// <summary> 当 <see cref="CryptoUtil.CreateRandomBytes"/> 的 <c>length</c> 参数小于零时抛出异常. </summary>
        [Test]
        public void CreateRandomBytes_throws_when_length_EqualOrLessThan_zero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CryptoUtil.CreateRandomBytes(0)).ForArg("length");
            Assert.Throws<ArgumentOutOfRangeException>(() => CryptoUtil.CreateRandomBytes(-1)).ForArg("length");
        }

        /// <summary> <see cref="CryptoUtil.CreateRandomBytes"/> 返回随机值. </summary>
        [Test]
        public void CreateRandomBytes_returns_RandomBytes()
        {
            var x = CryptoUtil.CreateRandomBytes(10);
            var y = CryptoUtil.CreateRandomBytes(10);

            Assert.IsFalse(CollectionUtil.ArrayEquals(x, y));
        }

        /// <summary> 当 <see cref="CryptoUtil.CreateRandomText"/> 的 <c>length</c> 参数小于零时抛出异常. </summary>
        [Test]
        public void CreateRandomText_throws_when_length_EqualOrLessThan_zero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CryptoUtil.CreateRandomText(0)).ForArg("length");
            Assert.Throws<ArgumentOutOfRangeException>(() => CryptoUtil.CreateRandomText(-1)).ForArg("length");
        }

        /// <summary> <see cref="CryptoUtil.CreateRandomText"/> 返回随机值. </summary>
        [Test]
        public void CreateRandomText_returns_RandomText()
        {
            var x = CryptoUtil.CreateRandomText(10);
            var y = CryptoUtil.CreateRandomText(10);

            Assert.AreNotEqual(x, y);
        }


        private void CompareHash(HashAlgorithm hashAlgorithm, String text)
        {
            String a = CryptoUtil.CalculateHash(hashAlgorithm, text);
            String b = CryptoUtil.CalculateHash(hashAlgorithm, text);
            Assert.AreEqual(a, b);
        }
    }
}
