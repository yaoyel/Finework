using System.Threading.Tasks;

namespace AppBoot.KeyGenerators
{
    public interface IAsyncKeyGenerator<TKey>
    {
        /// <summary> Generates a new primary key. </summary>
        /// <remarks> The value should have NO conflict with existing entity key values. </remarks>
        Task<TKey> GenerateKeyAsync();
    }
}