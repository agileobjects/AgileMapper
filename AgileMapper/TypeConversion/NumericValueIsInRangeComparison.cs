namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq.Expressions;
    using Extensions;

    internal static class NumericValueIsInRangeComparison
    {
        public static Expression For(Expression sourceValue, Type targetType)
        {
            var targetNonNullableType = targetType.GetNonNullableUnderlyingTypeIfAppropriate();

            Expression maxValueComparisonLeftSide, maxValueComparisonRightSide;
            GetMaximumValueComparisonOperands(
                sourceValue,
                sourceValue.Type,
                targetNonNullableType,
                out maxValueComparisonLeftSide,
                out maxValueComparisonRightSide);

            Expression minValueComparisonLeftSide, minValueComparisonRightSide;
            GetMinimumValueComparisonOperands(
                sourceValue,
                sourceValue.Type,
                targetNonNullableType,
                out minValueComparisonLeftSide,
                out minValueComparisonRightSide);

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
            Type sourceNonNullableType,
            Type targetNonNullableType,
            out Expression maxValueComparisonLeftSide,
            out Expression maxValueComparisonRightSide)
        {
            var numericMaxValue = Expression.Field(null, targetNonNullableType, "MaxValue");

            if (sourceValue.Type.HasGreaterMaxValueThan(targetNonNullableType))
            {
                maxValueComparisonLeftSide = sourceValue;
                maxValueComparisonRightSide = numericMaxValue.GetConversionTo(sourceNonNullableType);
            }
            else
            {
                maxValueComparisonLeftSide = sourceValue.GetConversionTo(targetNonNullableType);
                maxValueComparisonRightSide = numericMaxValue;
            }
        }

        private static void GetMinimumValueComparisonOperands(
            Expression sourceValue,
            Type sourceNonNullableType,
            Type targetNonNullableType,
            out Expression minValueComparisonLeftSide,
            out Expression minValueComparisonRightSide)
        {
            var numericMinValue = Expression.Field(null, targetNonNullableType, "MinValue");

            if (sourceValue.Type.HasSmallerMinValueThan(targetNonNullableType))
            {
                minValueComparisonLeftSide = sourceValue;
                minValueComparisonRightSide = numericMinValue.GetConversionTo(sourceNonNullableType);
            }
            else
            {
                minValueComparisonLeftSide = sourceValue.GetConversionTo(targetNonNullableType);
                minValueComparisonRightSide = numericMinValue;
            }
        }
    }
}