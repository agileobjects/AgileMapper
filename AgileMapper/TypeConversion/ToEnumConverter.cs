namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using NetStandardPolyfills;

    internal class ToEnumConverter : ValueConverterBase
    {
        private readonly ToStringConverter _toStringConverter;

        public ToEnumConverter(ToStringConverter toStringConverter)
        {
            _toStringConverter = toStringConverter;
        }

        public override bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
        {
            if (!nonNullableTargetType.IsEnum())
            {
                return false;
            }

            return nonNullableSourceType.IsEnum() ||
                (nonNullableSourceType == typeof(string)) ||
                (nonNullableSourceType == typeof(char)) ||
                nonNullableSourceType.IsNumeric();
        }

        public override Expression GetConversion(Expression sourceValue, Type targetType)
        {
            if (sourceValue.Type != typeof(string))
            {
                sourceValue = _toStringConverter.GetConversion(sourceValue);
            }

            var nonNullableEnumType = targetType.GetNonNullableType();

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