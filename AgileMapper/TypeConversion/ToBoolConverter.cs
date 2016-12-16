namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using ReadableExpressions.Extensions;

    internal class ToBoolConverter : ValueConverterBase
    {
        private static readonly Type[] _supportedSourceTypes = Constants
            .NumericTypes
            .Concat(typeof(bool), typeof(string), typeof(object), typeof(char))
            .ToArray();

        private readonly ToStringConverter _toStringConverter;

        public ToBoolConverter(ToStringConverter toStringConverter)
        {
            _toStringConverter = toStringConverter;
        }

        public override bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
            => (nonNullableTargetType == typeof(bool)) && _supportedSourceTypes.Contains(nonNullableSourceType);

        public override Expression GetConversion(Expression sourceValue, Type targetType)
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
                Expression.Constant(true, typeof(bool?)),
                Expression.Condition(
                    sourceEqualsFalseTests,
                    Expression.Constant(false, typeof(bool?)),
                    Expression.Default(typeof(bool?))));

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

            return Expression.Constant(value, valueType);
        }

        private Expression GetSourceEqualsTests(Expression sourceValue, IEnumerable<ConstantExpression> values)
        {
            return values.ToArray().Chain(
                firstValue => GetValueTest(sourceValue, firstValue),
                (testsSoFar, testValue) => Expression.OrElse(testsSoFar, GetValueTest(sourceValue, testValue)));
        }

        private Expression GetValueTest(Expression sourceValue, ConstantExpression testValue)
        {
            if (sourceValue.Type == typeof(object))
            {
                sourceValue = _toStringConverter.GetConversion(sourceValue);
            }

            var test = testValue.Value.ToString().Length == 1
                ? Expression.Equal(sourceValue, testValue)
                : sourceValue.GetCaseInsensitiveEquals(testValue);

            return test;
        }
    }
}