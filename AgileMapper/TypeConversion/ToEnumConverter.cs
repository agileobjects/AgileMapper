namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Linq.Expressions;
    using Extensions.Internal;
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
                  (nonNullableSourceType == typeof(object)) ||
                  (nonNullableSourceType == typeof(char)) ||
                   nonNullableSourceType.IsNumeric();
        }

        public override Expression GetConversion(Expression sourceValue, Type targetEnumType)
        {
            var fallbackValue = targetEnumType.ToDefaultExpression();
            var nonNullableTargetEnumType = targetEnumType.GetNonNullableType();

            if (nonNullableTargetEnumType.HasAttribute<FlagsAttribute>())
            {
                return GetFlagsEnumConversion(sourceValue, fallbackValue, nonNullableTargetEnumType);
            }

            var nonNullableSourceType = sourceValue.Type.GetNonNullableType();

            if (nonNullableSourceType.IsNumeric())
            {
                return GetNumericToEnumConversion(
                    sourceValue,
                    fallbackValue,
                    nonNullableSourceType,
                    nonNullableTargetEnumType);
            }

            return GetTryParseConversion(
                sourceValue,
                fallbackValue,
                nonNullableSourceType,
                nonNullableTargetEnumType);
        }

        private static Expression GetNumericToEnumConversion(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableSourceType,
            Type nonNullableTargetEnumType)
        {
            var convertedSourceValue = sourceValue
                .GetConversionTo(Enum.GetUnderlyingType(nonNullableTargetEnumType))
                .GetConversionTo(typeof(object));

            var numericValueIsDefined = Expression.Call(
                typeof(Enum).GetPublicStaticMethod("IsDefined"),
                nonNullableTargetEnumType.ToConstantExpression(),
                convertedSourceValue);

            Expression convertedNumericValue = Expression.Convert(sourceValue, nonNullableTargetEnumType);

            if (nonNullableTargetEnumType != fallbackValue.Type)
            {
                convertedNumericValue = convertedNumericValue.GetConversionTo(fallbackValue.Type);
            }

            var definedValueOrFallback = Expression.Condition(
                numericValueIsDefined,
                convertedNumericValue,
                fallbackValue);

            if (sourceValue.Type == nonNullableSourceType)
            {
                return definedValueOrFallback;
            }

            var nonNullDefinedValueOrFallback = Expression.Condition(
                sourceValue.GetIsNotDefaultComparison(),
                definedValueOrFallback,
                fallbackValue);

            return nonNullDefinedValueOrFallback;
        }

        private Expression GetTryParseConversion(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableSourceType,
            Type nonNullableTargetEnumType)
        {
            bool sourceIsAnEnum;

            if (sourceValue.Type != typeof(string))
            {
                sourceIsAnEnum = nonNullableSourceType.IsEnum();
                sourceValue = _toStringConverter.GetConversion(sourceValue);
            }
            else
            {
                sourceIsAnEnum = false;
            }

            var tryParseMethod = typeof(Enum)
                .GetPublicStaticMethod("TryParse", parameterCount: 3)
                .MakeGenericMethod(nonNullableTargetEnumType);

            var valueVariable = Expression.Variable(
                nonNullableTargetEnumType,
                nonNullableTargetEnumType.GetShortVariableName());

            var tryParseCall = Expression.Call(
                tryParseMethod,
                sourceValue,
                true.ToConstantExpression(), // <- IgnoreCase
                valueVariable);


            var parseSuccessBranch = GetParseSuccessBranch(sourceIsAnEnum, valueVariable, fallbackValue);

            var parsedValueOrDefault = Expression.Condition(tryParseCall, parseSuccessBranch, fallbackValue);
            var tryParseBlock = Expression.Block(new[] { valueVariable }, parsedValueOrDefault);

            return tryParseBlock;
        }

        private static Expression GetParseSuccessBranch(
            bool sourceIsAnEnum,
            Expression valueVariable,
            Expression defaultValue)
        {
            var successfulParseReturnValue = valueVariable.GetConversionTo(defaultValue.Type);

            if (sourceIsAnEnum)
            {
                // Enums are parsed using the member name, so no need to 
                // check if the value is defined if the parse succeeds:
                return successfulParseReturnValue;
            }

            var isDefinedCall = Expression.Call(
                null,
                typeof(Enum).GetPublicStaticMethod("IsDefined"),
                valueVariable.Type.ToConstantExpression(),
                valueVariable.GetConversionTo(typeof(object)));

            var definedValueOrDefault = Expression.Condition(isDefinedCall, successfulParseReturnValue, defaultValue);

            return definedValueOrDefault;
        }

        private Expression GetFlagsEnumConversion(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableTargetEnumType)
        {
            throw new NotImplementedException();
        }
    }
}