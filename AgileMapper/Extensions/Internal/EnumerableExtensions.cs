namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class EnumerableExtensions
    {
        public static readonly MethodInfo EnumerableNoneMethod = typeof(EnumerableExtensions)
            .GetPublicStaticMethods("None")
            .First(m => m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(string));

        public static void AddUnlessNullOrEmpty(this ICollection<Expression> items, Expression item)
        {
            if ((item != null) && (item != Constants.EmptyExpression))
            {
                items.Add(item);
            }
        }

        public static IEnumerable<TResult> Project<TItem, TResult>(this IEnumerable<TItem> items, Func<TItem, TResult> projector)
        {
            foreach (var item in items)
            {
                yield return projector.Invoke(item);
            }
        }

        public static IEnumerable<TResult> Project<TItem, TResult>(this IEnumerable<TItem> items, Func<TItem, int, TResult> projector)
        {
            var index = 0;

            foreach (var item in items)
            {
                yield return projector.Invoke(item, index++);
            }
        }

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

#if NET35
        public static void AddRange<TContained, TItem>(this List<TContained> items, IEnumerable<TItem> newItems)
            where TItem : TContained
        {
            foreach (var newItem in newItems)
            {
                items.Add(newItem);
            }
        }
#endif

        [DebuggerStepThrough]
        public static T First<T>(this IList<T> items) => items[0];

        public static T First<T>(this IList<T> items, Func<T, bool> predicate)
        {
            if (TryFindMatch(items, predicate, out var match))
            {
                return match;
            }

            throw new InvalidOperationException("Sequence contains no matching element");
        }

        public static T FirstOrDefault<T>(this IList<T> items, Func<T, bool> predicate)
            => TryFindMatch(items, predicate, out var match) ? match : default(T);

        public static bool TryFindMatch<T>(this IList<T> items, Func<T, bool> predicate, out T match)
        {
            for (int i = 0, n = items.Count; i < n; i++)
            {
                match = items[i];

                if (predicate.Invoke(match))
                {
                    return true;
                }
            }

            match = default(T);
            return false;
        }

        [DebuggerStepThrough]
        public static T Last<T>(this IList<T> items) => items[items.Count - 1];

        [DebuggerStepThrough]
        public static bool Any<T>(this ICollection<T> items) => items.Count != 0;

        [DebuggerStepThrough]
        public static bool Any<T>(this IList<T> items, Func<T, bool> predicate) => !items.None(predicate);

        [DebuggerStepThrough]
        public static bool None<T>(this ICollection<T> items) => items.Count == 0;

        [DebuggerStepThrough]
        public static bool None<T>(this IEnumerable<T> items) => !items.GetEnumerator().MoveNext();

        // Used in Dictionary mapping via EnumerableNoneMethod
        public static bool None<T>(this IEnumerable<T> items, Func<T, bool> predicate)
            => items.All(item => !predicate.Invoke(item));

        public static bool None<T>(this IList<T> items, Func<T, bool> predicate)
        {
            for (int i = 0, n = items.Count; i < n; i++)
            {
                if (predicate.Invoke(items[i]))
                {
                    return false;
                }
            }

            return true;
        }

        [DebuggerStepThrough]
        public static bool HasOne<T>(this ICollection<T> items) => items.Count == 1;

        public static Expression ReverseChain<T>(this IList<T> items)
            where T : IConditionallyChainable
        {
            return Chain(
                items,
                i => i.Last(),
                item => item.AddPreConditionIfNecessary(item.Value),
                (valueSoFar, item) => item.AddPreConditionIfNecessary(
                    Expression.Condition(item.Condition, item.Value, valueSoFar)),
                i => i.Reverse());
        }

        public static Expression AddPreConditionIfNecessary(this IConditionallyChainable item, Expression ifTrueBranch)
        {
            if (item.PreCondition == null)
            {
                return ifTrueBranch;
            }

            return Expression.Condition(
                item.PreCondition,
                ifTrueBranch,
                ifTrueBranch.Type.ToDefaultExpression());
        }

        public static Expression Chain<TItem>(
            this IList<TItem> items,
            Func<TItem, Expression> seedValueFactory,
            Func<Expression, TItem, Expression> itemValueFactory)
        {
            return Chain(items, i => i.First(), seedValueFactory, itemValueFactory, i => i);
        }

        private static Expression Chain<TItem>(
            IList<TItem> items,
            Func<IList<TItem>, TItem> seedFactory,
            Func<TItem, Expression> seedValueFactory,
            Func<Expression, TItem, Expression> itemValueFactory,
            Func<IList<TItem>, IEnumerable<TItem>> initialOperation)
        {
            if (items.None())
            {
                return null;
            }

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

            array.CopyFrom(items);

            return array;
        }

        public static void CopyTo<T>(this IList<T> sourceList, List<T> targetList, int startIndex = 0)
            => targetList.AddRange(sourceList);

        public static void CopyFrom<T>(this IList<T> targetList, IList<T> sourceList, int startIndex = 0)
        {
            for (var i = 0; i < sourceList.Count; i++)
            {
                targetList[i + startIndex] = sourceList[i];
            }
        }

        public static T[] ToArray<T>(this ICollection<T> items)
        {
            var array = new T[items.Count];

            items.CopyTo(array, 0);

            return array;
        }

        [DebuggerStepThrough]
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> items) => items.Filter(item => item != null);

        public static T[] Prepend<T>(this IList<T> items, T initialItem)
        {
            var newArray = new T[items.Count + 1];

            newArray.CopyFrom(items, 1);

            newArray[0] = initialItem;

            return newArray;
        }

        public static T[] Append<T>(this T[] array, T extraItem)
        {
            switch (array.Length)
            {
                case 0:
                    return new[] { extraItem };

                case 1:
                    return new[] { array[0], extraItem };

                case 2:
                    return new[] { array[0], array[1], extraItem };

                default:
                    var newArray = new T[array.Length + 1];

                    newArray.CopyFrom(array);

                    newArray[array.Length] = extraItem;

                    return newArray;
            }
        }

        public static T[] Append<T>(this IList<T> array, params T[] extraItems)
            => Append(array, (IList<T>)extraItems);

        public static T[] Append<T>(this IList<T> array, IList<T> extraItems)
        {
            var combinedArray = new T[array.Count + extraItems.Count];

            combinedArray.CopyFrom(array);
            combinedArray.CopyFrom(extraItems, array.Count);

            return combinedArray;
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
