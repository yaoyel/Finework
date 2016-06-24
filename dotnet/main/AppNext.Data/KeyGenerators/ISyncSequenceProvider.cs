namespace AppBoot.KeyGenerators
{
    /// <summary> Represents a synchronized sequence provider. </summary>
    /// <typeparam name="TKey"> The key type of all sequences. </typeparam>
    /// <typeparam name="TValue"> The value type of all sequences. </typeparam>
    public interface ISyncSequenceProvider<in TKey, TValue>
	{
        /// <summary> Adds a sequence. </summary>
        /// <param name="id"> The key of a sequence. </param>
        /// <param name="value"> The initial value of a sequence. </param>
        void AddKey(TKey id, TValue value);

        /// <summary> Removes a sequence. </summary>
        /// <param name="id"> The key of a sequence. </param>
        void RemoveKey(TKey id);

        /// <summary> Sets the naxt value for a sequence. </summary>
        /// <param name="id"> The key for a sequence. </param>
        /// <returns> The value of a sequence. </returns>
        TValue GetNextValue(TKey id);

        /// <summary> Sets the naxt value for a sequence. </summary>
        /// <param name="id"> The key for a sequence. </param>
        /// <param name="value"> The next value of a sequence. 
        /// It will be used as the return value for the next call of <see cref="GetNextValue"/>. </param>
        void SetNextValue(TKey id, TValue value);
	}
}