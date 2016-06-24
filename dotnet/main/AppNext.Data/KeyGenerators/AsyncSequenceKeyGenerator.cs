using System;
using System.Threading.Tasks;

namespace AppBoot.KeyGenerators
{
    /// <summary> Represents the asynchronized generator for primary key values. </summary>
    public class AsyncSequenceKeyGenerator : IAsyncKeyGenerator<int>
    {
        /// <summary> Creates an instance. </summary>
        /// <param name="sequenceProvider"> The <see cref="IAsyncSequenceProvider{TKey, TValue}"/>.</param>
        /// <param name="name"> The name of the sequence. </param>
        public AsyncSequenceKeyGenerator(IAsyncSequenceProvider<string, int> sequenceProvider, String name)
        {
            if (sequenceProvider == null) throw new ArgumentNullException("sequenceProvider");
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

            this.m_SequenceProvider = sequenceProvider;
            m_Name = name;
        }

        private readonly IAsyncSequenceProvider<String, int> m_SequenceProvider;

        private readonly String m_Name;

        /// <seealso cref="IAsyncKeyGenerator{TKey}.GenerateKeyAsync"/>
        public async Task<int> GenerateKeyAsync()
        {
            return await m_SequenceProvider.GetNextValueAsync(m_Name);
        }
    }
}