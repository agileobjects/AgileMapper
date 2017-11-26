namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class TestExtensions
    {
        public static T Second<T>(this IEnumerable<T> items) => items.ElementAt(1);

        public static T Third<T>(this IEnumerable<T> items) => items.ElementAt(2);
    }
}