namespace AgileObjects.AgileMapper.Extensions
{
    using System;

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
            => value.Equals(otherValue, StringComparison.OrdinalIgnoreCase);

        public static bool StartsWithIgnoreCase(this string value, string substring)
            => value.StartsWith(substring, StringComparison.OrdinalIgnoreCase);

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

            if (subjectKey.EqualsIgnoreCase(queryKey))
            {
                return true;
            }

            return (queryKey.IndexOf('.') != -1) && subjectKey.EqualsIgnoreCase(queryKey.Replace(".", null));
        }
    }
}