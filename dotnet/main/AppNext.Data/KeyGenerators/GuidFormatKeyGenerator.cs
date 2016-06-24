using System;
using System.Globalization;
using System.Threading.Tasks;

namespace AppBoot.KeyGenerators
{
    /// <summary> Represents the <see cref="string"/> key generator 
    /// of which its value is converted from a <see cref="Guid"/>. </summary>
    public sealed class GuidFormatKeyGenerator : ISyncKeyGenerator<String>, IAsyncKeyGenerator<String>
    {
        public GuidFormatKeyGenerator()
            : this("D")
        {
        }

        public GuidFormatKeyGenerator(String format)
        {
            var temp = String.IsNullOrEmpty(format) ? "D" : format;
            if (Array.IndexOf(m_ValidFormats, temp) == -1)
            {
                throw new ArgumentException(String.Format("Invalid format: [{0}]", format));
            }

            m_Format = temp;
        }

        private static readonly String[] m_ValidFormats = new[] { "D", "N", "B", "P", "X" };

        private readonly String m_Format;

        public String Format
        {
            get { return m_Format; }
        }

        public String GenerateKey()
        {
            return Guid.NewGuid().ToString(Format, CultureInfo.InvariantCulture);
        }

        public Task<String> GenerateKeyAsync()
        {
            return Task.FromResult(this.GenerateKey());
        }

        public static readonly GuidFormatKeyGenerator Default = new GuidFormatKeyGenerator();
    }
}