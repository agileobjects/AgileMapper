namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;
    using Extensions.Internal;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ToEnumConverter : IValueConverter
    {
        private readonly UserConfigurationSet _userConfigurations;

        public ToEnumConverter(UserConfigurationSet userConfigurations)
        {
            _userConfigurations = userConfigurations;
        }

        public bool CanConvert(Type nonNullableSourceType, Type nonNullableTargetType)
        {
            if (!nonNullableTargetType.IsEnum())
            {
                return false;
            }

            return nonNullableSourceType.IsNumeric() ||
                   ToStringConverter.HasNativeStringRepresentation(nonNullableSourceType);
        }

        public Expression GetConversion(Expression sourceValue, Type targetEnumType)
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

            if (nonNullableSourceType.IsEnum())
            {
                return GetEnumToEnumConversion(
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
                nonNullableTargetEnumType);
        }

        private static Expression GetFlagsEnumConversion(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableSourceType,
            Type nonNullableTargetEnumType)
        {
            var enumTypeName = GetVariableNameFor(nonNullableTargetEnumType);
            var underlyingEnumType = Enum.GetUnderlyingType(nonNullableTargetEnumType);

            var enumValueVariable = Expression.Variable(underlyingEnumType, enumTypeName + "Value");
            var underlyingTypeDefault = underlyingEnumType.ToDefaultExpression();
            var assignEnumValue = enumValueVariable.AssignTo(underlyingTypeDefault);

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
                sourceValue = ToStringConverter.GetConversion(sourceValue);
            }

            var sourceValuesVariable = GetEnumValuesVariable(enumTypeName, typeof(string));

            var splitSourceValueCall = Expression.Call(
                sourceValue,
                typeof(string).GetPublicInstanceMethod("Split", typeof(char[])),
                Expression.NewArrayInit(typeof(char), ','.ToConstantExpression()));

            var assignSourceValues = GetValuesEnumeratorAssignment(sourceValuesVariable, splitSourceValueCall);

            var ifNotMoveNextBreak = GetLoopExitCheck(sourceValuesVariable, out var loopBreakTarget);

            var localSourceValueVariable = Expression.Variable(typeof(string), enumTypeName);
            var enumeratorCurrent = Expression.Property(sourceValuesVariable, "Current");
            var stringTrimMethod = typeof(string).GetPublicInstanceMethod("Trim", parameterCount: 0);
            var currentTrimmed = Expression.Call(enumeratorCurrent, stringTrimMethod);
            var assignLocalVariable = localSourceValueVariable.AssignTo(currentTrimmed);

            var isNumericTest = GetIsNumericTest(localSourceValueVariable);

            var sourceNumericValueVariableName = enumTypeName + underlyingEnumType.Name + "Value";
            var sourceNumericValueVariable = Expression.Variable(underlyingEnumType, sourceNumericValueVariableName);
            var parsedString = GetStringParseCall(localSourceValueVariable, underlyingEnumType);
            var assignNumericVariable = sourceNumericValueVariable.AssignTo(parsedString);

            var numericValuePopulationLoop = GetNumericToFlagsEnumPopulationLoop(
                nonNullableTargetEnumType,
                enumTypeName,
                enumValueVariable,
                sourceNumericValueVariable,
                out var enumValuesVariable,
                out var assignEnumValues);

            var numericValuePopulationBlock = Expression.Block(
                new[] { enumValuesVariable },
                assignNumericVariable,
                assignEnumValues,
                numericValuePopulationLoop);

            var stringValueConversion = GetStringToEnumConversion(
                localSourceValueVariable,
                underlyingTypeDefault,
                nonNullableTargetEnumType);

            var assignParsedEnumValue = Expression.OrAssign(enumValueVariable, stringValueConversion);

            var assignValidValuesIfPossible = Expression.IfThenElse(
                isNumericTest,
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

        private static string GetVariableNameFor(Type enumType)
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

            var assignSourceVariable = sourceValueVariable.AssignTo(sourceValue);

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
            var assignLocalVariable = localEnumValueVariable.AssignTo(enumeratorCurrent);

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

        private static Expression GetIsNumericTest(Expression stringValue)
        {
            return Expression.Call(
                typeof(char).GetPublicStaticMethod("IsDigit", typeof(string), typeof(int)),
                stringValue,
                ToNumericConverter<int>.Zero);
        }

        public static Expression GetStringParseCall(Expression sourceValue, Type underlyingEnumType)
        {
            return Expression.Call(
                underlyingEnumType.GetPublicStaticMethod("Parse", typeof(string)),
                sourceValue);
        }

        private static Expression GetStringToEnumConversion(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableTargetEnumType)
        {
            if (fallbackValue.Type.GetNonNullableType() != nonNullableTargetEnumType)
            {
                var getConversionMethod = typeof(ToEnumConverter)
                    .GetNonPublicStaticMethods("GetStringToEnumValueConversion")
                    .First(m => m.IsGenericMethod)
                    .MakeGenericMethod(fallbackValue.Type);

                return (Expression)getConversionMethod.Invoke(
                    null,
                    new object[] { sourceValue, fallbackValue, nonNullableTargetEnumType });
            }

            var targetEnumValues = GetEnumValues(nonNullableTargetEnumType);

            return targetEnumValues.Reverse().Aggregate(
                fallbackValue,
                (valueSoFar, enumValue) => Expression.Condition(
                    sourceValue.GetCaseInsensitiveEquals(enumValue.Member.Name.ToConstantExpression()),
                    enumValue.GetConversionTo(fallbackValue.Type),
                    valueSoFar));
        }

        // ReSharper disable once UnusedMember.Local
        private static Expression GetStringToEnumValueConversion<TResult>(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableTargetEnumType)
        {
            var enumValues = Enum.GetValues(nonNullableTargetEnumType).Cast<object>();

            return enumValues.Reverse().Aggregate(
                fallbackValue,
                (valueSoFar, enumValue) => Expression.Condition(
                    sourceValue.GetCaseInsensitiveEquals(enumValue.ToString().ToConstantExpression()),
                    ((TResult)enumValue).ToConstantExpression(fallbackValue.Type),
                    valueSoFar));
        }

        private Expression GetEnumToEnumConversion(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableSourceEnumType,
            Type nonNullableTargetEnumType)
        {
            var sourceEnumValues = GetEnumValues(nonNullableSourceEnumType);
            var targetEnumValues = GetEnumValues(nonNullableTargetEnumType);

            var pairedMemberNames = _userConfigurations
                .GetEnumPairingsFor(nonNullableSourceEnumType, nonNullableTargetEnumType)
                .ToDictionary(pair => pair.PairingEnumMemberName, pair => pair.PairedEnumMemberName);

            var enumPairs = sourceEnumValues.ToDictionary(
                sv => sv,
                sv =>
                {
                    if (pairedMemberNames.TryGetValue(sv.Member.Name, out var pairedMemberName))
                    {
                        return new
                        {
                            Value = (Expression)targetEnumValues.First(tv => tv.Member.Name == pairedMemberName),
                            IsCustom = true
                        };
                    }

                    return new
                    {
                        Value = targetEnumValues
                            .FirstOrDefault(tv => tv.Member.Name.EqualsIgnoreCase(sv.Member.Name)) ??
                             fallbackValue,
                        IsCustom = false
                    };
                });

            var enumPairsConversion = sourceEnumValues
                .Project(sv => new
                {
                    SourceValue = sv,
                    PairedValue = enumPairs[sv]
                })
                .OrderByDescending(d => d.PairedValue.IsCustom)
                .Reverse()
                .Aggregate(
                    fallbackValue,
                    (valueSoFar, enumData) =>
                    {
                        if (enumData.PairedValue.Value == fallbackValue)
                        {
                            return valueSoFar;
                        }

                        return Expression.Condition(
                            Expression.Equal(sourceValue, enumData.SourceValue.GetConversionTo(sourceValue.Type)),
                            enumData.PairedValue.Value.GetConversionTo(fallbackValue.Type),
                            valueSoFar);
                    });

            return enumPairsConversion;
        }

        private static IList<MemberExpression> GetEnumValues(Type enumType)
            => enumType.GetPublicStaticFields().Project(f => Expression.Field(null, f)).ToList();

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

        private static Expression GetIsValidEnumValueCheck(
            Type enumType,
            Expression value,
            Expression validEnumValues = null)
        {
            if (validEnumValues == null)
            {
                validEnumValues = GetEnumValuesConstant(enumType, value.Type);
            }

            var containsMethod = validEnumValues.Type.GetPublicInstanceMethod("Contains");
            var containsCall = Expression.Call(validEnumValues, containsMethod, value);

            return containsCall;
        }

        private static Expression GetStringValueConversion(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableTargetEnumType)
        {
            if (sourceValue.Type != typeof(string))
            {
                sourceValue = ToStringConverter.GetConversion(sourceValue);
            }

            var underlyingEnumType = Enum.GetUnderlyingType(nonNullableTargetEnumType);

            var isNumericTest = GetIsNumericTest(sourceValue);

            var numericConversion = GetNumericStringToEnumConversion(
                sourceValue,
                fallbackValue,
                nonNullableTargetEnumType,
                underlyingEnumType);

            var nameMatchingConversion = GetStringToEnumConversion(
                sourceValue,
                fallbackValue,
                nonNullableTargetEnumType);

            var numericOrNameConversion = Expression.Condition(
                isNumericTest,
                numericConversion,
                nameMatchingConversion);

            var valueIsNullOrEmpty = Expression.Call(
#if NET35
                typeof(StringExtensions)
#else
                typeof(string)
#endif
                    .GetPublicStaticMethod("IsNullOrWhiteSpace"),
                sourceValue);

            var convertedValueOrDefault = Expression.Condition(
                valueIsNullOrEmpty,
                fallbackValue,
                numericOrNameConversion);

            return convertedValueOrDefault;
        }

        private static Expression GetNumericStringToEnumConversion(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableTargetEnumType,
            Type underlyingEnumType)
        {
            var validEnumValues = Enum
                .GetValues(nonNullableTargetEnumType)
                .Cast<object>()
                .Project(v => Convert.ChangeType(v, underlyingEnumType).ToString())
                .ToArray()
                .ToConstantExpression(typeof(ICollection<string>));

            var parsedString = GetStringParseCall(sourceValue, underlyingEnumType);

            var validValueOrFallback = Expression.Condition(
                GetIsValidEnumValueCheck(nonNullableTargetEnumType, sourceValue, validEnumValues),
                parsedString.GetConversionTo(fallbackValue.Type),
                fallbackValue);

            return validValueOrFallback;
        }
    }
}