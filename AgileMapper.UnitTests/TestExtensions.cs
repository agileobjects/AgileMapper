namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;

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

        public static void ShouldBeDefault<T>(this T value)
        {
            value.ShouldBe(default(T));
        }

        public static void ShouldBe<T>(this T? value, T expectedValue)
            where T : struct
        {
            ShouldBeTestExtensions.ShouldBe(value, expectedValue);
        }

        public static void ShouldBe<TActual, TExpected>(this TActual value, TExpected expectedValue)
        {
            ShouldBeTestExtensions.ShouldBe(value, Convert.ChangeType(expectedValue, typeof(TActual)));
        }
    }
}
