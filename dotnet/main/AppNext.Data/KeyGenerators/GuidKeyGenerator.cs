using System;
using System.Threading.Tasks;

namespace AppBoot.KeyGenerators
{
    /// <summary> Represents the <see cref="Guid"/> key generator. </summary>
    public sealed class GuidKeyGenerator : ISyncKeyGenerator<Guid>, IAsyncKeyGenerator<Guid>
	{
		/// <seealso cref="ISyncKeyGenerator{TKey}.GenerateKey"/>
		public Guid GenerateKey()
		{
			return Guid.NewGuid();
		}

        public Task<Guid> GenerateKeyAsync()
        {
            return Task.FromResult(this.GenerateKey());
        }

		/// <summary> The default instance. </summary>
        public static readonly GuidKeyGenerator Default = new GuidKeyGenerator();
	}
}
