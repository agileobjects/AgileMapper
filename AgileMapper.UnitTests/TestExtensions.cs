namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Shouldly;

    internal static class TestExtensions
    {
        public static string ToCurrentCultureString(this DateTime? dateTime)
            => dateTime.GetValueOrDefault().ToCurrentCultureString();

        public static string ToCurrentCultureString(this DateTime dateTime)
            => dateTime.ToString(CultureInfo.CurrentCulture);

        public static T Second<T>(this IEnumerable<T> items) => items.ElementAt(1);

        public static T Third<T>(this IEnumerable<T> items) => items.ElementAt(2);

        public static void ShouldBeDefault<T>(this T value) => value.ShouldBe(default(T));

        public static void ShouldNotBeDefault<T>(this T value) => value.ShouldNotBe(default(T));

        public static void ShouldBeTrue(this bool? value) => value.GetValueOrDefault().ShouldBeTrue();

        public static void ShouldBeFalse(this bool? value) => value.GetValueOrDefault().ShouldBeFalse();

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

        public static void ShouldBe<T>(this IEnumerable<T> actualValues, params T[] expectedValues)
        {
            actualValues.ShouldNotBeEmpty();
            actualValues.SequenceEqual(expectedValues).ShouldBeTrue();
        }

        public static void ShouldBe<T1, T2>(this IEnumerable<T1> actualValues, Func<T1, T2> converter, params T2[] expectedValues)
        {
            actualValues.ShouldBe(expectedValues, converter);
        }

        public static void ShouldBe<T1, T2>(this IEnumerable<T1> actualValues, IEnumerable<T2> expectedValues, Func<T1, T2> converter)
        {
            actualValues.ShouldNotBeEmpty();
            actualValues.Select(converter).SequenceEqual(expectedValues).ShouldBeTrue();
        }
    }
}
