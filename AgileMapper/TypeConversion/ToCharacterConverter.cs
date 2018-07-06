namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq;
    using Extensions.Internal;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal struct ToCharacterConverter : IValueConverter
    {
        public bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
        {
            return (nonNullableTargetType == typeof(char)) &&
                   (nonNullableSourceType.IsEnum() ||
                    (nonNullableSourceType == typeof(char)) ||
                    (nonNullableSourceType == typeof(string)) ||
                    (nonNullableSourceType == typeof(object)) ||
                    Constants.NumericTypes.Contains(nonNullableSourceType));
        }

        public Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type == typeof(char?))
            {
                return sourceValue.GetValueOrDefaultCall();
            }

            if (sourceValue.Type == typeof(object))
            {
                sourceValue = ToStringConverter.GetConversion(sourceValue);
            }

            if (sourceValue.Type == typeof(string))
            {
                return GetFromStringConversion(sourceValue, targetType);
            }

            if (sourceValue.Type.GetNonNullableType().IsEnum())
            {
                sourceValue = sourceValue.GetConversionTo<int>();
            }

            return GetFromNumericConversion(sourceValue, targetType);
        }

        private static Expression GetFromStringConversion(Expression sourceValue, Type targetType)
        {
            var sourceLength = Expression.Property(sourceValue, "Length");
            var lengthIsOne = Expression.Equal(sourceLength, ToNumericConverter<int>.One);

            var stringIndexer = typeof(string).GetPublicInstanceProperty("Chars");
            var elementZero = new[] { ToNumericConverter<int>.Zero };
            var zeroethCharacter = Expression.MakeIndex(sourceValue, stringIndexer, elementZero);
            var typedZeroeth = zeroethCharacter.GetConversionTo(targetType);
            var fallbackValue = targetType.ToDefaultExpression();

            var zeroethOrDefault = Expression.Condition(lengthIsOne, typedZeroeth, fallbackValue);

            return zeroethOrDefault;
        }

        private static Expression GetFromNumericConversion(Expression sourceValue, Type targetType)
        {
            var isWholeNumberNumeric = sourceValue.Type.GetNonNullableType().IsWholeNumberNumeric();
            var sourceValueIsValid = GetIsValidNumericValueCheck(sourceValue, isWholeNumberNumeric);

            var fallbackValue = targetType.ToDefaultExpression();
            var stringValue = ToStringConverter.GetConversion(sourceValue);

            if (!isWholeNumberNumeric)
            {
                stringValue = stringValue.GetFirstOrDefaultCall();
            }

            var convertedStringValue = GetFromStringConversion(stringValue, targetType);

            return Expression.Condition(sourceValueIsValid, fallbackValue, convertedStringValue);
        }

        private static Expression GetIsValidNumericValueCheck(Expression sourceValue, bool isWholeNumberNumeric)
        {
            var rangeCheck = GetNumericRangeCheck(sourceValue);

            if (isWholeNumberNumeric)
            {
                return rangeCheck;
            }

            var isNotWholeNumber = NumericConversions.GetModuloOneIsNotZeroCheck(sourceValue);

            return Expression.OrElse(rangeCheck, isNotWholeNumber);
        }

        private static Expression GetNumericRangeCheck(Expression sourceValue)
        {
            var nine = NumericConversions.GetConstantValue(9, sourceValue);
            var sourceGreaterThanNine = Expression.GreaterThan(sourceValue, nine);

            if (sourceValue.Type.IsUnsignedNumeric())
            {
                return sourceGreaterThanNine;
            }

            var minusNine = NumericConversions.GetConstantValue(-9, sourceValue);
            var sourceLessThanMinusNine = Expression.LessThan(sourceValue, minusNine);

            return Expression.OrElse(sourceGreaterThanNine, sourceLessThanMinusNine);
        }
    }
}