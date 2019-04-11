namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Internal;

    /// <summary>
    /// Provides mapping-related extension methods for enumerables.
    /// </summary>
    public static class PublicEnumerableExtensions
    {
        /// <summary>
        /// Project these <paramref name="items"/> to a new enumerable of type <typeparamref name="TResult"/>,
        /// using the given <paramref name="projector"/>.
        /// </summary>
        /// <typeparam name="TItem">The type of object stored in the enumerable.</typeparam>
        /// <typeparam name="TResult">
        /// The type of object to which each item in the enumerable will be projected.
        /// </typeparam>
        /// <param name="items">The items to project.</param>
        /// <param name="projector">A Func with which to project each item in the enumerable.</param>
        /// <returns>An iterator to transform this enumerable.</returns>
        [DebuggerStepThrough]
        public static IEnumerable<TResult> Project<TItem, TResult>(this IEnumerable<TItem> items, Func<TItem, TResult> projector)
        {
            foreach (var item in items)
            {
                yield return projector.Invoke(item);
            }
        }

        /// <summary>
        /// Project these <paramref name="items"/> to a new enumerable of type <typeparamref name="TResult"/>,
        /// using the given <paramref name="projector"/>.
        /// </summary>
        /// <typeparam name="TItem">The type of object stored in the enumerable.</typeparam>
        /// <typeparam name="TResult">
        /// The type of object to which each item in the enumerable will be projected.
        /// </typeparam>
        /// <param name="items">The items to project.</param>
        /// <param name="projector">A Func with which to project each item in the enumerable.</param>
        /// <returns>An iterator to transform this enumerable.</returns>
        [DebuggerStepThrough]
        public static IEnumerable<TResult> Project<TItem, TResult>(this IEnumerable<TItem> items, Func<TItem, int, TResult> projector)
        {
            var index = 0;

            foreach (var item in items)
            {
                yield return projector.Invoke(item, index++);
            }
        }

        /// <summary>
        /// Filter these <paramref name="items"/> using the given <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="TItem">The Type of object stored in this enumerable.</typeparam>
        /// <param name="items">The items to filter.</param>
        /// <param name="predicate">The predicate with which to filter these items.</param>
        /// <returns>
        /// An enumerator yielding the <paramref name="items"/> which pass the given <paramref name="predicate"/>.
        /// </returns>
        [DebuggerStepThrough]
        public static IEnumerable<TItem> Filter<TItem>(this IEnumerable<TItem> items, Func<TItem, bool> predicate)
        {
            foreach (var item in items)
            {
                if (predicate.Invoke(item))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Exclude the given <paramref name="excludedItems"/> from these <paramref name="items"/>, in a
        /// repeated-item-aware manner.
        /// </summary>
        /// <typeparam name="T">The type of items stored in the enumerable.</typeparam>
        /// <param name="items">
        /// The items from which the <paramref name="excludedItems"/> should be excluded.
        /// </param>
        /// <param name="excludedItems">The items to exclude.</param>
        /// <returns>
        /// This set of <paramref name="items"/>, with the given <paramref name="excludedItems"/> excluded.
        /// </returns>
        public static IEnumerable<T> Exclude<T>(this IEnumerable<T> items, IEnumerable<T> excludedItems)
            => (excludedItems != null) ? items.StreamExclude(excludedItems) : items;

        private static IEnumerable<T> StreamExclude<T>(this IEnumerable<T> items, IEnumerable<T> excludedItems)
        {
            Dictionary<T, int> excludedItemCountsByItem = null;

            foreach (var item in items)
            {
                if (excludedItemCountsByItem == null)
                {
                    // ReSharper disable once PossibleMultipleEnumeration
                    excludedItemCountsByItem = GetCountsByItem(excludedItems);
                }

                if (excludedItemCountsByItem.TryGetValue(item, out var count) && (count > 0))
                {
                    excludedItemCountsByItem[item] = count - 1;
                    continue;
                }

                yield return item;
            }
        }

        private static Dictionary<T, int> GetCountsByItem<T>(IEnumerable<T> items)
        {
            var itemCountsByItem = new Dictionary<T, int>();

            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                if (itemCountsByItem.TryGetValue(item, out var count))
                {
                    itemCountsByItem[item] = count + 1;
                }
                else
                {
                    itemCountsByItem.Add(item, 1);
                }
            }

            return itemCountsByItem;
        }

        /// <summary>
        /// Determines if none of the objects in these <paramref name="items"/> return true when passed to
        /// the given <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="T">The Type of object stored in this enumerable.</typeparam>
        /// <param name="items">The items for which to make the determination.</param>
        /// <param name="predicate">The predicate to apply to each of the items.</param>
        /// <returns>
        /// True if none of these <paramref name="items"/> return true when passed to the given
        /// <paramref name="predicate"/>, otherwise false.
        /// </returns>
        public static bool None<T>(this IEnumerable<T> items, Func<T, bool> predicate)
            => items.All(item => !predicate.Invoke(item));

        /// <summary>
        /// Copies this list of <paramref name="items"/> into a new array.
        /// </summary>
        /// <typeparam name="T">The type of object stored in the list.</typeparam>
        /// <param name="items">The list of items to convert.</param>
        /// <returns>This list of items, converted to an array.</returns>
        public static T[] ToArray<T>(this IList<T> items) => items.CopyToArray();

        /// <summary>
        /// Copies this collection of <paramref name="items"/> into a new array, or returns this
        /// object if it is an array.
        /// </summary>
        /// <typeparam name="T">The type of object stored in the list.</typeparam>
        /// <param name="items">The collection of items to convert.</param>
        /// <returns>This collection of items, converted to an array.</returns>
        public static T[] ToArray<T>(this ICollection<T> items)
        {
            if (items is T[] array)
            {
                return array;
            }

            array = new T[items.Count];

            items.CopyTo(array, 0);

            return array;
        }

        #region ForEach Overloads

        /// <summary>
        /// Iterate these <paramref name="items"/>, executing the given <paramref name="itemAction"/> on each.
        /// </summary>
        /// <typeparam name="T">The type of object stored in the enumerable.</typeparam>
        /// <param name="items">The enumerable to iterate.</param>
        /// <param name="itemAction">The action to execute on each item in the enumerable.</param>
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> itemAction)
        {
            foreach (var item in items)
            {
                itemAction.Invoke(item);
            }
        }

        /// <summary>
        /// Iterate these <paramref name="items"/>, executing the given <paramref name="itemAction"/> on each.
        /// </summary>
        /// <typeparam name="T1">The type of the first object stored in the enumerable.</typeparam>
        /// <typeparam name="T2">The type of the second object stored in the enumerable.</typeparam>
        /// <param name="items">The enumerable to iterate.</param>
        /// <param name="itemAction">The action to execute on each item in the enumerable.</param>
        public static void ForEach<T1, T2>(this IEnumerable<Tuple<T1, T2>> items, Action<T1, T2, int> itemAction)
        {
            var i = 0;

            foreach (var tuple in items)
            {
                itemAction.Invoke(tuple.Item1, tuple.Item2, i++);
            }
        }

        #endregion
    }
}
