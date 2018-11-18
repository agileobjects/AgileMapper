namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class NumericValueIsInRangeComparison
    {
        public static Expression For(Expression sourceValue, Type nonNullableTargetType)
        {
            GetMaximumValueComparisonOperands(
                sourceValue,
                sourceValue.Type,
                nonNullableTargetType,
                out var maxValueComparisonLeftSide,
                out var maxValueComparisonRightSide);

            var sourceValueIsLessThanOrEqualToMaxValue =
                Expression.LessThanOrEqual(maxValueComparisonLeftSide, maxValueComparisonRightSide);

            if (sourceValue.Type.GetNonNullableType().IsUnsignedNumeric())
            {
                return sourceValueIsLessThanOrEqualToMaxValue;
            }

            GetMinimumValueComparisonOperands(
                sourceValue,
                sourceValue.Type,
                nonNullableTargetType,
                out var minValueComparisonLeftSide,
                out var minValueComparisonRightSide);

            var sourceValueIsGreaterThanOrEqualToMinValue =
                Expression.GreaterThanOrEqual(minValueComparisonLeftSide, minValueComparisonRightSide);

            var sourceValueIsInRange =
                Expression.AndAlso(sourceValueIsGreaterThanOrEqualToMinValue, sourceValueIsLessThanOrEqualToMaxValue);

            return sourceValueIsInRange;
        }

        private static void GetMaximumValueComparisonOperands(
            Expression sourceValue,
            Type nonNullableSourceType,
            Type nonNullableTargetType,
            out Expression maxValueComparisonLeftSide,
            out Expression maxValueComparisonRightSide)
        {
            var numericMaxValue = GetValueConstant(nonNullableTargetType, "MaxValue");

            if (sourceValue.Type.HasGreaterMaxValueThan(nonNullableTargetType))
            {
                maxValueComparisonLeftSide = sourceValue;
                maxValueComparisonRightSide = numericMaxValue.GetConversionTo(nonNullableSourceType);
            }
            else
            {
                maxValueComparisonLeftSide = sourceValue.GetConversionTo(nonNullableTargetType);
                maxValueComparisonRightSide = numericMaxValue;
            }
        }

        private static void GetMinimumValueComparisonOperands(
            Expression sourceValue,
            Type nonNullableSourceType,
            Type nonNullableTargetType,
            out Expression minValueComparisonLeftSide,
            out Expression minValueComparisonRightSide)
        {
            var numericMinValue = GetValueConstant(nonNullableTargetType, "MinValue");

            if (sourceValue.Type.HasSmallerMinValueThan(nonNullableTargetType))
            {
                minValueComparisonLeftSide = sourceValue;
                minValueComparisonRightSide = numericMinValue.GetConversionTo(nonNullableSourceType);
            }
            else
            {
                minValueComparisonLeftSide = sourceValue.GetConversionTo(nonNullableTargetType);
                minValueComparisonRightSide = numericMinValue;
            }
        }

        private static ConstantExpression GetValueConstant(Type nonNullableTargetType, string fieldName)
        {
            return nonNullableTargetType
                .GetPublicStaticField(fieldName)
                .GetValue(null)
                .ToConstantExpression(nonNullableTargetType);
        }
    }
}