namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class EnumerableExtensions
    {
        public static void AddUnlessNullOrEmpty(this ICollection<Expression> items, Expression item)
        {
            if ((item != null) && (item != Constants.EmptyExpression))
            {
                items.Add(item);
            }
        }

#if NET35
        public static void AddRange<TContained, TItem>(this List<TContained> items, IEnumerable<TItem> newItems)
            where TItem : TContained
        {
            var itemsToAdd = newItems.ToArray();

            var requiredCapacity = items.Count + itemsToAdd.Length;

            if (items.Capacity < requiredCapacity)
            {
                items.Capacity = requiredCapacity + 3;
            }

            foreach (var newItem in itemsToAdd)
            {
                items.Add(newItem);
            }
        }
#endif

        [DebuggerStepThrough]
        public static T First<T>(this IList<T> items) => items[0];

        [DebuggerStepThrough]
        public static T First<T>(this IList<T> items, Func<T, bool> predicate)
        {
            if (TryFindMatch(items, predicate, out var match))
            {
                return match;
            }

            throw new InvalidOperationException("Sequence contains no matching element");
        }

        [DebuggerStepThrough]
        public static T FirstOrDefault<T>(this IList<T> items, Func<T, bool> predicate)
            => TryFindMatch(items, predicate, out var match) ? match : default(T);

        [DebuggerStepThrough]
        public static IEnumerable<T> TakeUntil<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            foreach (var item in items)
            {
                yield return item;

                if (predicate.Invoke(item))
                {
                    yield break;
                }
            }
        }

        [DebuggerStepThrough]
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

        public static bool All<T>(this IList<T> items, Func<T, bool> predicate)
            => None(items, item => !predicate.Invoke(item));

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

        public static void CopyTo<T>(this IList<T> sourceList, List<T> targetList, int startIndex = 0)
            => targetList.AddRange(sourceList);

        public static void CopyFrom<T>(this IList<T> targetList, IList<T> sourceList, int startIndex = 0)
        {
            for (var i = 0; i < sourceList.Count; i++)
            {
                targetList[i + startIndex] = sourceList[i];
            }
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
    }
}
