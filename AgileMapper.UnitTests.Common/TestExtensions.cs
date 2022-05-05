namespace AgileObjects.AgileMapper.UnitTests.Common;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

public static class TestExtensions
{
    public static string RemoveWhiteSpace(this string plan)
        => plan.Replace(Environment.NewLine, null).Replace(" ", null);

    public static string ToCurrentCultureString(this DateTime? dateTime)
        => dateTime.GetValueOrDefault().ToCurrentCultureString();

    public static string ToCurrentCultureString(this DateTime dateTime)
        => dateTime.ToString(CultureInfo.CurrentCulture);

    [DebuggerStepThrough]
    public static T Second<T>(this IEnumerable<T> items) => items.ElementAt(1);

    [DebuggerStepThrough]
    public static T Third<T>(this IEnumerable<T> items) => items.ElementAt(2);

    [DebuggerStepThrough]
    public static T Fourth<T>(this IEnumerable<T> items) => items.ElementAt(3);
}