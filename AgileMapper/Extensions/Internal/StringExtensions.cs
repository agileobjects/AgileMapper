namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System.Collections.Generic;
#if NET35
    using System.Linq;
#endif
    using System.Text.RegularExpressions;
    using static System.StringComparison;

    internal static class StringExtensions
    {
        public static string ToPascalCase(this string value)
            => char.ToUpperInvariant(value[0]) + value.Substring(1);

        public static string ToCamelCase(this string value)
            => char.ToLowerInvariant(value[0]) + value.Substring(1);

        public static string FirstOrDefault(this string value)
        {
            if (string.IsNullOrEmpty(value) || (value.Length <= 1))
            {
                return value;
            }

            return value[0].ToString();
        }

        public static bool EqualsIgnoreCase(this string value, string otherValue)
            => value.Equals(otherValue, OrdinalIgnoreCase);

        public static bool StartsWithIgnoreCase(this string value, string substring)
            => value.StartsWith(substring, OrdinalIgnoreCase);

        public static bool MatchesKey(
            this string subjectKey,
            string queryKey,
            string separator,
            Regex elementKeyPartMatcher)
        {
            if (queryKey == null)
            {
                // This can happen when mapping to types with multiple, nested
                // recursive relationships, e.g:
                // Dictionary<,> -> Order -> OrderItems -> Order -> OrderItems
                // ...it's basically not supported
                return false;
            }

            if (subjectKey.EqualsIgnoreCase(queryKey))
            {
                return true;
            }

            var elementKeyParts = elementKeyPartMatcher.Matches(queryKey);

            var searchEndIndex = queryKey.Length;

            for (var i = elementKeyParts.Count; i > 0; --i)
            {
                var elementKeyPart = elementKeyParts[i - 1];
                var matchStartIndex = elementKeyPart.Index;
                var matchEndIndex = matchStartIndex + elementKeyPart.Length;

                ReplaceSeparatorsInSubstring(matchStartIndex, matchEndIndex, ref queryKey, separator, ref searchEndIndex);
            }

            ReplaceSeparatorsInSubstring(searchEndIndex, 0, ref queryKey, separator, ref searchEndIndex);

            return subjectKey.EqualsIgnoreCase(queryKey);
        }

        private static void ReplaceSeparatorsInSubstring(
            int matchStartIndex,
            int matchEndIndex,
            ref string queryKey,
            string separator,
            ref int searchEndIndex)
        {
            var querySubstring = queryKey.Substring(matchEndIndex, searchEndIndex - matchEndIndex);

            if (querySubstring.IndexOf(separator, Ordinal) == -1)
            {
                searchEndIndex = matchStartIndex;
                return;
            }

            var flattenedQuerySubstring = querySubstring.Replace(separator, null);

            queryKey = queryKey
                .Remove(matchEndIndex, searchEndIndex - matchEndIndex)
                .Insert(matchEndIndex, flattenedQuerySubstring);

            searchEndIndex = matchStartIndex;
        }

        public static bool MatchesKey(this string subjectKey, string queryKey, string separator)
        {
            if (queryKey == null)
            {
                // This can happen when mapping to types with multiple, nested
                // recursive relationships, e.g:
                // Dictionary<,> -> Order -> OrderItems -> Order -> OrderItems
                // ...it's basically not supported
                return false;
            }

            return subjectKey.EqualsIgnoreCase(queryKey) ||
                   subjectKey.MatchesFlattenedKey(queryKey, separator);
        }

        private static bool MatchesFlattenedKey(this string subjectKey, string queryKey, string separator)
        {
            return (queryKey.IndexOf(separator, Ordinal) != -1) &&
                   subjectKey.EqualsIgnoreCase(queryKey.Replace(separator, null));
        }

        public static bool MatchesKey(this string subjectKey, string queryKey)
        {
            if (queryKey == null)
            {
                // This can happen when mapping to types with multiple, nested
                // recursive relationships, e.g:
                // Dictionary<,> -> Order -> OrderItems -> Order -> OrderItems
                // ...it's basically not supported
                return false;
            }

            return subjectKey.EqualsIgnoreCase(queryKey);
        }

        public static string Pluralise(this string value)
        {
            if (value.Length == 1)
            {
                return value + "s";
            }

            switch (value.Substring(value.Length - 2))
            {
                case "ch":
                case "sh":
                case "ss":
                    return value + "es";
            }

            if (value.EndsWith('s'))
            {
                return value;
            }

            if (value.EndsWith('y') && IsConsonant(value[value.Length - 2]))
            {
                return value.Substring(0, value.Length - 1) + "ies";
            }

            if (value.EndsWith('x') || value.EndsWith('z'))
            {
                return value + "es";
            }

            return value + "s";
        }

        private static bool IsConsonant(char character)
        {
            switch (char.ToUpperInvariant(character))
            {
                case 'A':
                case 'E':
                case 'I':
                case 'O':
                case 'U':
                    return false;
            }

            return true;
        }

        public static string Join(this IEnumerable<string> strings, string separator)
        {
#if NET35
            return string.Join(separator, strings.ToArray());
#else
            return string.Join(separator, strings);
#endif
        }

        public static string Join<T>(this IEnumerable<T> values, string separator)
        {
#if NET35
            return string.Join(separator, values.Project(v => v.ToString()).ToArray());
#else
            return string.Join(separator, values);
#endif
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
#if NET35
            return (value == null) || (value.Trim() == string.Empty);
#else
            return string.IsNullOrWhiteSpace(value);
#endif
        }
    }
}