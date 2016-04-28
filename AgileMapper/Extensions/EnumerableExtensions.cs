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
    }
}
