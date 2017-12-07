﻿namespace AgileObjects.AgileMapper.Extensions
{
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
    }
}