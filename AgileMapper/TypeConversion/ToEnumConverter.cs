﻿namespace AgileObjects.AgileMapper.TypeConversion
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

        public override Expression GetConversion(Expression sourceValue, Type targetType)
        {
            bool sourceIsAnEnum;

            if (sourceValue.Type != typeof(string))
            {
                sourceIsAnEnum = sourceValue.Type.GetNonNullableType().IsEnum();
                sourceValue = _toStringConverter.GetConversion(sourceValue);
            }
            else
            {
                sourceIsAnEnum = false;
            }

            var nonNullableEnumType = targetType.GetNonNullableType();

            var tryParseMethod = typeof(Enum)
                .GetPublicStaticMethod("TryParse", parameterCount: 3)
                .MakeGenericMethod(nonNullableEnumType);

            var valueVariable = Expression.Variable(nonNullableEnumType, nonNullableEnumType.GetShortVariableName());

            var tryParseCall = Expression.Call(
                tryParseMethod,
                sourceValue,
                true.ToConstantExpression(), // <- IgnoreCase
                valueVariable);

            var defaultValue = targetType.ToDefaultExpression();
            var parseSuccessBranch = GetParseSuccessBranch(sourceIsAnEnum, valueVariable, defaultValue);

            var parsedValueOrDefault = Expression.Condition(tryParseCall, parseSuccessBranch, defaultValue);
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
    }
}