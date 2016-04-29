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

        public static void ForEach<T>(this IEnumerable<T> items, Action<T, int> itemAction)
        {
            var i = 0;

            foreach (var item in items)
            {
                itemAction.Invoke(item, i++);
            }
        }

        public static void Broadcast<T>(this IEnumerable<Action<T>> callbacks, T data)
        {
            foreach (var callback in callbacks)
            {
                callback.Invoke(data);
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
