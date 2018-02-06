namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Collections;
    using System.Linq;
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
                .GetConversionToObject();

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

            var tryParseCall = GetTryParseCall(sourceValue, nonNullableTargetEnumType, out var valueVariable);
            var parseSuccessBranch = GetParseSuccessBranch(sourceIsAnEnum, valueVariable, fallbackValue);

            var parsedValueOrDefault = Expression.Condition(tryParseCall, parseSuccessBranch, fallbackValue);
            var tryParseBlock = Expression.Block(new[] { valueVariable }, parsedValueOrDefault);

            return tryParseBlock;
        }

        private static Expression GetTryParseCall(
            Expression sourceValue,
            Type nonNullableTargetEnumType,
            out ParameterExpression valueVariable)
        {
            var tryParseMethod = typeof(Enum)
                .GetPublicStaticMethods("TryParse")
                .Select(m => new
                {
                    Method = m,
                    Parameters = m.GetParameters()
                })
                .First(m => m.Parameters.Length == 3 && m.Parameters[1].ParameterType == typeof(bool))
                .Method
                .MakeGenericMethod(nonNullableTargetEnumType);

            valueVariable = Expression.Variable(
                nonNullableTargetEnumType,
                nonNullableTargetEnumType.GetShortVariableName());

            return Expression.Call(
                tryParseMethod,
                sourceValue,
                true.ToConstantExpression(), // <- IgnoreCase
                valueVariable);
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
            var enumTypeName = nonNullableTargetEnumType.GetVariableNameInCamelCase();
            var underlyingEnumType = Enum.GetUnderlyingType(nonNullableTargetEnumType);

            var enumValueVariable = Expression.Variable(underlyingEnumType, enumTypeName + "Value");
            var assignEnumValue = Expression.Assign(enumValueVariable, underlyingEnumType.ToDefaultExpression());

            if (nonNullableSourceType.IsNumeric())
            {
                return GetNumericToFlagsEnumConversion(
                    sourceValue,
                    fallbackValue,
                    nonNullableTargetEnumType,
                    enumTypeName,
                    enumValueVariable,
                    assignEnumValue);
            }

            if (sourceValue.Type != typeof(string))
            {
                sourceValue = _toStringConverter.GetConversion(sourceValue);
            }

            var sourceValuesVariable = GetEnumValuesVariable(enumTypeName);

            var splitSourceValueCall = Expression.Call(
                sourceValue,
                typeof(string).GetPublicInstanceMethod("Split", parameterCount: 1),
                Expression.NewArrayInit(typeof(char), ','.ToConstantExpression()));

            var assignSourceValues = GetValuesEnumeratorAssignment(sourceValuesVariable, splitSourceValueCall);

            var ifNotMoveNextBreak = GetLoopExitCheck(sourceValuesVariable, out var loopBreakTarget);

            var localSourceValueVariable = Expression.Variable(typeof(string), enumTypeName);
            var enumeratorCurrent = Expression.Property(sourceValuesVariable, "Current");
            var currentToString = Expression.Call(enumeratorCurrent, typeof(object).GetPublicInstanceMethod("ToString"));
            var stringTrimMethod = typeof(string).GetPublicInstanceMethod("Trim", parameterCount: 0);
            var currentTrimmed = Expression.Call(currentToString, stringTrimMethod);
            var assignLocalVariable = Expression.Assign(localSourceValueVariable, currentTrimmed);

            var sourceNumericValueVariableName = enumTypeName + underlyingEnumType.Name + "Value";
            var sourceNumericValueVariable = Expression.Parameter(underlyingEnumType, sourceNumericValueVariableName);

            var numericTryParseCall = Expression.Call(
                underlyingEnumType.GetPublicStaticMethod("TryParse", parameterCount: 2),
                localSourceValueVariable,
                sourceNumericValueVariable);

            var numericValuePopulationLoop = GetNumericToFlagsEnumPopulationLoop(
                nonNullableTargetEnumType,
                enumTypeName,
                enumValueVariable,
                sourceNumericValueVariable,
                out var enumValuesVariable,
                out var assignEnumValues);

            var numericValuePopulationBlock = Expression.Block(
                new[] { enumValuesVariable },
                assignEnumValues,
                numericValuePopulationLoop);

            var stringHasValidValueCheck = GetTryParseCall(
                localSourceValueVariable,
                nonNullableTargetEnumType,
                out var sourceEnumValueVariable);

            var convertedEnumValue = sourceEnumValueVariable.GetConversionTo(enumValueVariable.Type);
            var assignParsedEnumValue = Expression.OrAssign(enumValueVariable, convertedEnumValue);

            var assignValidValuesIfPossible = Expression.IfThenElse(
                numericTryParseCall,
                numericValuePopulationBlock,
                Expression.IfThen(stringHasValidValueCheck, assignParsedEnumValue));

            var loopBody = Expression.Block(
                new[] { localSourceValueVariable, sourceNumericValueVariable, sourceEnumValueVariable },
                ifNotMoveNextBreak,
                assignLocalVariable,
                assignValidValuesIfPossible);

            var populationBlock = Expression.Block(
                new[] { sourceValuesVariable, enumValueVariable },
                assignEnumValue,
                assignSourceValues,
                Expression.Loop(loopBody, loopBreakTarget),
                enumValueVariable.GetConversionTo(fallbackValue.Type));

            return populationBlock;
        }

        private static Expression GetNumericToFlagsEnumConversion(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableTargetEnumType,
            string enumTypeName,
            ParameterExpression enumValueVariable,
            Expression assignEnumValue)
        {
            var underlyingEnumType = enumValueVariable.Type;

            var sourceValueVariable = Expression.Variable(underlyingEnumType, enumTypeName + "Source");

            if (sourceValue.Type != underlyingEnumType)
            {
                sourceValue = Expression.Convert(sourceValue, underlyingEnumType);
            }

            var assignSourceVariable = Expression.Assign(sourceValueVariable, sourceValue);

            var populationLoop = GetNumericToFlagsEnumPopulationLoop(
                nonNullableTargetEnumType,
                enumTypeName,
                enumValueVariable,
                sourceValueVariable,
                out var enumValuesVariable,
                out var assignEnumValues);

            var populationBlock = Expression.Block(
                new[] { sourceValueVariable, enumValueVariable, enumValuesVariable },
                assignSourceVariable,
                assignEnumValue,
                assignEnumValues,
                populationLoop,
                enumValueVariable.GetConversionTo(fallbackValue.Type));

            return populationBlock;
        }

        private static Expression GetNumericToFlagsEnumPopulationLoop(
            Type nonNullableTargetEnumType,
            string enumTypeName,
            Expression enumValueVariable,
            Expression sourceValueVariable,
            out ParameterExpression enumValuesVariable,
            out Expression assignEnumValues)
        {
            var underlyingEnumType = enumValueVariable.Type;
            enumValuesVariable = GetEnumValuesVariable(enumTypeName);

            var enumGetValuesCall = Expression.Call(
                typeof(Enum).GetPublicStaticMethod("GetValues"),
                nonNullableTargetEnumType.ToConstantExpression());

            assignEnumValues = GetValuesEnumeratorAssignment(enumValuesVariable, enumGetValuesCall);

            var ifNotMoveNextBreak = GetLoopExitCheck(enumValuesVariable, out var loopBreakTarget);

            var localEnumValueVariable = Expression.Variable(underlyingEnumType, enumTypeName);
            var enumeratorCurrent = Expression.Property(enumValuesVariable, "Current");
            var currentAsEnumType = Expression.Convert(enumeratorCurrent, underlyingEnumType);
            var assignLocalVariable = Expression.Assign(localEnumValueVariable, currentAsEnumType);

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

            return Expression.Loop(loopBody, loopBreakTarget);
        }

        private static ParameterExpression GetEnumValuesVariable(string enumTypeName)
            => Expression.Variable(typeof(IEnumerator), enumTypeName + "Values");

        private static Expression GetValuesEnumeratorAssignment(
            Expression enumValuesVariable,
            Expression enumeratedValues)
        {
            var getValuesEnumeratorCall = Expression.Call(
                enumeratedValues,
                enumeratedValues.Type.GetPublicInstanceMethod("GetEnumerator"));

            return enumValuesVariable.AssignTo(getValuesEnumeratorCall);
        }

        private static Expression GetLoopExitCheck(
            Expression valuesEnumerator,
            out LabelTarget loopBreakTarget)
        {
            var enumeratorMoveNext = Expression.Call(
                valuesEnumerator,
                valuesEnumerator.Type.GetPublicInstanceMethod("MoveNext"));

            loopBreakTarget = Expression.Label();

            return Expression.IfThen(Expression.Not(enumeratorMoveNext), Expression.Break(loopBreakTarget));
        }
    }
}