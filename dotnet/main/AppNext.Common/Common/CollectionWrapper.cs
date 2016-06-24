using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AppBoot.Common
{
    /// <summary> 
    /// Wraps a collection with element type of <typeparamref name="TSource"/> 
    /// to a collection with element type of <typeparamref name="TTarget"/>. 
    /// </summary>
    /// <typeparam name="TSource"> The source element type. </typeparam>
    /// <typeparam name="TTarget"> The target element type. </typeparam>
    /// <remarks> 
    /// The wrapper does not own elements actually, 
    /// all element operations will be delegated to the source collection. 
    /// </remarks>
    public class CollectionWrapper<TSource, TTarget> : ICollection<TTarget> where TSource : TTarget
    {
        public CollectionWrapper(ICollection<TSource> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            m_Source = source;
        }

        private ICollection<TSource> m_Source;

        public ICollection<TSource> Source
        {
            get { return m_Source; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                m_Source = value;
            }
        }

        public IEnumerator<TTarget> GetEnumerator()
        {
            return this.Source.Cast<TTarget>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TTarget item)
        {
            this.Source.Add((TSource) item);
        }

        public void Clear()
        {
            this.Source.Clear();
        }

        public bool Contains(TTarget item)
        {
            return this.Source.Contains((TSource) item);
        }

        public void CopyTo(TTarget[] array, int arrayIndex)
        {
            this.Source.Cast<TTarget>().ToList().CopyTo(array, arrayIndex);
        }

        public bool Remove(TTarget item)
        {
            return this.Source.Remove((TSource) item);
        }

        public int Count
        {
            get { return this.Source.Count; }
        }

        public bool IsReadOnly
        {
            get { return this.Source.IsReadOnly; }
        }
    }
}
