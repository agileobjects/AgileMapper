namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    internal static class TestExtensions
    {
        public static string ToCurrentCultureString(this DateTime? dateTime)
            => dateTime.GetValueOrDefault().ToCurrentCultureString();

        public static string ToCurrentCultureString(this DateTime dateTime)
            => dateTime.ToString(CultureInfo.CurrentCulture);

        public static T Second<T>(this IEnumerable<T> items)
        {
            return items.ElementAt(1);
        }

        public static T Third<T>(this IEnumerable<T> items)
        {
            return items.ElementAt(2);
        }
    }
}
