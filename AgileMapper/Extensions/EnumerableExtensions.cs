namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class EnumerableExtensions
    {
        public static bool None<T>(this IEnumerable<T> items)
        {
            return !items.Any();
        }

        public static bool HasOne<T>(this IEnumerable<T> items)
        {
            return items.Count() == 1;
        }

        public static bool DoesNotContain<T>(this IEnumerable<T> items, T item)
        {
            return !items.Contains(item);
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> items, params T[] extraItems)
        {
            return items.Concat(extraItems.AsEnumerable());
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
                .Where(item => item != null)
                .Select(item => new
                {
                    Id = idFactory.Invoke(item),
                    Item = item
                })
                .Where(item => !EqualityComparer<TId>.Default.Equals(item.Id, default(TId)))
                .ToDictionary(d => d.Id, d => d.Item);
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T, int> itemAction)
        {
            var i = 0;

            foreach (var item in items)
            {
                itemAction.Invoke(item, i++);
            }
        }

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
