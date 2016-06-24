using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AppBoot.KeyGenerators
{
    [TestFixture]
    public class GuidKeyGeneratorTests
    {
        [Test]
        public void GenerateKey_returns_Guid()
        {
            var g = new GuidKeyGenerator();
            Assert.IsInstanceOf<Guid>(g.GenerateKey());
            Assert.IsInstanceOf<Guid>(GuidKeyGenerator.Default.GenerateKey());
        }

        [Test]
        public async Task GenerateKeyAsync_returns_Guid()
        {
            var g = new GuidKeyGenerator();
            Assert.IsInstanceOf<Guid>(await g.GenerateKeyAsync());
            Assert.IsInstanceOf<Guid>(await GuidKeyGenerator.Default.GenerateKeyAsync());
        }

        [Test]
        public void Default_is_GuidKeyGenerator()
        {
            Assert.IsInstanceOf<GuidKeyGenerator>(GuidKeyGenerator.Default);
        }
    }
}