namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    internal static class TestExtensions
    {
        public static string ToCurrentCultureString(this DateTime? dateTime)
            => dateTime.GetValueOrDefault().ToCurrentCultureString();

        public static string ToCurrentCultureString(this DateTime dateTime)
            => dateTime.ToString(CultureInfo.CurrentCulture);

        [DebuggerStepThrough]
        public static T Second<T>(this IEnumerable<T> items) => items.ElementAt(1);

        [DebuggerStepThrough]
        public static T Third<T>(this IEnumerable<T> items) => items.ElementAt(2);
    }
}
