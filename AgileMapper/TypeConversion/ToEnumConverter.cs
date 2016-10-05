namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using ReadableExpressions.Extensions;

    internal class ToEnumConverter : IValueConverter
    {
        private readonly ToStringConverter _toStringConverter;

        public ToEnumConverter(ToStringConverter toStringConverter)
        {
            _toStringConverter = toStringConverter;
        }

        public bool IsFor(Type nonNullableTargetType)
        {
            return nonNullableTargetType.IsEnum();
        }

        public bool CanConvert(Type nonNullableSourceType)
        {
            return nonNullableSourceType.IsEnum() ||
                (nonNullableSourceType == typeof(string)) ||
                (nonNullableSourceType == typeof(char)) ||
                nonNullableSourceType.IsNumeric();
        }

        public Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type != typeof(string))
            {
                sourceValue = _toStringConverter.GetConversion(sourceValue);
            }

            var nonNullableEnumType = targetType.GetNonNullableUnderlyingTypeIfAppropriate();

            var tryParseMethod = typeof(Enum)
                .GetPublicStaticMethods()
                .First(m => (m.Name == "TryParse") && (m.GetParameters().Length == 3))
                .MakeGenericMethod(nonNullableEnumType);

            var valueVariable = Expression.Variable(nonNullableEnumType, nonNullableEnumType.GetShortVariableName());

            var tryParseCall = Expression.Call(
                tryParseMethod,
                sourceValue,
                Expression.Constant(true, typeof(bool)), // <- IgnoreCase
                valueVariable);

            var isDefinedCall = Expression.Call(
                null,
                typeof(Enum).GetPublicStaticMethod("IsDefined"),
                Expression.Constant(nonNullableEnumType),
                valueVariable.GetConversionTo(typeof(object)));

            var successfulParseReturnValue = valueVariable.GetConversionTo(targetType);
            var defaultValue = Expression.Default(targetType);

            var definedValueOrDefault = Expression.Condition(isDefinedCall, successfulParseReturnValue, defaultValue);
            var parsedValueOrDefault = Expression.Condition(tryParseCall, definedValueOrDefault, defaultValue);
            var tryParseBlock = Expression.Block(new[] { valueVariable }, parsedValueOrDefault);

            return tryParseBlock;
        }
    }
}