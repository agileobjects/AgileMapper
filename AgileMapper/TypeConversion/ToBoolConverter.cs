namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Extensions.Internal;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal struct ToBoolConverter : IValueConverter
    {
        public bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
        {
            return (nonNullableTargetType == typeof(bool)) &&
                  ((nonNullableSourceType == typeof(bool)) ||
                   Constants.NumericTypes.Contains(nonNullableSourceType) ||
                    ToStringConverter.HasNativeStringRepresentation(nonNullableSourceType));
        }

        public Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type == typeof(bool?))
            {
                return sourceValue.GetValueOrDefaultCall();
            }

            var nonNullableSourceType = sourceValue.Type.GetNonNullableType();

            var trueValues = GetTrueValues(sourceValue.Type, nonNullableSourceType);
            var sourceEqualsTrueTests = GetSourceEqualsTests(sourceValue, trueValues);

            if (!targetType.IsNullableType())
            {
                return sourceEqualsTrueTests;
            }

            var falseValues = GetFalseValues(sourceValue.Type, nonNullableSourceType);
            var sourceEqualsFalseTests = GetSourceEqualsTests(sourceValue, falseValues);

            var sourceValueConversion = Expression.Condition(
                sourceEqualsTrueTests,
                true.ToConstantExpression(typeof(bool?)),
                Expression.Condition(
                    sourceEqualsFalseTests,
                    false.ToConstantExpression(typeof(bool?)),
                    typeof(bool?).ToDefaultExpression()));

            return sourceValueConversion;
        }

        private static IEnumerable<ConstantExpression> GetTrueValues(Type sourceValueType, Type nonNullableSourceType)
            => GetValues(sourceValueType, nonNullableSourceType, "true", 1);

        private static IEnumerable<ConstantExpression> GetFalseValues(Type sourceValueType, Type nonNullableSourceType)
            => GetValues(sourceValueType, nonNullableSourceType, "false", 0);

        private static IEnumerable<ConstantExpression> GetValues(
            Type sourceValueType,
            Type nonNullableSourceType,
            string textValue,
            int numericValue)
        {
            if ((sourceValueType == typeof(string)) || (sourceValueType == typeof(object)))
            {
                yield return GetConstant(numericValue.ToString(CultureInfo.InvariantCulture));
                yield return GetConstant(textValue);
                yield break;
            }

            if (nonNullableSourceType == typeof(char))
            {
                yield return GetConstant(numericValue.ToString(CultureInfo.InvariantCulture)[0], sourceValueType);
                yield break;
            }

            yield return GetConstant(Convert.ChangeType(numericValue, nonNullableSourceType), sourceValueType);
        }

        private static ConstantExpression GetConstant<T>(T value, Type valueType = null)
        {
            if (valueType == null)
            {
                valueType = typeof(T);
            }

            return value.ToConstantExpression(valueType);
        }

        private static Expression GetSourceEqualsTests(Expression sourceValue, IEnumerable<ConstantExpression> values)
        {
            return values.ToArray().Chain(
                firstValue => GetValueTest(sourceValue, firstValue),
                (testsSoFar, testValue) => Expression.OrElse(testsSoFar, GetValueTest(sourceValue, testValue)));
        }

        private static Expression GetValueTest(Expression sourceValue, ConstantExpression testValue)
        {
            if (sourceValue.Type != testValue.Type)
            {
                sourceValue = ToStringConverter.GetConversion(sourceValue);
            }

            var test = testValue.Value.ToString().Length == 1
                ? Expression.Equal(sourceValue, testValue)
                : sourceValue.GetCaseInsensitiveEquals(testValue);

            return test;
        }
    }
}