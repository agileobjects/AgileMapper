namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal abstract class ToNumericConverterBase : TryParseConverterBase
    {
        private static readonly Type[] _handledSourceTypes =
            Constants.NumericTypes
                .Concat(typeof(string), typeof(char), typeof(object))
                .ToArray();

        protected ToNumericConverterBase(ToStringConverter toStringConverter, Type numericType)
            : base(toStringConverter, numericType)
        {
        }

        public override bool CanConvert(Type nonNullableSourceType)
        {
            return base.CanConvert(nonNullableSourceType) ||
                   nonNullableSourceType.IsEnum ||
                   _handledSourceTypes.Contains(nonNullableSourceType);
        }

        public override Expression GetConversion(Expression sourceValue, Type targetType)
        {
            var sourceType = GetNonEnumSourceType(sourceValue);

            if (IsCoercible(sourceType))
            {
                if (!targetType.IsWholeNumberNumeric())
                {
                    sourceValue = sourceValue.GetConversionTo(sourceType);
                }

                return sourceValue.GetConversionTo(targetType);
            }

            return IsNonNumericType(sourceValue.Type)
                ? base.GetConversion(sourceValue, targetType)
                : GetCheckedNumericConversion(sourceValue, targetType);
        }

        private static Type GetNonEnumSourceType(Expression sourceValue)
        {
            if (sourceValue.Type.IsEnum)
            {
                switch (Type.GetTypeCode(sourceValue.Type))
                {
                    case TypeCode.Byte:
                        return typeof(byte);

                    case TypeCode.SByte:
                        return typeof(sbyte);

                    case TypeCode.Int16:
                        return typeof(short);

                    case TypeCode.UInt16:
                        return typeof(ushort);

                    case TypeCode.Int32:
                        return typeof(int);

                    case TypeCode.UInt32:
                        return typeof(uint);

                    case TypeCode.Int64:
                        return typeof(long);

                    case TypeCode.UInt64:
                        return typeof(ulong);
                }
            }

            return sourceValue.Type;
        }

        private static bool IsNonNumericType(Type type)
        {
            return (type == typeof(string)) ||
                   (type == typeof(object)) ||
                   (type == typeof(char)) ||
                   (type == typeof(char?));
        }

        protected abstract bool IsCoercible(Type sourceType);

        private static Expression GetCheckedNumericConversion(Expression sourceValue, Type targetType)
        {
            var numericValueIsValid = GetNumericValueValidityCheck(sourceValue, targetType);
            var castSourceValue = sourceValue.GetConversionTo(targetType);
            var defaultTargetType = Expression.Default(targetType);
            var inRangeValueOrDefault = Expression.Condition(numericValueIsValid, castSourceValue, defaultTargetType);

            return inRangeValueOrDefault;
        }

        private static Expression GetNumericValueValidityCheck(Expression sourceValue, Type targetType)
        {
            var nonNullableTargetType = targetType.GetNonNullableUnderlyingTypeIfAppropriate();
            var numericValueIsInRange = NumericValueIsInRangeComparison.For(sourceValue, nonNullableTargetType);

            if (NonWholeNumberCheckIsNotRequired(sourceValue, nonNullableTargetType))
            {
                return numericValueIsInRange;
            }

            var one = GetConstantValue(1, sourceValue);
            var sourceValueModuloOne = Expression.Modulo(sourceValue, one);
            var zero = GetConstantValue(0, sourceValue);
            var moduloOneEqualsZero = Expression.Equal(sourceValueModuloOne, zero);

            return Expression.AndAlso(numericValueIsInRange, moduloOneEqualsZero);
        }

        private static bool NonWholeNumberCheckIsNotRequired(Expression sourceValue, Type nonNullableTargetType)
        {
            return sourceValue.Type.IsEnum ||
                   sourceValue.Type.IsWholeNumberNumeric() ||
                   !nonNullableTargetType.IsWholeNumberNumeric();
        }

        private static Expression GetConstantValue(int value, Expression sourceValue)
        {
            var constant = Expression.Constant(value);

            return (sourceValue.Type != typeof(int))
                ? constant.GetConversionTo(sourceValue.Type) : constant;
        }
    }
}