namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using Extensions.Internal;
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
                out Expression maxValueComparisonLeftSide,
                out Expression maxValueComparisonRightSide);

            GetMinimumValueComparisonOperands(
                sourceValue,
                sourceValue.Type,
                nonNullableTargetType,
                out Expression minValueComparisonLeftSide,
                out Expression minValueComparisonRightSide);

            var sourceValueIsLessThanOrEqualToMaxValue =
                Expression.LessThanOrEqual(maxValueComparisonLeftSide, maxValueComparisonRightSide);

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
            var numericMaxValue = Expression.Field(null, nonNullableTargetType, "MaxValue");

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
            var numericMinValue = Expression.Field(null, nonNullableTargetType, "MinValue");

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
    }
}