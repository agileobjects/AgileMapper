namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
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

            return GetStringValueConversion(
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
            var underlyingEnumType = Enum.GetUnderlyingType(nonNullableTargetEnumType);
            var convertedNumericValue = sourceValue.GetConversionTo(underlyingEnumType);

            var validValueOrFallback = Expression.Condition(
                GetIsValidEnumValueCheck(nonNullableTargetEnumType, convertedNumericValue),
                sourceValue.GetConversionTo(nonNullableTargetEnumType).GetConversionTo(fallbackValue.Type),
                fallbackValue);

            if (sourceValue.Type == nonNullableSourceType)
            {
                return validValueOrFallback;
            }

            var nonNullValidValueOrFallback = Expression.Condition(
                sourceValue.GetIsNotDefaultComparison(),
                validValueOrFallback,
                fallbackValue);

            return nonNullValidValueOrFallback;
        }

        private static Expression GetIsValidEnumValueCheck(Type enumType, Expression value)
        {
            var validEnumValues = GetEnumValuesConstant(enumType, value.Type);
            var containsMethod = validEnumValues.Type.GetPublicInstanceMethod("Contains");
            var containsCall = Expression.Call(validEnumValues, containsMethod, value);

            return containsCall;
        }

        private Expression GetStringValueConversion(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableSourceType,
            Type nonNullableTargetEnumType)
        {
            if (sourceValue.Type != typeof(string))
            {
                sourceValue = _toStringConverter.GetConversion(sourceValue);
            }

            var underlyingEnumType = Enum.GetUnderlyingType(nonNullableTargetEnumType);

            var tryParseCall = GetNumericTryParseCall(
                GetEnumTypeName(nonNullableTargetEnumType),
                underlyingEnumType,
                sourceValue,
                out var parseResultVariable);

            var numericConversion = GetNumericToEnumConversion(
                parseResultVariable,
                fallbackValue,
                nonNullableSourceType,
                nonNullableTargetEnumType);

            var nameMatchingConversion = GetStringToEnumConversion(
                sourceValue,
                fallbackValue,
                nonNullableTargetEnumType,
                nonNullableTargetEnumType);

            var numericOrNameConversion = Expression.Condition(
                tryParseCall,
                numericConversion,
                nameMatchingConversion);

            return Expression.Block(new[] { parseResultVariable }, numericOrNameConversion);
        }

        private static Expression GetNumericTryParseCall(
            string enumTypeName,
            Type underlyingEnumType,
            Expression stringValue,
            out ParameterExpression numericValueVariable)
        {
            var tryParseMethod = underlyingEnumType
                .GetPublicStaticMethod("TryParse", parameterCount: 2);

            var sourceNumericValueVariableName = enumTypeName + underlyingEnumType.Name + "Value";
            numericValueVariable = Expression.Parameter(underlyingEnumType, sourceNumericValueVariableName);

            var tryParseCall = Expression.Call(tryParseMethod, stringValue, numericValueVariable);

            return tryParseCall;
        }

        private static Expression GetStringToEnumConversion(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableTargetEnumType,
            Type conversionResultType)
        {
            var getConversionMethod = typeof(ToEnumConverter)
                .GetNonPublicStaticMethods("GetStringToEnumConversion")
                .First(m => m.IsGenericMethod)
                .MakeGenericMethod(nonNullableTargetEnumType, conversionResultType);

            return (Expression)getConversionMethod.Invoke(
                null,
                new object[] { sourceValue, fallbackValue });
        }

        // ReSharper disable once UnusedMember.Local
        private static Expression GetStringToEnumConversion<TEnum, TResult>(
            Expression sourceValue,
            Expression fallbackValue)
        {
            var enumValues = Enum.GetValues(typeof(TEnum)).Cast<TResult>().Reverse();

            return enumValues.Aggregate(
                fallbackValue,
                (valueSoFar, enumValue) => Expression.Condition(
                    GetEnumNameCheck<TEnum, TResult>(sourceValue, enumValue),
                    enumValue.ToConstantExpression(fallbackValue.Type),
                    valueSoFar));
        }

        private static Expression GetEnumNameCheck<TEnum, TValue>(Expression sourceValue, TValue enumValue)
        {
            var comparisonValue = (typeof(TValue) == typeof(TEnum))
                ? enumValue.ToString()
                : ((TEnum)((object)enumValue)).ToString();

            return sourceValue.GetCaseInsensitiveEquals(comparisonValue.ToConstantExpression());
        }

        private Expression GetFlagsEnumConversion(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableSourceType,
            Type nonNullableTargetEnumType)
        {
            var enumTypeName = GetEnumTypeName(nonNullableTargetEnumType);
            var underlyingEnumType = Enum.GetUnderlyingType(nonNullableTargetEnumType);

            var enumValueVariable = Expression.Variable(underlyingEnumType, enumTypeName + "Value");
            var underlyingTypeDefault = underlyingEnumType.ToDefaultExpression();
            var assignEnumValue = Expression.Assign(enumValueVariable, underlyingTypeDefault);

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

            var sourceValuesVariable = GetEnumValuesVariable(enumTypeName, typeof(string));

            var splitSourceValueCall = Expression.Call(
                sourceValue,
                typeof(string).GetPublicInstanceMethod("Split", parameterCount: 1),
                Expression.NewArrayInit(typeof(char), ','.ToConstantExpression()));

            var assignSourceValues = GetValuesEnumeratorAssignment(sourceValuesVariable, splitSourceValueCall);

            var ifNotMoveNextBreak = GetLoopExitCheck(sourceValuesVariable, out var loopBreakTarget);

            var localSourceValueVariable = Expression.Variable(typeof(string), enumTypeName);
            var enumeratorCurrent = Expression.Property(sourceValuesVariable, "Current");
            var stringTrimMethod = typeof(string).GetPublicInstanceMethod("Trim", parameterCount: 0);
            var currentTrimmed = Expression.Call(enumeratorCurrent, stringTrimMethod);
            var assignLocalVariable = Expression.Assign(localSourceValueVariable, currentTrimmed);

            var numericTryParseCall = GetNumericTryParseCall(
                enumTypeName,
                underlyingEnumType,
                localSourceValueVariable,
                out var sourceNumericValueVariable);

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

            var stringValueConversion = GetStringToEnumConversion(
                localSourceValueVariable,
                underlyingTypeDefault,
                nonNullableTargetEnumType,
                underlyingEnumType);

            var assignParsedEnumValue = Expression.OrAssign(enumValueVariable, stringValueConversion);

            var assignValidValuesIfPossible = Expression.IfThenElse(
                numericTryParseCall,
                numericValuePopulationBlock,
                assignParsedEnumValue);

            var loopBody = Expression.Block(
                new[] { localSourceValueVariable, sourceNumericValueVariable },
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

        private static string GetEnumTypeName(Type enumType)
            => enumType.GetVariableNameInCamelCase();

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
            enumValuesVariable = GetEnumValuesVariable(enumTypeName, underlyingEnumType);

            var enumValues = GetEnumValuesConstant(nonNullableTargetEnumType, underlyingEnumType);

            assignEnumValues = GetValuesEnumeratorAssignment(enumValuesVariable, enumValues);

            var ifNotMoveNextBreak = GetLoopExitCheck(enumValuesVariable, out var loopBreakTarget);

            var localEnumValueVariable = Expression.Variable(underlyingEnumType, enumTypeName);
            var enumeratorCurrent = Expression.Property(enumValuesVariable, "Current");
            var assignLocalVariable = Expression.Assign(localEnumValueVariable, enumeratorCurrent);

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

        private static ParameterExpression GetEnumValuesVariable(string enumTypeName, Type elementType)
            => Expression.Variable(typeof(IEnumerator<>).MakeGenericType(elementType), enumTypeName + "Values");

        private static Expression GetEnumValuesConstant(Type enumType, Type underlyingType)
        {
            if (underlyingType == typeof(int))
            {
                return GetEnumValuesConstant<int>(enumType);
            }

            return (Expression)typeof(ToEnumConverter)
                .GetNonPublicStaticMethod("GetEnumValuesConstant")
                .MakeGenericMethod(underlyingType)
                .Invoke(null, new object[] { enumType });
        }

        private static Expression GetEnumValuesConstant<TUnderlyingType>(Type enumType)
        {
            return Enum
                .GetValues(enumType)
                .Cast<TUnderlyingType>()
                .ToArray()
                .ToConstantExpression<ICollection<TUnderlyingType>>();
        }

        private static Expression GetValuesEnumeratorAssignment(
            Expression enumValuesVariable,
            Expression enumeratedValues)
        {
            var enumerableType = enumeratedValues.Type.IsClosedTypeOf(typeof(IEnumerable<>))
                ? enumeratedValues.Type
                : typeof(IEnumerable<>).MakeGenericType(enumeratedValues.Type.GetEnumerableElementType());

            var getValuesEnumeratorCall = Expression.Call(
                enumeratedValues,
                enumerableType.GetPublicInstanceMethod("GetEnumerator"));

            return enumValuesVariable.AssignTo(getValuesEnumeratorCall);
        }

        private static Expression GetLoopExitCheck(
            Expression valuesEnumerator,
            out LabelTarget loopBreakTarget)
        {
            var enumeratorMoveNext = Expression.Call(
                valuesEnumerator,
                typeof(IEnumerator).GetPublicInstanceMethod("MoveNext"));

            loopBreakTarget = Expression.Label();

            return Expression.IfThen(Expression.Not(enumeratorMoveNext), Expression.Break(loopBreakTarget));
        }
    }
}