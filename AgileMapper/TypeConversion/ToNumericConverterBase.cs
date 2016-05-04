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
                .Concat(typeof(string), typeof(char))
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
            if (IsCoercible(sourceValue))
            {
                return sourceValue.GetConversionTo(targetType);
            }

            return IsStringType(sourceValue.Type)
                ? base.GetConversion(sourceValue, targetType)
                : GetCheckedNumericConversion(sourceValue, targetType);
        }

        private static bool IsStringType(Type type)
        {
            return (type == typeof(string)) || (type == typeof(char)) || (type == typeof(char?));
        }

        protected abstract bool IsCoercible(Expression sourceValue);

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
            var numericValueIsInRange = NumericValueIsInRangeComparison.For(sourceValue, targetType);

            if (sourceValue.Type.IsEnum || sourceValue.Type.IsWholeNumberNumeric())
            {
                return numericValueIsInRange;
            }

            var one = GetConstantValue(1, sourceValue);
            var sourceValueModuloOne = Expression.Modulo(sourceValue, one);
            var zero = GetConstantValue(0, sourceValue);
            var moduloOneEqualsZero = Expression.Equal(sourceValueModuloOne, zero);

            return Expression.AndAlso(numericValueIsInRange, moduloOneEqualsZero);
        }

        private static Expression GetConstantValue(int value, Expression sourceValue)
        {
            var constant = Expression.Constant(value);

            return (sourceValue.Type != typeof(int))
                ? constant.GetConversionTo(sourceValue.Type) : constant;
        }
    }
}