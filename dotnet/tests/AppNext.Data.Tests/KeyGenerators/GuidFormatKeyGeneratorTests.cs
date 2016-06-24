using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AppBoot.KeyGenerators
{
    [TestFixture]
    public class GuidFormatKeyGeneratorTests
    {
        private readonly String[] m_ActualFormats = new[] { null, "", "D", "N", "B", "P", "X" };
        private readonly String[] m_ExpectFormats = new[] { "D", "D", "D", "N", "B", "P", "X" };

        [Test]
        public void Constructor_throws_when_format_is_invalid()
        {
            Assert.Throws<ArgumentException>(() => new GuidFormatKeyGenerator("errorFormat"));
        }

        [Test]
        public void GenerateKey_returns_formated_Guid()
        {
            for (int i = 0; i < m_ActualFormats.Length; i++)
            {
                var actualFormat = m_ActualFormats[i];
                var expectFormat = m_ExpectFormats[i];


                var g = new GuidFormatKeyGenerator(actualFormat);
                var s = g.GenerateKey();

                var guid = Guid.Parse(s);
                Assert.AreEqual(s, guid.ToString(expectFormat));
            }
        }

        [Test]
        public async Task GenerateKeyAsync_returns_formated_Guid()
        {
            for (int i = 0; i < m_ActualFormats.Length; i++)
            {
                var actualFormat = m_ActualFormats[i];
                var expectFormat = m_ExpectFormats[i];


                var g = new GuidFormatKeyGenerator(actualFormat);
                var s = await g.GenerateKeyAsync();

                var guid = Guid.Parse(s);
                Assert.AreEqual(s, guid.ToString(expectFormat));
            }
        }

        [Test]
        public void Default_is_GuidFormatKeyGenerator()
        {
            Assert.IsInstanceOf<GuidFormatKeyGenerator>(GuidFormatKeyGenerator.Default);
        }
    }
}