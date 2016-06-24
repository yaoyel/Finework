using System;
using System.Threading.Tasks;

namespace AppBoot.KeyGenerators
{
    /// <summary> Represents a key generator that indicates the functionality is not supported. </summary>
    /// <typeparam name="TKey"> The type of key. </typeparam>
	public class NotSupportedKeyGenerator<TKey> : ISyncKeyGenerator<TKey>, IAsyncKeyGenerator<TKey>
    {
		/// <exception cref="NotSupportedException"> The call to this method 
		/// always throws <see cref="NotSupportedException"/>. </exception>
		public TKey GenerateKey()
		{
			throw new NotSupportedException();
		}

        public Task<TKey> GenerateKeyAsync()
        {
            throw new NotSupportedException();
        }

		/// <summary> Creates the default instance. </summary>
        public static readonly NotSupportedKeyGenerator<TKey> Default = new NotSupportedKeyGenerator<TKey>();
	}
}