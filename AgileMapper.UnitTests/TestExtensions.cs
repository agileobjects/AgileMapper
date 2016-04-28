namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class TestExtensions
    {
        public static T Second<T>(this IEnumerable<T> items)
        {
            return items.ElementAt(1);
        }

        public static bool SequenceEqual<T1, T2>(this IEnumerable<T1> first, Func<T1, T2> converter, params T2[] second)
        {
            return first.Select(converter).SequenceEqual(second);
        }

        public static bool SequenceEqual<T>(this IEnumerable<T> first, params T[] second)
        {
            return first.SequenceEqual(second.AsEnumerable());
        }
    }
}
