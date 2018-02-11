namespace AgileObjects.AgileMapper.UnitTests.Orms
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class TestExtensions
    {
        private static readonly Regex _tableNameMatcher = new Regex(@"FROM\s+(?<table>.+)\s+AS");

        public static T Second<T>(this IEnumerable<T> items) => items.ElementAt(1);

        public static T Third<T>(this IEnumerable<T> items) => items.ElementAt(2);

        public static T Fourth<T>(this IEnumerable<T> items) => items.ElementAt(3);

        public static string GetTableName(this string traceString)
            => _tableNameMatcher.Match(traceString).Groups["table"].Value;
    }
}