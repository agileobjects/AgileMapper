namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal static class EnumerableExtensions
    {
        public static void AddUnlessNullOrEmpty(this ICollection<Expression> items, Expression item)
        {
            if ((item != null) && (item != Constants.EmptyExpression))
            {
                items.Add(item);
            }
        }

        public static bool Any<T>(this ICollection<T> items) => items.Count > 0;

        public static bool None<T>(this ICollection<T> items) => items.Count == 0;

        public static bool None<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            using (var enumerator = items.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (predicate.Invoke(enumerator.Current))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool HasOne<T>(this ICollection<T> items) => items.Count == 1;

        public static bool DoesNotContain<T>(this ICollection<T> items, T item) => !items.Contains(item);

        public static Expression ReverseChain<T>(this ICollection<T> items)
            where T : IConditionallyChainable
        {
            return ReverseChain(
                items,
                item => item.Value,
                (valueSoFar, item) => Expression.Condition(item.Condition, item.Value, valueSoFar));
        }

        public static Expression ReverseChain<TItem>(
            this ICollection<TItem> items,
            Func<TItem, Expression> seedValueFactory,
            Func<Expression, TItem, Expression> itemValueFactory)
        {
            return Chain(items, i => i.Last(), seedValueFactory, itemValueFactory, i => i.Reverse());
        }

        public static Expression Chain<TItem>(
            this ICollection<TItem> items,
            Func<TItem, Expression> seedValueFactory,
            Func<Expression, TItem, Expression> itemValueFactory)
        {
            return Chain(items, i => i.First(), seedValueFactory, itemValueFactory, i => i);
        }

        private static Expression Chain<TItem>(
            ICollection<TItem> items,
            Func<ICollection<TItem>, TItem> seedFactory,
            Func<TItem, Expression> seedValueFactory,
            Func<Expression, TItem, Expression> itemValueFactory,
            Func<ICollection<TItem>, IEnumerable<TItem>> initialOperation)
        {
            if (items.HasOne())
            {
                return seedValueFactory.Invoke(items.First());
            }

            return initialOperation.Invoke(items)
                .Skip(1)
                .Aggregate(
                    seedValueFactory.Invoke(seedFactory.Invoke(items)),
                    itemValueFactory.Invoke);
        }

        public static T[] ToArray<T>(this IList<T> items)
        {
            var array = new T[items.Count];

            for (var i = 0; i < array.Length; i++)
            {
                array[i] = items[i];
            }

            return array;
        }

        public static T[] ToArray<T>(this ICollection<T> items)
        {
            var array = new T[items.Count];

            items.CopyTo(array, 0);

            return array;
        }

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
            Dictionary<T, int> excludedItemCountsByItem = null;

            foreach (var item in items)
            {
                if (excludedItemCountsByItem == null)
                {
                    excludedItemCountsByItem = GetCountsByItem(excludedItems);
                }

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

        private static Dictionary<T, int> GetCountsByItem<T>(IEnumerable<T> items)
        {
            var itemCountsByItem = new Dictionary<T, int>();

            foreach (var item in items)
            {
                if (item == null)
                {
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
    }
}
