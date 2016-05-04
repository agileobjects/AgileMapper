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
            return first.SequenceEqual(converter, second.AsEnumerable());
        }

        public static bool SequenceEqual<T1, T2>(this IEnumerable<T1> first, Func<T1, T2> converter, IEnumerable<T2> second)
        {
            return first.Select(converter).SequenceEqual(second);
        }

        public static bool SequenceEqual<T>(this IEnumerable<T> first, params T[] second)
        {
            return first.SequenceEqual(second.AsEnumerable());
        }

        public static void ShouldBe<T>(this IEnumerable<T> actualValues, params T[] expectedValues)
        {
            actualValues.SequenceEqual(expectedValues).ShouldBeTrue();
        }

        public static void ShouldBeDefault<T>(this T value)
        {
            value.ShouldBe(default(T));
        }

        public static void ShouldBe<TActual, TExpected>(this TActual? value, TExpected expectedValue)
            where TActual : struct
        {
            value.GetValueOrDefault().ShouldBe(expectedValue);
        }

        public static void ShouldBe<TActual, TExpected>(this TActual value, TExpected expectedValue)
        {
            var actualExpectedValue = typeof(TExpected).IsAssignableFrom(typeof(TActual))
                ? expectedValue
                : Convert.ChangeType(expectedValue, typeof(TActual));

            ShouldBeTestExtensions.ShouldBe(value, actualExpectedValue);
        }
    }
}
