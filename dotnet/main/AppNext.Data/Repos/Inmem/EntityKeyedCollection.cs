using System;
using System.Collections.ObjectModel;

namespace AppBoot.Repos.Inmem
{
	/// <summary> Represents a collection of entities. </summary>
	/// <typeparam name="TKey"> The type of entity key. </typeparam>
	/// <typeparam name="T"> The type of entity. </typeparam>
	public class EntityKeyedCollection<TKey, T> : KeyedCollection<TKey, T>
    {
	    public EntityKeyedCollection(Func<T, TKey> keyGetter)
	    {
	        if (keyGetter == null) throw new ArgumentNullException("keyGetter");
	        this.m_KeyGetter = keyGetter;
	    }

	    private readonly Func<T, TKey> m_KeyGetter;

        /// <inheritdoc />
		protected override TKey GetKeyForItem(T item)
		{
			return m_KeyGetter(item);
		}
	}
}