namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    internal static class TypeExtensions
    {
        private static readonly Assembly _msCorLib = typeof(string).GetAssembly();
#if NET35
        private static readonly Assembly _systemCoreLib = typeof(Func<>).GetAssembly();
#endif

        public static string GetShortVariableName(this Type type)
        {
            var variableName = type.GetVariableNameInPascalCase();

            var shortVariableName =
                variableName[0] +
                variableName.ToCharArray().Skip(1).Filter(char.IsUpper).Join(string.Empty);

            shortVariableName = shortVariableName.ToLowerInvariant();

            return (!type.IsArray && type.IsEnumerable())
                ? shortVariableName.Pluralise()
                : shortVariableName;
        }

        public static string GetVariableNameInCamelCase(this Type type) => GetVariableName(type).ToCamelCase();

        public static string GetVariableNameInPascalCase(this Type type) => GetVariableName(type).ToPascalCase();

        private static string GetVariableName(Type type)
        {
            if (type.IsArray)
            {
                return GetVariableName(type.GetElementType()) + "Array";
            }

            var typeIsEnumerable = type.IsEnumerable();
            var typeIsDictionary = typeIsEnumerable && type.IsDictionary();
            var namingType = (typeIsEnumerable && !typeIsDictionary) ? type.GetEnumerableElementType() : type;
            var variableName = GetBaseVariableName(namingType);

            if (namingType.IsInterface())
            {
                variableName = variableName.Substring(1);
            }

            if (namingType.IsGenericType())
            {
                variableName = GetGenericTypeVariableName(variableName, namingType);
            }

            variableName = RemoveLeadingNonAlphaNumerics(variableName);

            return (typeIsDictionary || !typeIsEnumerable) ? variableName : variableName.Pluralise();
        }

        private static string GetBaseVariableName(Type namingType)
            => namingType.IsPrimitive() ? namingType.GetFriendlyName() : namingType.Name;

        private static string GetGenericTypeVariableName(string variableName, Type namingType)
        {
            var nonNullableType = namingType.GetNonNullableType();
            var genericTypeArguments = namingType.GetGenericTypeArguments();

            if (nonNullableType != namingType)
            {
                return "nullable" + genericTypeArguments[0].GetVariableNameInPascalCase();
            }

            variableName = variableName.Substring(0, variableName.IndexOf('`'));

            variableName += genericTypeArguments
                .Project(arg => "_" + arg.GetVariableNameInPascalCase())
                .Join(string.Empty);

            return variableName;
        }

        private static string RemoveLeadingNonAlphaNumerics(string value)
        {
            // Anonymous types start with non-alphanumeric characters
            while (!char.IsLetterOrDigit(value, 0))
            {
                value = value.Substring(1);
            }

            return value;
        }

        public static Type GetEnumerableElementType(this Type enumerableType)
        {
            if (enumerableType.HasElementType)
            {
                return enumerableType.GetElementType();
            }

            if (enumerableType.IsGenericType())
            {
                return enumerableType.GetGenericTypeArguments().Last();
            }

            var enumerableInterfaceType = enumerableType
                .GetAllInterfaces()
                .FirstOrDefault(interfaceType => interfaceType.IsClosedTypeOf(typeof(IEnumerable<>)));

            return enumerableInterfaceType?.GetGenericTypeArguments().First() ?? typeof(object);
        }

        public static bool RuntimeTypeNeeded(this Type type)
        {
            return (type == typeof(object)) ||
                   (type == typeof(IEnumerable)) ||
                   (type == typeof(ICollection));
        }

        public static bool IsFromBcl(this Type type)
        {
            return ReferenceEquals(type.GetAssembly(), _msCorLib)
#if NET35
                || ReferenceEquals(type.GetAssembly(), _systemCoreLib)
#endif
                ;
        }

        public static bool IsEnumerable(this Type type)
        {
            return type.IsArray ||
                  (type != typeof(string) &&
                   type.IsAssignableTo(typeof(IEnumerable)));
        }

        public static bool IsQueryable(this Type type) => type.IsClosedTypeOf(typeof(IQueryable<>));

        public static bool IsComplex(this Type type) => !type.IsSimple() && !type.IsEnumerable();

        public static bool IsSimple(this Type type)
        {
            type = type.GetNonNullableType();

            if (type == typeof(ValueType))
            {
                return true;
            }

            if (type.GetTypeCode() != NetStandardTypeCode.Object)
            {
                return true;
            }

            if ((type == typeof(Guid)) ||
                (type == typeof(TimeSpan)) ||
                (type == typeof(DateTimeOffset)))
            {
                return true;
            }

            return type.IsValueType() && type.IsFromBcl();
        }

        public static bool IsDictionary(this Type type) => IsDictionary(type, out _);

        public static bool IsDictionary(this Type type, out KeyValuePair<Type, Type> keyAndValueTypes)
        {
            keyAndValueTypes = GetDictionaryTypes(type);

            return !keyAndValueTypes.Equals(default(KeyValuePair<Type, Type>));
        }

        public static KeyValuePair<Type, Type> GetDictionaryTypes(this Type type)
        {
            var dictionaryType = GetDictionaryType(type);

            return (dictionaryType != null)
                ? GetDictionaryTypesFrom(dictionaryType)
                : default(KeyValuePair<Type, Type>);
        }

        public static Type GetDictionaryType(this Type type)
        {
            if (type.IsGenericType())
            {
                var typeDefinition = type.GetGenericTypeDefinition();

                if ((typeDefinition == typeof(Dictionary<,>)) || (typeDefinition == typeof(IDictionary<,>)))
                {
                    return type;
                }
            }

            var interfaceType = type
                .GetAllInterfaces()
                .FirstOrDefault(t => t.IsClosedTypeOf(typeof(IDictionary<,>)));

            return interfaceType;
        }

        private static KeyValuePair<Type, Type> GetDictionaryTypesFrom(Type type)
        {
            var types = type.GetGenericTypeArguments();
            return new KeyValuePair<Type, Type>(types[0], types[1]);
        }

        [DebuggerStepThrough]
        public static Type GetNonNullableType(this Type type) => Nullable.GetUnderlyingType(type) ?? type;

        public static Type[] GetCoercibleNumericTypes(this Type numericType)
        {
            var typeMaxValue = Constants.NumericTypeMaxValuesByType[numericType];

            return Constants
                .NumericTypeMaxValuesByType
                .Filter(kvp => kvp.Value < typeMaxValue)
                .Project(kvp => kvp.Key)
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

        public static bool IsUnsignedNumeric(this Type type)
            => Constants.UnsignedTypes.Contains(type);

        public static bool IsWholeNumberNumeric(this Type type)
            => Constants.WholeNumberNumericTypes.Contains(type);

        public static bool IsNonWholeNumberNumeric(this Type type)
            => IsNumeric(type) && !IsWholeNumberNumeric(type);

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
            => Enum.GetValues(enumType).Cast<object>().Project(Convert.ToInt64);

        public static bool StartsWith(this string value, char character) => value[0] == character;

        public static bool EndsWith(this string value, char character) => value[value.Length - 1] == character;

        public static bool CannotBeNull(this Type type) => !type.CanBeNull();
    }
}
