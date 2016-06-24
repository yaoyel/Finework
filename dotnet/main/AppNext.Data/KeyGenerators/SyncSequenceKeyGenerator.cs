using System;

namespace AppBoot.KeyGenerators
{
    /// <summary> Represents the asynchronized generator for primary key values. </summary>
    public class SyncSequenceKeyGenerator : ISyncKeyGenerator<int>
	{
		/// <summary> Creates an instance. </summary>
		/// <param name="sequenceProvider"> The <see cref="ISyncSequenceProvider{TKey, TValue}"/>.</param>
		/// <param name="name"> The name of the sequence. </param>
		public SyncSequenceKeyGenerator(ISyncSequenceProvider<string, int> sequenceProvider, String name)
		{
			if (sequenceProvider == null) throw new ArgumentNullException("sequenceProvider");
			if (String.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

			this.m_SequenceProvider = sequenceProvider;
			m_Name = name;
		}

		private readonly ISyncSequenceProvider<String, int> m_SequenceProvider;

		private readonly String m_Name;

		/// <seealso cref="ISyncKeyGenerator{TKey}.GenerateKey"/>
		public int GenerateKey()
		{
			return m_SequenceProvider.GetNextValue(m_Name);
		}
	}
}