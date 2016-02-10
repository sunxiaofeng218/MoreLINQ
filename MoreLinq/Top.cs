#region License and Terms
// MoreLINQ - Extensions to LINQ to Objects
// Copyright (c) 2008 Jonathan Skeet. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

namespace MoreLinq
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    static partial class MoreEnumerable
    {
        /// <summary>
        /// Combines <see cref="Enumerable.OrderBy{TSource,TKey}(IEnumerable{TSource},Func{TSource,TKey})"/>,
        /// where each element is its key, and <see cref="Enumerable.Take{TSource}"/>
        /// in a single operation.
        /// </summary>
        /// <typeparam name="T">Type of elements in the sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="count">Number of (maximum) elements to return.</param>
        /// <returns>A sequence containing at most top <paramref name="count"/>
        /// elements from source, in their ascending order.</returns>
        /// <remarks>
        /// This operator uses deferred execution and streams it results.
        /// </remarks>

        public static IEnumerable<T> Top<T>(this IEnumerable<T> source, int count)
        {
            return source.Top(count, null);
        }

        /// <summary>
        /// Combines <see cref="Enumerable.OrderBy{TSource,TKey}(IEnumerable{TSource},Func{TSource,TKey},IComparer{TKey})"/>,
        /// where each element is its key, and <see cref="Enumerable.Take{TSource}"/>
        /// in a single operation. An additional parameter specifies how the
        /// elements compare to each other.
        /// </summary>
        /// <typeparam name="T">Type of elements in the sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="count">Number of (maximum) elements to return.</param>
        /// <param name="comparer">A <see cref="IComparer{T}"/> to compare elements.</param>
        /// <returns>A sequence containing at most top <paramref name="count"/>
        /// elements from source, in their ascending order.</returns>
        /// <remarks>
        /// This operator uses deferred execution and streams it results.
        /// </remarks>

        public static IEnumerable<T> Top<T>(this IEnumerable<T> source,
            int count, IComparer<T> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            return TopByImpl<T, T>(source, count, null, null, comparer);
        }

        /// <summary>
        /// Combines <see cref="Enumerable.OrderBy{TSource,TKey}(IEnumerable{TSource},Func{TSource,TKey},IComparer{TKey})"/>,
        /// and <see cref="Enumerable.Take{TSource}"/> in a single operation.
        /// </summary>
        /// <typeparam name="TSource">Type of elements in the sequence.</typeparam>
        /// <typeparam name="TKey">Type of keys.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="count">Number of (maximum) elements to return.</param>
        /// <returns>A sequence containing at most top <paramref name="count"/>
        /// elements from source, in ascending order of their keys.</returns>
        /// <remarks>
        /// This operator uses deferred execution and streams it results.
        /// </remarks>

        public static IEnumerable<TSource> TopBy<TSource, TKey>(
            this IEnumerable<TSource> source, int count,
            Func<TSource, TKey> keySelector)
        {
            return source.TopBy(count, keySelector, null);
        }

        /// <summary>
        /// Combines <see cref="Enumerable.OrderBy{TSource,TKey}(IEnumerable{TSource},Func{TSource,TKey},IComparer{TKey})"/>,
        /// and <see cref="Enumerable.Take{TSource}"/> in a single operation.
        /// An additional parameter specifies how the keys compare to each other.
        /// </summary>
        /// <typeparam name="TSource">Type of elements in the sequence.</typeparam>
        /// <typeparam name="TKey">Type of keys.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <param name="count">Number of (maximum) elements to return.</param>
        /// <param name="comparer">A <see cref="IComparer{T}"/> to compare elements.</param>
        /// <returns>A sequence containing at most top <paramref name="count"/>
        /// elements from source, in ascending order of their keys.</returns>
        /// <remarks>
        /// This operator uses deferred execution and streams it results.
        /// </remarks>

        public static IEnumerable<TSource> TopBy<TSource, TKey>(
            this IEnumerable<TSource> source, int count,
            Func<TSource, TKey> keySelector,
            IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (keySelector == null) throw new ArgumentNullException("keySelector");
            return TopByImpl(source, count, keySelector, comparer, null);
        }

        static IEnumerable<TSource> TopByImpl<TSource, TKey>(
            IEnumerable<TSource> source, int count,
            Func<TSource, TKey> keySelector,
            IComparer<TKey> keyComparer, IComparer<TSource> comparer)
        {
            Debug.Assert(source != null);

            var keys = keySelector != null ? new List<TKey>(count) : null;
            var top = new List<TSource>(count);

            foreach (var item in source)
            {
                int i;
                var key = default(TKey);
                if (keys != null)
                {
                    key = keySelector(item);
                    i = keys.BinarySearch(key, keyComparer);
                }
                else
                {
                    i = top.BinarySearch(item, comparer);
                }

                if (i < 0 && (i = ~i) >= count)
                    continue;

                if (top.Count == count)
                {
                    if (keys != null)
                        keys.RemoveAt(top.Count - 1);

                    top.RemoveAt(top.Count - 1);
                }

                // TODO Stable sorting

                if (keys != null)
                    keys.Insert(i, key);

                top.Insert(i, item);
            }

            // ReSharper disable once LoopCanBeConvertedToQuery

            foreach (var item in top)
                yield return item;
        }
    }
}
