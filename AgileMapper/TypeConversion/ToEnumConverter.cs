namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Collections;
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
            var nonNullableSourceType = sourceValue.Type.GetNonNullableType();
            var nonNullableTargetEnumType = targetEnumType.GetNonNullableType();

            if (nonNullableTargetEnumType.HasAttribute<FlagsAttribute>())
            {
                return GetFlagsEnumConversion(
                    sourceValue,
                    fallbackValue,
                    nonNullableSourceType,
                    nonNullableTargetEnumType);
            }


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
            Expression convertedNumericValue = Expression.Convert(sourceValue, nonNullableTargetEnumType);

            if (nonNullableTargetEnumType != fallbackValue.Type)
            {
                convertedNumericValue = convertedNumericValue.GetConversionTo(fallbackValue.Type);
            }

            var definedValueOrFallback = Expression.Condition(
                GetEnumIsDefinedCall(nonNullableTargetEnumType, sourceValue),
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

        private static Expression GetEnumIsDefinedCall(Type enumType, Expression value)
        {
            var convertedValue = value
                .GetConversionTo(Enum.GetUnderlyingType(enumType))
                .GetConversionTo(typeof(object));

            return Expression.Call(
                typeof(Enum).GetPublicStaticMethod("IsDefined"),
                enumType.ToConstantExpression(),
                convertedValue);
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

            var definedValueOrDefault = Expression.Condition(
                GetEnumIsDefinedCall(valueVariable.Type, valueVariable),
                successfulParseReturnValue,
                defaultValue);

            return definedValueOrDefault;
        }

        private Expression GetFlagsEnumConversion(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableSourceType,
            Type nonNullableTargetEnumType)
        {
            if (nonNullableSourceType.IsNumeric())
            {
                return GetNumericToFlagsEnumConversion(
                    sourceValue,
                    fallbackValue,
                    nonNullableSourceType,
                    nonNullableTargetEnumType);
            }

            throw new NotImplementedException();
        }

        private static Expression GetNumericToFlagsEnumConversion(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableSourceType,
            Type nonNullableTargetEnumType)
        {
            var enumTypeName = nonNullableTargetEnumType.GetVariableNameInCamelCase();
            var underlyingEnumType = Enum.GetUnderlyingType(nonNullableTargetEnumType);

            var enumValueVariable = Expression.Variable(underlyingEnumType, enumTypeName + "Value");
            var assignEnumValue = Expression.Assign(enumValueVariable, underlyingEnumType.ToDefaultExpression());

            var enumValuesVariable = Expression.Variable(typeof(IEnumerator), enumTypeName + "Values");

            var enumGetValuesCall = Expression.Call(
                typeof(Enum).GetPublicStaticMethod("GetValues"),
                nonNullableTargetEnumType.ToConstantExpression());

            var getValuesEnumeratorCall = Expression.Call(
                enumGetValuesCall,
                enumGetValuesCall.Type.GetPublicInstanceMethod("GetEnumerator"));

            var assignEnumValues = Expression.Assign(enumValuesVariable, getValuesEnumeratorCall);

            var enumeratorMoveNext = Expression.Call(
                enumValuesVariable,
                enumValuesVariable.Type.GetPublicInstanceMethod("MoveNext"));

            var loopBreakTarget = Expression.Label();

            var ifNotMoveNextBreak = Expression.IfThen(
                Expression.Not(enumeratorMoveNext),
                Expression.Break(loopBreakTarget));

            var localEnumValueVariable = Expression.Variable(underlyingEnumType, enumTypeName);
            var enumeratorCurrent = Expression.Property(enumValuesVariable, "Current");
            var currentAsEnumType = Expression.Convert(enumeratorCurrent, underlyingEnumType);
            var assignLocalVariable = Expression.Assign(localEnumValueVariable, currentAsEnumType);

            var sourceValueVariable = Expression.Variable(underlyingEnumType, enumTypeName + "Source");

            if (sourceValue.Type != underlyingEnumType)
            {
                sourceValue = Expression.Convert(sourceValue, underlyingEnumType);
            }

            var assignSourceVariable = Expression.Assign(sourceValueVariable, sourceValue);

            var localVariableAndSourceValue = Expression.And(localEnumValueVariable, sourceValueVariable);
            var andResultEqualsEnumValue = Expression.Equal(localVariableAndSourceValue, localEnumValueVariable);

            var ifAndResultMatchesAssign = Expression.IfThen(
                andResultEqualsEnumValue,
                Expression.OrAssign(enumValueVariable, localEnumValueVariable));

            var loopBody = Expression.Block(
                new[] { localEnumValueVariable },
                ifNotMoveNextBreak,
                assignLocalVariable,
                ifAndResultMatchesAssign);

            var populationBlock = Expression.Block(
                new[] { sourceValueVariable, enumValueVariable, enumValuesVariable },
                assignSourceVariable,
                assignEnumValue,
                assignEnumValues,
                Expression.Loop(loopBody, loopBreakTarget),
                enumValueVariable.GetConversionTo(fallbackValue.Type));

            return populationBlock;
        }
    }
}