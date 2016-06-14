namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Members;
    using ReadableExpressions;
    internal static class TypeExtensions
    {
        public static string GetShortVariableName(this Type type)
        {
            var variableName = type.GetVariableName();

            var shortVariableName =
                variableName[0] +
                string.Join(
                    string.Empty,
                    variableName.ToCharArray().Skip(1).Where(char.IsUpper));

            return Pluralise(shortVariableName.ToLowerInvariant());
        }

        public static string GetVariableName(
            this Type type,
            Func<VariableFormatterSelector, Func<string, string>> formatter = null)
        {
            var typeIsEnumerable = type.IsEnumerable();
            var namingType = typeIsEnumerable ? type.GetEnumerableElementType() : type;
            var variableName = namingType.Name;

            if (namingType.IsInterface)
            {
                variableName = variableName.Substring(1);
            }

            if (namingType.IsGenericType)
            {
                variableName = variableName.Substring(0, variableName.IndexOf('`'));

                variableName += string.Join(
                    string.Empty,
                    namingType.GetGenericArguments().Select(arg => "_" + arg.GetVariableName(f => f.InPascalCase)));
            }

            if (formatter != null)
            {
                variableName = formatter.Invoke(VariableFormatterSelector.Instance).Invoke(variableName);
            }

            return typeIsEnumerable ? Pluralise(variableName) : variableName;
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

        #region VariableFormatterSelector

        public class VariableFormatterSelector
        {
            internal static readonly VariableFormatterSelector Instance = new VariableFormatterSelector();

            private VariableFormatterSelector()
            {
            }

            public string InCamelCase(string variableName)
            {
                return variableName.ToCamelCase();
            }

            public string InPascalCase(string variableName)
            {
                return variableName.ToPascalCase();
            }
        }

        #endregion

        public static Type GetEnumerableElementType(this Type enumerableType)
        {
            return enumerableType.IsArray
                ? enumerableType.GetElementType()
                : enumerableType.IsGenericType
                    ? enumerableType.GetGenericArguments().First()
                    : typeof(object);
        }

        public static bool CouldHaveADifferentRuntimeType(this Type type)
        {
            return !type.IsValueType &&
                !type.IsArray &&
                !type.IsSealed &&
                (type != typeof(string));
        }

        public static Type GetRuntimeTargetType(this Type declaredType, Type sourceType)
            => declaredType.IsAssignableFrom(sourceType) ? sourceType : declaredType;

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
            return type.IsValueType || (type == typeof(string));
        }

        public static bool CanBeNull(this Type type)
        {
            return type.IsClass || type.IsInterface || type.IsNullableType();
        }

        public static bool IsNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static Type GetNonNullableUnderlyingTypeIfAppropriate(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

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
            type = type.GetNonNullableUnderlyingTypeIfAppropriate();

            return type.IsEnum ? enumValueFactory.Invoke(GetEnumValues(type)) : cache[type];
        }

        private static IEnumerable<long> GetEnumValues(Type enumType)
            => Enum.GetValues(enumType).Cast<object>().Select(Convert.ToInt64);

        public static Member CreateElementMember(this Type enumerableType)
        {
            return new Member(
                MemberType.EnumerableElement,
                Constants.EnumerableElementMemberName,
                enumerableType,
                enumerableType.GetEnumerableElementType());
        }
    }
}
