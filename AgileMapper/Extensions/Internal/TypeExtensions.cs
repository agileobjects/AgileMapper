namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
#if FEATURE_DRAWING
    using System.Drawing;
#endif
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using Caching.Dictionaries;
    using Members;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
    using static Members.Member;

    internal static class TypeExtensions
    {
        private static readonly Assembly[] _bclAssemblies =
        {
            typeof(string).GetAssembly(),
#if FEATURE_DRAWING
            typeof(Image).GetAssembly(),
#endif
#if NET35
            typeof(Func<>).GetAssembly()
#endif
        };

        public static ParameterExpression GetOrCreateSourceParameter(this Type type)
            => type.GetOrCreateParameter(RootSourceMemberName);

        public static ParameterExpression GetOrCreateTargetParameter(this Type type)
            => type.GetOrCreateParameter(RootTargetMemberName);

        public static ParameterExpression GetOrCreateParameter(this Type type, string name = null)
        {
            if (type == null)
            {
                return null;
            }

            var cache = GlobalContext.Instance.Cache
                .CreateScopedWithHashCodes<TypeKey, ParameterExpression>();

            var parameter = cache.GetOrAdd(
                TypeKey.ForParameter(type, name),
                key => Parameters.Create(key.Type, key.Name));

            return parameter;
        }

        public static string GetSourceValueVariableName(this Type sourceType)
            => RootSourceMemberName + sourceType.GetVariableNameInPascalCase();

        public static string GetShortVariableName(this Type type)
        {
            var variableName = type.GetVariableNameInPascalCase();

            var shortVariableName =
                variableName[0] +
                variableName.ToCharArray().Skip(1).Filter(char.IsUpper).Join(string.Empty);

            return shortVariableName.ToLowerInvariant();
        }

        public static bool RuntimeTypeNeeded(this Type type)
        {
            return (type == typeof(object)) ||
                   (type == typeof(IEnumerable)) ||
                   (type == typeof(ICollection));
        }

        public static bool IsFromBcl(this Type type) => _bclAssemblies.Contains(type.GetAssembly());

        public static bool IsQueryable(this Type type) => type.IsClosedTypeOf(typeof(IQueryable<>));

        public static bool IsComplex(this Type type) => !type.IsSimple() && !type.IsEnumerable();

        public static bool AreForCallback(this ICollection<Type> contextTypes)
            => contextTypes.Count == 2;

        public static Type[] GetCoercibleNumericTypes(this Type numericType)
        {
            var typeMaxValue = Constants.NumericTypeMaxValuesByType[numericType];

            return Constants
                .NumericTypeMaxValuesByType
                .Filter(typeMaxValue, (tmv, kvp) => kvp.Value < tmv)
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
            ISimpleDictionary<Type, double> cache,
            Func<IEnumerable<long>, long> enumValueFactory)
        {
            type = type.GetNonNullableType();

            return type.IsEnum() ? enumValueFactory.Invoke(GetEnumValuesArray(type, Convert.ToInt64)) : cache[type];
        }

        public static TResult[] GetEnumValuesArray<TResult>(this Type enumType, Func<object, TResult> resultFactory)
        {
            var values = Enum.GetValues(enumType);
            var valueCount = values.Length;

            if (valueCount == 0)
            {
                return Enumerable<TResult>.EmptyArray;
            }

            var resultValues = new TResult[valueCount];
            var i = 0;

            foreach (var value in values)
            {
                resultValues[i++] = resultFactory.Invoke(value);
            }

            return resultValues;
        }

        public static bool StartsWith(this string value, char character) => value[0] == character;

        public static bool EndsWith(this string value, char character) => value[value.Length - 1] == character;

        public static bool CannotBeNull(this Type type) => !type.CanBeNull();

        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Type type)
            where TAttribute : Attribute
        {
#if NETSTANDARD1_0 || NETSTANDARD1_3
            return type.GetTypeInfo().GetCustomAttributes<TAttribute>();
#else
            return type.GetCustomAttributes(typeof(TAttribute), inherit: false).Project(attr => (TAttribute)attr);
#endif
        }
    }
}
