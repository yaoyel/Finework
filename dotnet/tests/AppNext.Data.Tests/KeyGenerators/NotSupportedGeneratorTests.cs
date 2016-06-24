using System;
using NUnit.Framework;

namespace AppBoot.KeyGenerators
{
    [TestFixture]
    public class NotSupportedKeyGeneratorTests
    {
        [Test]
        public void GenerateKey_throws_NotSupportedException()
        {
            var g = new NotSupportedKeyGenerator<String>();
            Assert.Throws<NotSupportedException>(() => g.GenerateKey());
        }

        [Test]
        public void GenerateKeyAsync_throws_NotSupportedException()
        {
            var g = new NotSupportedKeyGenerator<String>();
            Assert.Throws<NotSupportedException>(async() => await g.GenerateKeyAsync());
        }

        [Test]
        public void Default_is_NotSupportedKeyGenerator()
        {
            Assert.IsInstanceOf<NotSupportedKeyGenerator<String>>(NotSupportedKeyGenerator<String>.Default);
        }
    }
}