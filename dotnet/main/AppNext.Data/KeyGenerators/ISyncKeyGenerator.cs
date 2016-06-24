
namespace AppBoot.KeyGenerators
{
    /// <summary> Represents the generator for primary key value. </summary>
    /// <typeparam name="TKey"> The primary key type. </typeparam>
    public interface ISyncKeyGenerator<out TKey>
    {
        /// <summary> Generates a new primary key. </summary>
        /// <remarks> The value should have NO conflict with existing entity key values. </remarks>
        TKey GenerateKey();
    }
}
