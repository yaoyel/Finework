using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace AppBoot.KeyGenerators
{
    public class AsyncSequenceKeyGeneratorTests
    {
        private const String m_Key = "KnownKey";

        private IAsyncSequenceProvider<String, int> CreateAsyncSequenceProvider()
        {
            var mock = new Mock<IAsyncSequenceProvider<String, int>>();
            int nextValue = 1;
            mock.Setup(o => o.GetNextValueAsync(m_Key)).Returns(() => Task.FromResult(nextValue)).Callback(() => nextValue++);
            mock.Setup(o => o.GetNextValueAsync(It.IsNotIn(m_Key))).Throws<ArgumentException>();
            return mock.Object;
        }

        [Test]
        public async Task AsyncSequenceProvider_returns_sequential_values()
        {
            var sp = CreateAsyncSequenceProvider();
            Assert.AreEqual(1, await sp.GetNextValueAsync(m_Key));
            Assert.AreEqual(2, await sp.GetNextValueAsync(m_Key));
            Assert.AreEqual(3, await sp.GetNextValueAsync(m_Key));
        }

        [Test]
        public void Constructor_throws_when_argument_is_null()
        {
            var sp = CreateAsyncSequenceProvider();
            Assert.Throws<ArgumentNullException>(() => new AsyncSequenceKeyGenerator(null, m_Key));
            Assert.Throws<ArgumentNullException>(() => new AsyncSequenceKeyGenerator(sp, null));
        }

        [Test]
        public async Task GenerateKeyAsync_returns_sequential_values()
        {
            var sp = CreateAsyncSequenceProvider();
            var g = new AsyncSequenceKeyGenerator(sp, m_Key);
            Assert.AreEqual(1, await g.GenerateKeyAsync());
            Assert.AreEqual(2, await g.GenerateKeyAsync());
            Assert.AreEqual(3, await g.GenerateKeyAsync());
        }
    }
}