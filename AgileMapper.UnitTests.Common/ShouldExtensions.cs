namespace AgileObjects.AgileMapper.UnitTests.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    public static class ShouldExtensions
    {
        public static void ShouldBeDefault<T>(this T value) => value.ShouldBe(default(T));

        public static void ShouldNotBeDefault<T>(this T value) => value.ShouldNotBe(default(T));

        public static void ShouldBe(this DateTime value, DateTime expectedValue, TimeSpan tolerance)
        {
            var minimumExpectedValue = expectedValue.Subtract(tolerance);

            if (value < minimumExpectedValue)
            {
                Asplode($"a DateTime greater than {minimumExpectedValue}", $"{value}");
            }

            var maximumExpectedValue = expectedValue.Add(tolerance);

            if (value > maximumExpectedValue)
            {
                Asplode($"a DateTime less than {maximumExpectedValue}", $"{value}");
            }
        }

        public static void ShouldBe<TActual, TExpected>(this TActual? value, TExpected expectedValue)
            where TActual : struct
        {
            if (expectedValue == null && value.HasValue)
            {
                Asplode("null", "not null");
            }

            if (expectedValue != null && !value.HasValue)
            {
                Asplode(expectedValue.ToString(), "null");
            }

            value.GetValueOrDefault().ShouldBe(expectedValue);
        }

        public static void ShouldBe<TActual, TExpected>(this TActual value, TExpected expectedValue)
        {
            if (!AreEqual(expectedValue, value))
            {
                Asplode(expectedValue.ToString(), value?.ToString());
            }
        }

        private static readonly MethodInfo _areEqualMethod = typeof(ShouldExtensions)
            .GetNonPublicStaticMethod("AreEqual");

        private static bool AreEqual<TExpected, TActual>(TExpected expected, TActual actual)
        {
            if (ReferenceEquals(expected, actual))
            {
                return true;
            }

            if (typeof(TActual) == typeof(object))
            {
                return (bool)_areEqualMethod
                    .MakeGenericMethod(typeof(TExpected), actual.GetType())
                    .Invoke(null, new object[] { expected, actual });
            }

            if (typeof(TActual) == typeof(ValueType))
            {
                return (bool)_areEqualMethod
                    .MakeGenericMethod(typeof(TExpected), actual.GetType())
                    .Invoke(null, new object[] { expected, actual });
            }

            var actualExpectedValue = GetActualExpectedValue(expected, actual);

            if (typeof(TActual).IsValueType())
            {
                return actual.Equals(actualExpectedValue);
            }

            if (actual is IComparable<TActual>)
            {
                return Comparer<TActual>.Default.Compare(actualExpectedValue, actual) == 0;
            }

            if (typeof(TActual).IsAssignableTo(typeof(TExpected)))
            {
                return false;
            }

            throw new NotSupportedException(
                $"Cannot determine equality between {typeof(TExpected).Name} and {typeof(TActual).Name}");
        }

        private static TActual GetActualExpectedValue<TExpected, TActual>(TExpected expected, TActual actual)
        {
            if (typeof(TActual).IsAssignableTo(typeof(TExpected)))
            {
                return (TActual)(object)expected;
            }

            if (actual is string)
            {
                return (TActual)(object)expected.ToString();
            }

            return (TActual)Convert.ChangeType(expected, typeof(TActual));
        }

        public static void ShouldBe<T1, T2>(this IEnumerable<T1> actualValues, IEnumerable<T2> expectedValues)
        {
            void FailTest()
            {
                Asplode(
                    expectedValues.Project(v => v.ToString()).Join(", "),
                    actualValues.Project(v => v.ToString()).Join(", "));
            }

            using (var expectedEnumerator = expectedValues.GetEnumerator())
            using (var actualEnumerator = actualValues.GetEnumerator())
            {
                while (true)
                {
                    var moreExpected = expectedEnumerator.MoveNext();
                    var moreActual = actualEnumerator.MoveNext();

                    if (!moreExpected && !moreActual)
                    {
                        return;
                    }

                    if (moreExpected != moreActual)
                    {
                        FailTest();
                    }

                    ShouldBe(expectedEnumerator.Current, actualEnumerator.Current);
                }
            }
        }

        public static void ShouldBe<T>(this IEnumerable<T> actualValues, params T[] expectedValues)
        {
            actualValues.ShouldNotBeNull();
            actualValues.ShouldNotBeEmpty();
            actualValues.SequenceEqual(expectedValues).ShouldBeTrue();
        }

        public static void ShouldBe<T1, T2>(this IEnumerable<T1> actualValues, Func<T1, T2> converter, params T2[] expectedValues)
        {
            actualValues.ShouldNotBeNull();
            actualValues.ShouldBe(expectedValues, converter);
        }

        public static void ShouldBe<T1, T2>(this IEnumerable<T1> actualValues, IEnumerable<T2> expectedValues, Func<T1, T2> converter)
        {
            actualValues.ShouldNotBeNull();
            actualValues.ShouldNotBeEmpty();
            actualValues.Project(converter).SequenceEqual(expectedValues).ShouldBeTrue();
        }

        public static void ShouldNotBe<TActual, TExpected>(this TActual value, TExpected expectedValue)
        {
            if (AreEqual(expectedValue, value))
            {
                Asplode("Not " + expectedValue, value.ToString());
            }
        }

        public static void ShouldNotBeSameAs<T>(this T actualItem, T nonExpectedItem)
            where T : class
        {
            if (ReferenceEquals(nonExpectedItem, actualItem))
            {
                Asplode("Not " + nonExpectedItem, nonExpectedItem.ToString());
            }
        }

        public static void ShouldBeSameAs<T>(this T actualItem, T expectedItem)
            where T : class
        {
            if (!ReferenceEquals(expectedItem, actualItem))
            {
                Asplode(expectedItem.ToString(), actualItem.ToString());
            }
        }

        public static void ShouldBeTrue(this bool? value)
        {
            value.GetValueOrDefault().ShouldBeTrue();
        }

        public static void ShouldBeTrue(this bool boolValue)
        {
            if (boolValue != true)
            {
                Asplode("true", "false");
            }
        }

        public static void ShouldBeFalse(this bool? value)
        {
            value.GetValueOrDefault().ShouldBeFalse();
        }

        public static void ShouldBeFalse(this bool boolValue)
        {
            if (boolValue)
            {
                Asplode("false", "true");
            }
        }

        public static void ShouldNotBeNull<T>(this T? actual)
            where T : struct
        {
            if (!actual.HasValue)
            {
                Asplode("non-null", "null");
            }
        }

        public static void ShouldBeNull<T>(this T? actual)
            where T : struct
        {
            if (actual.HasValue)
            {
                Asplode("null", "non-null");
            }
        }

        public static T ShouldNotBeNull<T>(this T actual, string expectedValue = null)
            where T : class
        {
            if (actual == null)
            {
                Asplode(expectedValue ?? "non-null", "null");
            }

            return actual;
        }

        public static void ShouldBeNull<T>(this T actual, string errorMessage = null)
            where T : class
        {
            if (actual != null)
            {
                Asplode("null", "non-null", errorMessage);
            }
        }

        public static void ShouldHaveFlag(this Enum actual, Enum expected)
            => HasFlag(actual, expected).ShouldBeTrue();

        public static void ShouldNotHaveFlag(this Enum actual, Enum notExpected)
            => HasFlag(actual, notExpected).ShouldBeFalse();

        private static bool HasFlag(Enum actual, Enum expected)
        {
#if NET35
            var actualValue = Convert.ToUInt64(actual);
            var expectedValue = Convert.ToUInt64(expected);

            return (actualValue & expectedValue) == expectedValue;
#else
            return actual.HasFlag(expected);
#endif
        }

        public static T ShouldHaveSingleItem<T>(this IEnumerable<T> items)
        {
            using (var enumerator = items.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    Asplode("a single item", "no items");
                }

                var singleItem = enumerator.Current;

                if (enumerator.MoveNext())
                {
                    Asplode("a single item", "multiple items");
                }

                return singleItem;
            }
        }

        public static void ShouldAllBe<T>(this IEnumerable<T> items, Func<T, bool> itemTest)
        {
            foreach (var item in items)
            {
                if (!itemTest.Invoke(item))
                {
                    Asplode("All items to match", item.ToString());
                }
            }
        }

        public static IEnumerable<T> ShouldNotBeEmpty<T>(this IEnumerable<T> items)
        {
            if (!items.GetEnumerator().MoveNext())
            {
                Asplode("a non-empty collection", "an empty collection");
            }

            return items;
        }

        public static void ShouldBeEmpty<T>(this IEnumerable<T> actual)
        {
            if (actual == null)
            {
                Asplode("an empty collection", "null");
            }
            else if (actual.Any())
            {
                Asplode("an empty collection", "non-empty");
            }
        }

        public static T ShouldContain<T>(this IEnumerable<T> items, T expectedItem)
        {
            if (!items.Contains(expectedItem))
            {
                Asplode("Collection to contain " + expectedItem, "Not contained");
            }

            return expectedItem;
        }

        public static T ShouldNotContain<T>(this IEnumerable<T> items, T expectedItem)
        {
            if (items.Contains(expectedItem))
            {
                Asplode("Collection not to contain " + expectedItem, "Contained");
            }

            return expectedItem;
        }

        public static void ShouldContain(this string actualString, string expectedSubstring)
        {
            if (!actualString.Contains(expectedSubstring))
            {
                Asplode("String to contain " + expectedSubstring, actualString);
            }
        }

        public static void ShouldNotContain(this string actualString, string nonExpectedSubstring)
        {
            if (actualString.Contains(nonExpectedSubstring))
            {
                Asplode("String not to contain " + nonExpectedSubstring, actualString);
            }
        }

        public static IDictionary<TKey, TValue> ShouldContainKey<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey expectedKey)
        {
            if (!dictionary.ContainsKey(expectedKey))
            {
                Asplode("Dictionary with key " + expectedKey, "No contained key");
            }

            return dictionary;
        }

        public static IDictionary<TKey, TValue> ShouldNotContainKey<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey nonExpectedKey)
        {
            if (dictionary.ContainsKey(nonExpectedKey))
            {
                Asplode("Dictionary without key " + nonExpectedKey, "Contained key");
            }

            return dictionary;
        }

        public static IDictionary<TKey, TValue> ShouldContainKeyAndValue<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey expectedKey,
            TValue expectedValue)
            where TValue : class
        {
            if (!dictionary.TryGetValue(expectedKey, out var value))
            {
                Asplode("Dictionary with key " + expectedKey, "No contained key");
            }

            value.ShouldBeSameAs(expectedValue);

            return dictionary;
        }

        public static void ShouldBeOfType<TExpected>(this object actual)
        {
            if (!(actual is TExpected))
            {
                Asplode(
                    "An object of type " + typeof(TExpected).GetFriendlyName(),
                    actual.GetType().GetFriendlyName());
            }
        }

        public static void ShouldContain<T>(this IList<T> actual, T expected)
        {
            if (!actual.Contains(expected))
            {
                Asplode(expected.ToString(), "No match");
            }
        }

        public static void ShouldNotContain<T>(this IList<T> actual, T expected)
        {
            if (actual.Contains(expected))
            {
                Asplode("No match", expected.ToString());
            }
        }

        private static void Asplode(string expected, string actual, string errorMessage = null)
            => throw new TestAsplodeException(expected, actual, errorMessage);

        public class TestAsplodeException : Exception
        {
            public TestAsplodeException(string expected, string actual, string errorMessage)
                : base(errorMessage ?? $"Expected {expected}, but was {actual}")
            {
            }
        }
    }
}
