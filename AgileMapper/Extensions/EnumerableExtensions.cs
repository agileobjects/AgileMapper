namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class EnumerableExtensions
    {
        public static bool None<T>(this ICollection<T> items) => items.Count == 0;

        public static bool HasOne<T>(this ICollection<T> items) => items.Count == 1;

        public static bool DoesNotContain<T>(this ICollection<T> items, T item) => !items.Contains(item);

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> items, params T[] extraItems)
            => items.Concat(extraItems.AsEnumerable());

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> items) => items.Where(item => item != null);

        public static T[] Prepend<T>(this T[] array, T initialItem)
        {
            var newArray = new T[array.Length + 1];

            array.CopyTo(newArray, 1);

            newArray[0] = initialItem;

            return newArray;
        }

        public static T[] Append<T>(this T[] array, T extraItem)
        {
            var newArray = new T[array.Length + 1];

            array.CopyTo(newArray, 0);

            newArray[array.Length] = extraItem;

            return newArray;
        }

        public static IEnumerable<T> Exclude<T>(this IEnumerable<T> items, IEnumerable<T> excludedItems)
            => (excludedItems != null) ? items.StreamExclude(excludedItems) : items;

        private static IEnumerable<T> StreamExclude<T>(this IEnumerable<T> items, IEnumerable<T> excludedItems)
        {
            int nullItemCount;
            var excludedItemCountsByItem = GetCountsByItem(excludedItems, out nullItemCount);

            foreach (var item in items)
            {
                int count;

                if (excludedItemCountsByItem.TryGetValue(item, out count))
                {
                    if (count > 0)
                    {
                        excludedItemCountsByItem[item] = count - 1;
                        continue;
                    }
                }

                yield return item;
            }
        }

        private static Dictionary<T, int> GetCountsByItem<T>(IEnumerable<T> items, out int nullItemCount)
        {
            var itemCountsByItem = new Dictionary<T, int>();
            nullItemCount = 0;

            foreach (var item in items)
            {
                if (item == null)
                {
                    ++nullItemCount;
                    continue;
                }

                int count;

                if (itemCountsByItem.TryGetValue(item, out count))
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

        #region ForEach Overloads

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> itemAction)
        {
            foreach (var item in items)
            {
                itemAction.Invoke(item);
            }
        }

        public static void ForEach<T1, T2>(this IEnumerable<Tuple<T1, T2>> items, Action<T1, T2, int> itemAction)
        {
            var i = 0;

            foreach (var tuple in items)
            {
                itemAction.Invoke(tuple.Item1, tuple.Item2, i++);
            }
        }

        #endregion

        // With thanks to http://stackoverflow.com/questions/3093622/generating-all-possible-combinations:
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };

            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(item)
                );
        }
    }
}
