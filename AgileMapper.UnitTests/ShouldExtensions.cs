namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetStandardPolyfills;

    internal static class ShouldExtensions
    {
        public static void ShouldBeDefault<T>(this T value) => value.ShouldBe(default(T));

        public static void ShouldNotBeDefault<T>(this T value) => value.ShouldNotBe(default(T));

        public static void ShouldBe<TActual, TExpected>(this TActual? value, TExpected expectedValue)
            where TActual : struct
        {
            value.GetValueOrDefault().ShouldBe(expectedValue);
        }

        public static void ShouldBe<TActual, TExpected>(this TActual value, TExpected expectedValue)
        {
            var actualExpectedValue = GetActualExpectedValue(expectedValue, typeof(TActual));

            if (value is IComparable<TActual>)
            {
                if (Comparer<TActual>.Default.Compare((TActual)actualExpectedValue, value) != 0)
                {
                    Asplode(expectedValue.ToString(), value.ToString());
                }

                return;
            }

            if (typeof(TActual).IsValueType() || (typeof(TActual) == typeof(ValueType)))
            {
                if (!value.Equals(actualExpectedValue))
                {
                    Asplode(expectedValue.ToString(), value.ToString());
                }

                return;
            }

            if (!ReferenceEquals(expectedValue, value))
            {
                Asplode(expectedValue.ToString(), value.ToString());
            }
        }

        private static object GetActualExpectedValue<TExpected>(TExpected expectedValue, Type actualType)
        {
            if (actualType.IsAssignableTo(typeof(TExpected)))
            {
                return expectedValue;
            }

            if (actualType == typeof(ValueType))
            {
                return (ValueType)(object)expectedValue;
            }

            return Convert.ChangeType(expectedValue, actualType);
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

        public static void ShouldNotBe<TActual, TExpected>(this TActual value, TExpected expectedValue)
        {
            var actualExpectedValue = GetActualExpectedValue(expectedValue, typeof(TActual));

            if (value is IComparable<TActual>)
            {
                if (Comparer<TActual>.Default.Compare((TActual)actualExpectedValue, value) == 0)
                {
                    Asplode(expectedValue.ToString(), value.ToString());
                }

                return;
            }

            if (typeof(TActual).IsValueType() || (typeof(TActual) == typeof(ValueType)))
            {
                if (value.Equals(actualExpectedValue))
                {
                    Asplode(expectedValue.ToString(), value.ToString());
                }

                return;
            }

            if (ReferenceEquals(expectedValue, value))
            {
                Asplode(expectedValue.ToString(), value.ToString());
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
                Asplode("An empty collection", "A non-empty collection");
            }

            return items;
        }

        public static void ShouldBeEmpty<T>(this IEnumerable<T> actual)
        {
            if (actual.Any())
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
                Asplode("An object of type " + typeof(TExpected).Name, actual.GetType().Name);
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
        {
            throw new Exception(errorMessage ?? $"Expected {expected}, but was {actual}");
        }
    }
}
