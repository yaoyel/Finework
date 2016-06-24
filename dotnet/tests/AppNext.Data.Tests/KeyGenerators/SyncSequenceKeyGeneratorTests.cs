using System;
using Moq;
using NUnit.Framework;

namespace AppBoot.KeyGenerators
{
    [TestFixture]
    public class SyncSequenceKeyGeneratorTests
    {
        private const String m_Key = "KnownKey";

        private ISyncSequenceProvider<String, int> CreateSyncSequenceProvider()
        {
            var mock = new Mock<ISyncSequenceProvider<String, int>>();
            int nextValue = 1;
            mock.Setup(o => o.GetNextValue(m_Key)).Returns(() => nextValue).Callback(() => nextValue++);
            mock.Setup(o => o.GetNextValue(It.IsNotIn(m_Key))).Throws<ArgumentException>();
            return mock.Object;
        }

        [Test]
        public void SyncSequenceProvider_returns_sequential_values()
        {
            var sp = CreateSyncSequenceProvider();
            Assert.AreEqual(1, sp.GetNextValue(m_Key));
            Assert.AreEqual(2, sp.GetNextValue(m_Key));
            Assert.AreEqual(3, sp.GetNextValue(m_Key));

            Assert.Throws<ArgumentException>(() => sp.GetNextValue("NoSuchKey"));
        }

        [Test]
        public void Constructor_throws_when_argument_is_null()
        {
            var sp = CreateSyncSequenceProvider();
            Assert.Throws<ArgumentNullException>(() => new SyncSequenceKeyGenerator(null, m_Key));
            Assert.Throws<ArgumentNullException>(() => new SyncSequenceKeyGenerator(sp, null));
        }

        [Test]
        public void GenerateKey_returns_sequential_values()
        {
            var sp = CreateSyncSequenceProvider();
            var g = new SyncSequenceKeyGenerator(sp, m_Key);
            Assert.AreEqual(1, g.GenerateKey());
            Assert.AreEqual(2, g.GenerateKey());
            Assert.AreEqual(3, g.GenerateKey());
        }
    }
}