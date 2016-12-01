namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using ReadableExpressions.Extensions;

    internal class ToBoolConverter : ValueConverterBase
    {
        private static readonly Type[] _supportedSourceTypes = Constants
            .NumericTypes
            .Concat(typeof(bool?), typeof(string), typeof(char))
            .ToArray();

        public override bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
            => (nonNullableTargetType == typeof(bool)) && _supportedSourceTypes.Contains(nonNullableSourceType);

        public override Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type == typeof(bool?))
            {
                return GetNullableBoolConversion(sourceValue, targetType);
            }

            var nonNullableSourceType = sourceValue.Type.GetNonNullableType();
            var comparisonValues = GetComparisons(sourceValue, nonNullableSourceType);

            var sourceEqualsTrueValue = Expression.Equal(sourceValue, comparisonValues.Item1);

            if (!targetType.IsNullableType())
            {
                return sourceEqualsTrueValue;
            }

            var sourceEqualsFalseValue = Expression.Equal(sourceValue, comparisonValues.Item2);

            var sourceValueConversion = Expression.Condition(
                sourceEqualsTrueValue,
                Expression.Constant(true, typeof(bool?)),
                Expression.Condition(
                    sourceEqualsFalseValue,
                    Expression.Constant(false, typeof(bool?)),
                    Expression.Default(typeof(bool?))));

            return sourceValueConversion;
        }

        private static Expression GetNullableBoolConversion(Expression sourceValue, Type targetType)
        {
            if (targetType == typeof(bool))
            {
                return sourceValue.GetValueOrDefaultCall();
            }

            return Expression.Condition(
                Expression.Property(sourceValue, "HasValue"),
                Expression.Property(sourceValue, "Value"),
                Expression.Default(typeof(bool?)));
        }

        private static Tuple<Expression, Expression> GetComparisons(Expression sourceValue, Type nonNullableSourceType)
        {
            if (sourceValue.Type == typeof(string))
            {
                return GetComparisonsTuple("1", "0");
            }

            if (nonNullableSourceType == typeof(char))
            {
                return GetComparisonsTuple('1', '0', sourceValue.Type);
            }

            return GetComparisonsTuple(
                Convert.ChangeType(1, nonNullableSourceType),
                Convert.ChangeType(0, nonNullableSourceType),
                sourceValue.Type);
        }

        private static Tuple<Expression, Expression> GetComparisonsTuple<T>(
            T trueValue,
            T falseValue,
            Type valueType = null)
        {
            if (valueType == null)
            {
                valueType = typeof(T);
            }

            return Tuple.Create(
                (Expression)Expression.Constant(trueValue, valueType),
                (Expression)Expression.Constant(falseValue, valueType));
        }


    }
}