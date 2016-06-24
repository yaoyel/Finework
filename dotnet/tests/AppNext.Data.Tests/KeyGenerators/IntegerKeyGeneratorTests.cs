using NUnit.Framework;

namespace AppBoot.KeyGenerators
{
    [TestFixture]
    public class IntegerKeyGeneratorTests
    {
        [Test]
        public void GenerateKey_starts_with_One()
        {
            var g = new IntegerKeyGenerator();
            Assert.AreEqual(1, g.GenerateKey());
        }

        [Test]
        public void GeneratorKey_starts_with_the_specified_number()
        {
            const int startNumber = 10;
            var g = new IntegerKeyGenerator(startNumber);
            Assert.AreEqual(startNumber, g.GenerateKey());
            Assert.AreEqual(startNumber + 1, g.GenerateKey());
        }

        [Test]
        public void GenerateKeyAsync_starts_with_One()
        {
            var g = new IntegerKeyGenerator();
            Assert.AreEqual(1, g.GenerateKeyAsync().Result);
        }

        [Test]
        public void GeneratorKeyAsync_starts_with_the_specified_number()
        {
            const int startNumber = 10;
            var g = new IntegerKeyGenerator(startNumber);
            Assert.AreEqual(startNumber, g.GenerateKeyAsync().Result);
            Assert.AreEqual(startNumber + 1, g.GenerateKeyAsync().Result);
        }
    }
}
