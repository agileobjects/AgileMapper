namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class EnumerableExtensions
    {
        public static bool None<T>(this IEnumerable<T> items) => !items.Any();

        public static bool HasOne<T>(this IEnumerable<T> items) => items.Count() == 1;

        public static bool DoesNotContain<T>(this IEnumerable<T> items, T item) => !items.Contains(item);

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> items, params T[] extraItems)
            => items.Concat(extraItems.AsEnumerable());

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> items) => items.Where(item => item != null);

        public static T[] Append<T>(this T[] array, T extraItem)
        {
            var newArray = new T[array.Length + 1];

            array.CopyTo(newArray, 0);

            newArray[array.Length] = extraItem;

            return newArray;
        }

        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> items, T newItem)
        {
            yield return newItem;

            foreach (var item in items)
            {
                yield return item;
            }
        }

        public static T[] SubArray<T>(this T[] sourceArray, int sourceIndex)
        {
            var subArray = new T[sourceArray.Length - sourceIndex];
            Array.Copy(sourceArray, sourceIndex, subArray, 0, subArray.Length);

            return subArray;
        }

        public static IEnumerable<T> Exclude<T>(this IEnumerable<T> items, IEnumerable<T> excludedItems)
        {
            var excludedItemCountsByItem = GetCountsByItem(excludedItems);

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

        public static IEnumerable<TSource> ExcludeById<TSource, TTarget, TId>(
            this IEnumerable<TSource> sourceItems,
            IEnumerable<TTarget> excludedItems,
            Func<TSource, TId> sourceIdFactory,
            Func<TTarget, TId> targetIdFactory)
        {
            var excludedItemsById = GetItemsById(excludedItems, targetIdFactory);
            var excludedItemCountsById = GetCountsByItem(excludedItemsById.Keys);

            foreach (var sourceItem in sourceItems)
            {
                if (sourceItem == null)
                {
                    yield return default(TSource);
                    continue;
                }

                var sourceItemId = sourceIdFactory.Invoke(sourceItem);
                int count;

                if (excludedItemCountsById.TryGetValue(sourceItemId, out count) && (count > 0))
                {
                    excludedItemCountsById[sourceItemId] = count - 1;
                    continue;
                }

                yield return sourceItem;
            }
        }

        private static Dictionary<T, int> GetCountsByItem<T>(IEnumerable<T> items)
        {
            var itemCountsByItem = new Dictionary<T, int>();

            foreach (var item in items)
            {
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

        public static IEnumerable<Tuple<TSource, TTarget>> IntersectById<TSource, TTarget, TId>(
            this IEnumerable<TSource> sourceItems,
            IEnumerable<TTarget> targetItems,
            Func<TSource, TId> sourceIdFactory,
            Func<TTarget, TId> targetIdFactory)
        {
            var targetsById = GetItemsById(targetItems, targetIdFactory);

            foreach (var sourceItem in sourceItems)
            {
                if (sourceItem == null)
                {
                    yield return Tuple.Create(default(TSource), default(TTarget));
                    continue;
                }

                var sourceItemId = sourceIdFactory.Invoke(sourceItem);

                TTarget targetItem;

                if (targetsById.TryGetValue(sourceItemId, out targetItem))
                {
                    yield return Tuple.Create(sourceItem, targetItem);
                }
            }
        }

        private static Dictionary<TId, TItem> GetItemsById<TItem, TId>(IEnumerable<TItem> items, Func<TItem, TId> idFactory)
        {
            return items
                .WhereNotNull()
                .Select(item => new
                {
                    Id = idFactory.Invoke(item),
                    Item = item
                })
                .Where(item => !EqualityComparer<TId>.Default.Equals(item.Id, default(TId)))
                .ToDictionary(d => d.Id, d => d.Item);
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
