using System;
using System.Collections.Generic;

namespace AppBoot.Repos.Core
{
    /// <summary> Represents the collection used in 
    /// defining the entity with a pair of declaraction/implementation types. </summary>
    /// <typeparam name="TImpl"> The entity implementation type. </typeparam>
    /// <typeparam name="TDecl"> The entity declaraction type. </typeparam>
    public class UpCastSet<TImpl, TDecl> : HashSet<TImpl>, ICollection<TDecl> where TImpl : TDecl
    {
        /// <summary> Creates an empty set. </summary>
        public UpCastSet()
            : base(EqualityComparer<TImpl>.Default)
        {
        }

        /// <summary> Creates a set with a collection of elements. </summary>
        public UpCastSet(IEnumerable<TImpl> collection)
            : this(collection, EqualityComparer<TImpl>.Default)
        {
        }

        /// <summary> Creates a set with a collection of elements and a given comparer. </summary>
        public UpCastSet(IEnumerable<TImpl> collection, IEqualityComparer<TImpl> comparer)
            : base(collection, comparer)
        {
        }

        /// <summary> Clears all existing elements and then adds all elements from a given collection. </summary>
        /// <param name="collection"> The collection of new elements, may be <c>null</c>. </param>
        public void ResetItems(IEnumerable<TImpl> collection)
        {
            this.Clear();
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    this.Add(item);
                }
            }
            this.TrimExcess();
        }

        #region ICollection<TDecl> explicit implementations

        IEnumerator<TDecl> IEnumerable<TDecl>.GetEnumerator()
        {
            IEnumerator<TImpl> er = this.GetEnumerator();
            while (er.MoveNext())
            {
                yield return er.Current;
            }
        }

        void ICollection<TDecl>.Add(TDecl item)
        {
            this.Add((TImpl)item);
        }

        bool ICollection<TDecl>.Contains(TDecl item)
        {
            return Contains((TImpl)item);
        }

        void ICollection<TDecl>.CopyTo(TDecl[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException("arrayIndex");
            if (arrayIndex + this.Count > array.Length) throw new ArgumentException("No enough space.", "arrayIndex");

            int index = arrayIndex;
            using (IEnumerator<TDecl> er = ((ICollection<TDecl>)this).GetEnumerator())
            {
                while (er.MoveNext())
                {
                    array[index] = er.Current;
                    index++;
                }
            }
        }

        bool ICollection<TDecl>.Remove(TDecl item)
        {
            return this.Remove((TImpl)item);
        }

        bool ICollection<TDecl>.IsReadOnly
        {
            get { return false; }
        }

        #endregion
    }
}
