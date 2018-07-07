namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Collections.Generic;
#if NET35
    using System.Linq;
#endif
    using static System.StringComparison;

    internal static class StringExtensions
    {
        public static string ToPascalCase(this string value)
            => char.ToUpperInvariant(value[0]) + value.Substring(1);

        public static string ToCamelCase(this string value)
            => char.ToLowerInvariant(value[0]) + value.Substring(1);

        public static bool EqualsIgnoreCase(this string value, string otherValue)
            => value.Equals(otherValue, OrdinalIgnoreCase);

        public static bool StartsWithIgnoreCase(this string value, string substring)
            => value.StartsWith(substring, OrdinalIgnoreCase);

#if NET35
        public static Guid ToGuid(this string value)
            => TryParseGuid(value, out var guid) ? guid.GetValueOrDefault() : default(Guid);

        public static Guid? ToGuidNullable(this string value)
            => TryParseGuid(value, out var guid) ? guid : default(Guid?);

        private static bool TryParseGuid(string value, out Guid? guid)
        {
            if (value.IsNullOrWhiteSpace() || (value.Length != 36))
            {
                guid = default(Guid?);
                return false;
            }

            if ((value[8] != '-') || (value[13] != '-') || (value[18] != '-') || (value[23] != '-'))
            {
                guid = default(Guid?);
                return false;
            }

            foreach (var character in value)
            {
                if ((character != '-') && !char.IsLetterOrDigit(character))
                {
                    guid = default(Guid?);
                    return false;
                }
            }

            guid = new Guid(value);
            return true;
        }
#endif
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