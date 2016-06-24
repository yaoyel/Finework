using System;
using System.Collections.Generic;

namespace AppBoot.Common
{
    /// <summary> A set of utilities for handling collections. </summary>
    public static class CollectionUtil
    {
        #region AddAll

        /// <summary> Adds elements to a collection from a given source. </summary>
        /// <typeparam name="T"> the element type. </typeparam>
        /// <param name="collection"> the collection. </param>
        /// <param name="source"> the source elements. </param>
        public static void AddAll<T>(this ICollection<T> collection, IEnumerable<T> source)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            if (source == null) throw new ArgumentNullException("source");

            foreach (var item in source)
            {
                collection.Add(item);
            }
        }

        #endregion

        #region ArrayEquals

        /// <summary> Determinates if two arrays are same. </summary>
        /// <typeparam name="T"> The element type. </typeparam>
        /// <param name="a1"> the first array. </param>
        /// <param name="a2"> the second array. </param>
        /// <returns> <c>true</c> if two arrays have the same elements in the same order, otherwise <c>false</c>. </returns>
        public static bool ArrayEquals<T>(T[] a1, T[] a2)
        {
            return ArrayEquals(a1, a2, EqualityComparer<T>.Default);
        }

        /// <summary> Determinates if two arrays are same. </summary>
        /// <typeparam name="T"> The element type. </typeparam>
        /// <param name="a1"> the first array. </param>
        /// <param name="a2"> the second array. </param>
        /// <param name="elementComparer"> the comparer for comparing array elements. </param>
        /// <returns> <c>true</c> if two arrays have the same elements in the same order, otherwise <c>false</c>. </returns>
        public static bool ArrayEquals<T>(T[] a1, T[] a2, IEqualityComparer<T> elementComparer)
        {
            if (elementComparer == null) throw new ArgumentNullException("elementComparer");

            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
            {
                if (!elementComparer.Equals(a1[i], a2[i])) return false;
            }
            return true;
        }

        #endregion
    }
}
