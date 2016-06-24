using System.Threading;
using System.Threading.Tasks;

namespace AppBoot.KeyGenerators
{
    /// <summary> Represents the <see cref="int"/> key generator. </summary>
    public class IntegerKeyGenerator : ISyncKeyGenerator<int>, IAsyncKeyGenerator<int>
    {
        public IntegerKeyGenerator()
            : this(1)
        {
        }

        public IntegerKeyGenerator(int nextKey)
        {
            this.m_NextKey = nextKey - 1;
        }

        private int m_NextKey;

        public int GenerateKey()
        {
            return Interlocked.Increment(ref m_NextKey);
        }

        public Task<int> GenerateKeyAsync()
        {
            return Task.FromResult(this.GenerateKey());
        }
    }
}