﻿namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using NetStandardPolyfills;

    internal class ToNumericConverter<TNumeric> : TryParseConverter<TNumeric>
    {
        #region Cached Items

        private static readonly Type[] _coercibleNumericTypes =
            typeof(TNumeric)
                .GetCoercibleNumericTypes()
                .ToArray();

        // ReSharper disable StaticMemberInGenericType
        public static readonly Expression One = GetNumericConstant(1);
        public static readonly Expression Zero = GetNumericConstant(0);
        // ReSharper restore StaticMemberInGenericType

        private static Expression GetNumericConstant(int value)
        {
            if (typeof(TNumeric) == typeof(int))
            {
                return value.ToConstantExpression();
            }

            return Convert
                .ChangeType(value, typeof(TNumeric))
                .ToConstantExpression(typeof(TNumeric));
        }

        #endregion

        public ToNumericConverter(ToStringConverter toStringConverter)
            : base(toStringConverter)
        {
        }

        protected override bool CanConvert(Type nonNullableSourceType)
        {
            return base.CanConvert(nonNullableSourceType) ||
                  (nonNullableSourceType == typeof(bool)) ||
                   Constants.NumericTypes.Contains(nonNullableSourceType);
        }

        public override Expression GetConversion(Expression sourceValue, Type targetType)
        {
            var sourceType = GetNonEnumSourceType(sourceValue);
            var nonNullableSourceType = sourceType.GetNonNullableType();

            if (nonNullableSourceType == typeof(bool))
            {
                return GetBoolToNumericConversion(sourceValue, targetType);
            }

            if (IsCoercible(nonNullableSourceType))
            {
                if (!targetType.IsWholeNumberNumeric())
                {
                    sourceValue = sourceValue.GetConversionTo(sourceType);
                }

                return sourceValue.GetConversionTo(targetType);
            }

            return IsNumericType(nonNullableSourceType)
                ? GetCheckedNumericConversion(sourceValue, targetType)
                : base.GetConversion(sourceValue, targetType);
        }

        private static Type GetNonEnumSourceType(Expression sourceValue)
            => sourceValue.Type.IsEnum() ? Enum.GetUnderlyingType(sourceValue.Type) : sourceValue.Type;

        private static Expression GetBoolToNumericConversion(Expression sourceValue, Type targetType)
        {
            var sourceIsNotNullable = sourceValue.Type == typeof(bool);

            var testValue = sourceIsNotNullable
                ? sourceValue
                : sourceValue.GetConversionTo<bool>();

            var boolConversion = Expression.Condition(
                Expression.IsTrue(testValue),
                One.GetConversionTo(targetType),
                Zero.GetConversionTo(targetType));

            if (sourceIsNotNullable)
            {
                return boolConversion;
            }

            return Expression.Condition(
                sourceValue.GetIsNotDefaultComparison(),
                boolConversion,
                boolConversion.Type.ToDefaultExpression());
        }

        private static bool IsCoercible(Type sourceType) => _coercibleNumericTypes.Contains(sourceType);

        private static bool IsNumericType(Type type)
        {
            if ((type == typeof(string)) ||
                (type == typeof(object)) ||
                (type == typeof(char)) ||
                (type == typeof(char?)))
            {
                return false;
            }

            return Constants.NumericTypes.Contains(type);
        }

        private static Expression GetCheckedNumericConversion(Expression sourceValue, Type targetType)
        {
            var castSourceValue = sourceValue.GetConversionTo(targetType);

            if (sourceValue.Type.GetNonNullableType() == targetType.GetNonNullableType())
            {
                return castSourceValue;
            }

            var numericValueIsValid = GetNumericValueValidityCheck(sourceValue, targetType);
            var defaultTargetType = targetType.ToDefaultExpression();
            var inRangeValueOrDefault = Expression.Condition(numericValueIsValid, castSourceValue, defaultTargetType);

            return inRangeValueOrDefault;
        }

        private static Expression GetNumericValueValidityCheck(Expression sourceValue, Type targetType)
        {
            var nonNullableTargetType = targetType.GetNonNullableType();
            var numericValueIsInRange = NumericValueIsInRangeComparison.For(sourceValue, nonNullableTargetType);

            if (NonWholeNumberCheckIsNotRequired(sourceValue, nonNullableTargetType))
            {
                return numericValueIsInRange;
            }

            var moduloOneEqualsZero = NumericConversions.GetModuloOneIsZeroCheck(sourceValue);

            return Expression.AndAlso(numericValueIsInRange, moduloOneEqualsZero);
        }

        private static bool NonWholeNumberCheckIsNotRequired(Expression sourceValue, Type nonNullableTargetType)
        {
            var sourceType = sourceValue.Type.GetNonNullableType();

            return sourceType.IsEnum() ||
                   sourceType.IsWholeNumberNumeric() ||
                  !nonNullableTargetType.IsWholeNumberNumeric();
        }
    }
}