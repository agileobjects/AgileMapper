namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using NetStandardPolyfills;

    internal static class TypeExtensions
    {
        private static readonly Assembly _msCorLib = typeof(string).GetAssembly();

        public static string GetShortVariableName(this Type type)
        {
            var variableName = type.GetVariableNameInPascalCase();

            var shortVariableName =
                variableName[0] +
                string.Join(
                    string.Empty,
                    variableName.ToCharArray().Skip(1).Where(char.IsUpper));

            shortVariableName = shortVariableName.ToLowerInvariant();

            return (!type.IsArray && type.IsEnumerable())
                ? Pluralise(shortVariableName)
                : shortVariableName;
        }

        public static string GetVariableNameInCamelCase(this Type type) => type.GetVariableName(f => f.ToCamelCase());

        public static string GetVariableNameInPascalCase(this Type type) => type.GetVariableName(f => f.ToPascalCase());

        private static string GetVariableName(
            this Type type,
            Func<string, string> formatter)
        {
            var typeIsEnumerable = type.IsEnumerable();
            var namingType = typeIsEnumerable ? type.GetEnumerableElementType() : type;
            var variableName = namingType.Name;

            if (namingType.IsInterface())
            {
                variableName = variableName.Substring(1);
            }

            if (namingType.IsGenericType())
            {
                variableName = variableName.Substring(0, variableName.IndexOf('`'));

                variableName += string.Join(
                    string.Empty,
                    namingType.GetGenericArguments().Select(arg => "_" + arg.GetVariableNameInPascalCase()));
            }

            variableName = RemoveNonAlphaNumerics(variableName);

            if (formatter != null)
            {
                variableName = formatter.Invoke(variableName);
            }

            return typeIsEnumerable
                ? type.IsArray ? variableName + "Array" : Pluralise(variableName)
                : variableName;
        }

        private static string RemoveNonAlphaNumerics(string value)
        {
            // Anonymous types start with non-alphanumeric characters
            while (!char.IsLetterOrDigit(value, 0))
            {
                value = value.Substring(1);
            }

            return value;
        }

        private static string Pluralise(string value)
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

        public static Type GetEnumerableElementType(this Type enumerableType)
        {
            return enumerableType.IsArray
                ? enumerableType.GetElementType()
                : enumerableType.IsGenericType()
                    ? enumerableType.GetGenericArguments().First()
                    : typeof(object);
        }

        public static bool RuntimeTypeNeeded(this Type type)
        {
            return (type == typeof(object)) ||
                   (type == typeof(IEnumerable)) ||
                   (type == typeof(ICollection));
        }

        public static bool IsPublic(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().IsPublic;
#else
            return type.IsPublic;
#endif
        }

        public static bool IsFromBcl(this Type type) => ReferenceEquals(type.GetAssembly(), _msCorLib);

        public static bool IsEnumerable(this Type type)
        {
            return type.IsArray ||
                (type != typeof(string) &&
                typeof(IEnumerable).IsAssignableFrom(type));
        }

        public static bool IsComplex(this Type type)
        {
            return !type.IsSimple() && !type.IsEnumerable();
        }

        public static bool IsSimple(this Type type)
        {
            return type.IsValueType() || (type == typeof(string));
        }

        public static Type GetNonNullableType(this Type type) => Nullable.GetUnderlyingType(type) ?? type;

        public static IEnumerable<Type> GetCoercibleNumericTypes(this Type numericType)
        {
            var typeMaxValue = Constants.NumericTypeMaxValuesByType[numericType];

            return Constants
                .NumericTypeMaxValuesByType
                .Where(kvp => kvp.Value < typeMaxValue)
                .Select(kvp => kvp.Key)
                .ToArray();
        }

        public static bool HasGreaterMaxValueThan(this Type typeOne, Type typeTwo)
        {
            var typeOneMaxValue = GetMaxValueFor(typeOne);
            var typeTwoMaxValue = GetMaxValueFor(typeTwo);

            return typeOneMaxValue > typeTwoMaxValue;
        }

        public static bool HasSmallerMinValueThan(this Type typeOne, Type typeTwo)
        {
            var typeOneMinValue = GetMinValueFor(typeOne);
            var typeTwoMinValue = GetMinValueFor(typeTwo);

            return typeOneMinValue < typeTwoMinValue;
        }

        public static bool IsNumeric(this Type type) => Constants.NumericTypes.Contains(type);

        public static bool IsWholeNumberNumeric(this Type type)
            => Constants.WholeNumberNumericTypes.Contains(type);

        private static double GetMaxValueFor(Type type)
            => GetValueFor(type, Constants.NumericTypeMaxValuesByType, values => values.Max());

        private static double GetMinValueFor(Type type)
            => GetValueFor(type, Constants.NumericTypeMinValuesByType, values => values.Min());

        private static double GetValueFor(
            Type type,
            IDictionary<Type, double> cache,
            Func<IEnumerable<long>, long> enumValueFactory)
        {
            type = type.GetNonNullableType();

            return type.IsEnum() ? enumValueFactory.Invoke(GetEnumValues(type)) : cache[type];
        }

        private static IEnumerable<long> GetEnumValues(Type enumType)
            => Enum.GetValues(enumType).Cast<object>().Select(Convert.ToInt64);

        public static bool StartsWith(this string value, char character) => value[0] == character;

        public static bool EndsWith(this string value, char character) => value[value.Length - 1] == character;
    }
}
