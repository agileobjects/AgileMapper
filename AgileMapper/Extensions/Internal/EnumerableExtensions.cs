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
            => First(items, predicate, (p, item) => p.Invoke(item));

        [DebuggerStepThrough]
        public static T First<TArg, T>(this IList<T> items, TArg argument, Func<TArg, T, bool> predicate)
        {
            if (TryFindMatch(items, argument, predicate, out var match))
            {
                return match;
            }

            throw new InvalidOperationException("Sequence contains no matching element");
        }

        [DebuggerStepThrough]
        public static T FirstOrDefault<T>(this IList<T> items, Func<T, bool> predicate)
            => FirstOrDefault(items, predicate, (p, item) => predicate.Invoke(item));

        [DebuggerStepThrough]
        public static T FirstOrDefault<TArg, T>(this IList<T> items, TArg argument, Func<TArg, T, bool> predicate)
            => TryFindMatch(items, argument, predicate, out var match) ? match : default(T);

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
            => TryFindMatch(items, predicate, (p, item) => p.Invoke(item), out match);

        [DebuggerStepThrough]
        public static bool TryFindMatch<TArg, T>(this IList<T> items, TArg argument, Func<TArg, T, bool> predicate, out T match)
        {
            for (int i = 0, n = items.Count; i < n; ++i)
            {
                match = items[i];

                if (predicate.Invoke(argument, match))
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
        public static bool Any<TArg, T>(this IList<T> items, TArg argument, Func<TArg, T, bool> predicate)
            => !None(items, argument, predicate);

        [DebuggerStepThrough]
        public static bool None<T>(this ICollection<T> items) => items.Count == 0;

        [DebuggerStepThrough]
        public static bool NoneOrNull<T>(this ICollection<T> items) => (items == null) || items.None();

        [DebuggerStepThrough]
        public static bool None<T>(this IEnumerable<T> items) => !items.GetEnumerator().MoveNext();

        public static bool None<T>(this IList<T> items, Func<T, bool> predicate)
            => None(items, predicate, (p, item) => p.Invoke(item));

        public static bool None<TArg, T>(this IList<T> items, TArg argument, Func<TArg, T, bool> predicate)
        {
            for (int i = 0, n = items.Count; i < n; i++)
            {
                if (predicate.Invoke(argument, items[i]))
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

        public static TResult[] ProjectToArray<TItem, TResult>(this IList<TItem> items, Func<TItem, TResult> projector)
            => ProjectToArray(items, projector, (p, item) => p.Invoke(item));

        public static TResult[] ProjectToArray<TArg, TItem, TResult>(
            this IList<TItem> items,
            TArg argument,
            Func<TArg, TItem, TResult> projector)
        {
            var itemCount = items.Count;

            switch (itemCount)
            {
                case 0:
                    return Enumerable<TResult>.EmptyArray;

                case 1:
                    return new[] { projector.Invoke(argument, items[0]) };

                default:
                    var result = new TResult[items.Count];

                    for (var i = 0; ;)
                    {
                        result[i] = projector.Invoke(argument, items[i]);

                        if (++i == itemCount)
                        {
                            return result;
                        }
                    }
            }
        }

        public static T[] CopyToArray<T>(this IList<T> items)
        {
            if (items.Count == 0)
            {
                return Enumerable<T>.EmptyArray;
            }

            var clonedArray = new T[items.Count];

            clonedArray.CopyFrom(items);

            return clonedArray;
        }

        public static Expression Chain<TItem>(
            this IList<TItem> items,
            Func<TItem, Expression> seedValueFactory,
            Func<Expression, TItem, Expression> chainedValueFactory)
        {
            return Chain(items, i => i.First(), seedValueFactory, chainedValueFactory, i => i);
        }

        public static Expression Chain<TItem>(
            this IList<TItem> items,
            Func<IList<TItem>, TItem> seedFactory,
            Func<TItem, Expression> seedValueFactory,
            Func<Expression, TItem, Expression> chainedValueFactory,
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
                .WhereNotNull()
                .Aggregate(
                    seedValueFactory.Invoke(seedFactory.Invoke(items)),
                    (chainedExpression, item) => (chainedExpression == null)
                        ? seedValueFactory.Invoke(item)
                        : chainedValueFactory.Invoke(chainedExpression, item));
        }

        public static void CopyTo<T>(this IList<T> sourceList, List<T> targetList)
            => targetList.AddRange(sourceList);

        public static void CopyFrom<T>(this IList<T> targetList, IList<T> sourceList, int startIndex = 0)
        {
            for (var i = 0; i < sourceList.Count && i < targetList.Count; ++i)
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

        public static void Insert<T>(this IList<T> items, T item, int insertionOffset)
        {
            var insertionIndex = items.Count - insertionOffset;
            items.Insert(insertionIndex, item);
        }

        public static T[] Append<T>(this IList<T> array, T extraItem)
        {
            switch (array.Count)
            {
                case 0:
                    return new[] { extraItem };

                case 1:
                    return new[] { array[0], extraItem };

                case 2:
                    return new[] { array[0], array[1], extraItem };

                default:
                    var newArray = new T[array.Count + 1];

                    newArray.CopyFrom(array);

                    newArray[array.Count] = extraItem;

                    return newArray;
            }
        }

        public static IList<T> Append<T>(this IList<T> array, IList<T> extraItems)
        {
            if (extraItems.Count == 0)
            {
                return array;
            }

            if (array.Count == 0)
            {
                return extraItems;
            }

            if (extraItems.Count == 1)
            {
                return Append(array, extraItems[0]);
            }

            if (array.Count == 1)
            {
                return Prepend(extraItems, array[0]);
            }

            var combinedArray = new T[array.Count + extraItems.Count];

            combinedArray.CopyFrom(array);
            combinedArray.CopyFrom(extraItems, array.Count);

            return combinedArray;
        }
    }
}
