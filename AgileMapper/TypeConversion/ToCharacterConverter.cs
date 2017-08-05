namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Extensions;
    using NetStandardPolyfills;

    internal class ToCharacterConverter : ValueConverterBase
    {
        private static readonly Type[] _handledSourceTypes =
            Constants.NumericTypes
                .Concat(typeof(char), typeof(string), typeof(object))
                .ToArray();

        private readonly ToStringConverter _toStringConverter;

        public ToCharacterConverter(ToStringConverter toStringConverter)
        {
            _toStringConverter = toStringConverter;
        }

        public override bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
        {
            return (nonNullableTargetType == typeof(char)) &&
                   (nonNullableSourceType.IsEnum() || _handledSourceTypes.Contains(nonNullableSourceType));
        }

        public override Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type == typeof(char?))
            {
                return sourceValue.GetValueOrDefaultCall();
            }

            if (sourceValue.Type == typeof(object))
            {
                sourceValue = _toStringConverter.GetConversion(sourceValue);
            }

            if (sourceValue.Type == typeof(string))
            {
                return GetFromStringConversion(sourceValue, targetType);
            }

            var nonNullableType = sourceValue.Type.GetNonNullableType();

            if (nonNullableType.IsEnum())
            {
                // TODO: Nullable enum

                sourceValue = sourceValue.GetConversionTo(typeof(int));
            }

            return GetFromNumericConversion(sourceValue, targetType);
        }

        private static Expression GetFromStringConversion(Expression sourceValue, Type targetType)
        {
            var sourceLength = Expression.Property(sourceValue, "Length");
            var lengthIsOne = Expression.Equal(sourceLength, 1.ToConstantExpression());

            var stringIndexer = typeof(string).GetProperty("Chars");
            var elementZero = new[] { 0.ToConstantExpression() };
            var zeroethCharacter = Expression.MakeIndex(sourceValue, stringIndexer, elementZero);
            var typedZeroeth = zeroethCharacter.GetConversionTo(targetType);
            var fallbackValue = targetType.ToDefaultExpression();

            var zeroethOrDefault = Expression.Condition(lengthIsOne, typedZeroeth, fallbackValue);

            return zeroethOrDefault;
        }

        private Expression GetFromNumericConversion(Expression sourceValue, Type targetType)
        {
            var isWholeNumberNumeric = sourceValue.Type.GetNonNullableType().IsWholeNumberNumeric();
            var sourceValueIsValid = GetIsValidNumericValueCheck(sourceValue, isWholeNumberNumeric);

            var fallbackValue = targetType.ToDefaultExpression();
            var stringValue = _toStringConverter.GetConversion(sourceValue);

            if (!isWholeNumberNumeric)
            {
                stringValue = stringValue.GetLeftCall(numberOfCharacters: 1);
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