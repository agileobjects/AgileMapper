namespace AgileObjects.AgileMapper.TypeConversion
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Configuration;
    using Extensions;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
    using static System.Convert;
#if NET35
    using static Microsoft.Scripting.Ast.Expression;
#else
    using static System.Linq.Expressions.Expression;
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

        public Expression GetConversion(
            Expression sourceValue,
            Type targetEnumType,
            bool useSingleStatement)
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
                    nonNullableTargetEnumType,
                    useSingleStatement);
            }

            return GetStringValueConversion(
                sourceValue,
                fallbackValue,
                nonNullableTargetEnumType,
                useSingleStatement);
        }

        private static Expression GetFlagsEnumConversion(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableSourceType,
            Type nonNullableTargetEnumType)
        {
            var enumTypeName = GetVariableNameFor(nonNullableTargetEnumType);
            var underlyingEnumType = Enum.GetUnderlyingType(nonNullableTargetEnumType);

            var enumValueVariable = Variable(underlyingEnumType, enumTypeName + "Value");
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

            var splitSourceValueCall = Call(
                sourceValue,
                typeof(string).GetPublicInstanceMethod("Split", typeof(char[])),
                NewArrayInit(typeof(char), ','.ToConstantExpression()));

            var assignSourceValues = GetValuesEnumeratorAssignment(sourceValuesVariable, splitSourceValueCall);

            var ifNotMoveNextBreak = GetLoopExitCheck(sourceValuesVariable, out var loopBreakTarget);

            var localSourceValueVariable = Variable(typeof(string), enumTypeName);
            var enumeratorCurrent = Property(sourceValuesVariable, "Current");
            var stringTrimMethod = typeof(string).GetPublicInstanceMethod("Trim", parameterCount: 0);
            var currentTrimmed = Call(enumeratorCurrent, stringTrimMethod);
            var assignLocalVariable = localSourceValueVariable.AssignTo(currentTrimmed);

            var isNumericTest = GetIsNumericTest(localSourceValueVariable);

            var sourceNumericValueVariableName = enumTypeName + underlyingEnumType.Name + "Value";
            var sourceNumericValueVariable = Variable(underlyingEnumType, sourceNumericValueVariableName);
            var parsedString = GetStringParseCall(localSourceValueVariable, underlyingEnumType);
            var assignNumericVariable = sourceNumericValueVariable.AssignTo(parsedString);

            var numericValuePopulationLoop = GetNumericToFlagsEnumPopulationLoop(
                nonNullableTargetEnumType,
                enumTypeName,
                enumValueVariable,
                sourceNumericValueVariable,
                out var enumValuesVariable,
                out var assignEnumValues);

            var numericValuePopulationBlock = Block(
                new[] { enumValuesVariable },
                assignNumericVariable,
                assignEnumValues,
                numericValuePopulationLoop);

            var stringValueConversion = GetStringToEnumConversion(
                localSourceValueVariable,
                underlyingTypeDefault,
                nonNullableTargetEnumType);

            var assignParsedEnumValue = OrAssign(enumValueVariable, stringValueConversion);

            var assignValidValuesIfPossible = IfThenElse(
                isNumericTest,
                numericValuePopulationBlock,
                assignParsedEnumValue);

            var loopBody = Block(
                new[] { localSourceValueVariable, sourceNumericValueVariable },
                ifNotMoveNextBreak,
                assignLocalVariable,
                assignValidValuesIfPossible);

            var populationBlock = Block(
                new[] { sourceValuesVariable, enumValueVariable },
                assignEnumValue,
                assignSourceValues,
                Loop(loopBody, loopBreakTarget),
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

            var sourceValueVariable = Variable(underlyingEnumType, enumTypeName + "Source");

            if (sourceValue.Type != underlyingEnumType)
            {
                sourceValue = Convert(sourceValue, underlyingEnumType);
            }

            var assignSourceVariable = sourceValueVariable.AssignTo(sourceValue);

            var populationLoop = GetNumericToFlagsEnumPopulationLoop(
                nonNullableTargetEnumType,
                enumTypeName,
                enumValueVariable,
                sourceValueVariable,
                out var enumValuesVariable,
                out var assignEnumValues);

            var populationBlock = Block(
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

            var localEnumValueVariable = Variable(underlyingEnumType, enumTypeName);
            var enumeratorCurrent = Property(enumValuesVariable, "Current");
            var assignLocalVariable = localEnumValueVariable.AssignTo(enumeratorCurrent);

            var localVariableAndSourceValue = And(localEnumValueVariable, sourceValueVariable);
            var andResultEqualsEnumValue = Equal(localVariableAndSourceValue, localEnumValueVariable);

            var ifAndResultMatchesAssign = IfThen(
                andResultEqualsEnumValue,
                OrAssign(enumValueVariable, localEnumValueVariable));

            var loopBody = Block(
                new[] { localEnumValueVariable },
                ifNotMoveNextBreak,
                assignLocalVariable,
                ifAndResultMatchesAssign);

            return Loop(loopBody, loopBreakTarget);
        }

        private static ParameterExpression GetEnumValuesVariable(string enumTypeName, Type elementType)
            => Variable(typeof(IEnumerator<>).MakeGenericType(elementType), enumTypeName + "Values");

        private static Expression GetEnumValuesConstant(Type enumType, Type underlyingType)
        {
            if (underlyingType == typeof(int))
            {
                return GetEnumValuesConstantWithUnderlyingType<int>(enumType);
            }

            return (Expression)typeof(ToEnumConverter)
                .GetNonPublicStaticMethod(nameof(GetEnumValuesConstantWithUnderlyingType))
                .MakeGenericMethod(underlyingType)
                .Invoke(null, new object[] { enumType });
        }

        private static Expression GetEnumValuesConstantWithUnderlyingType<TUnderlyingType>(Type enumType)
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

            var getValuesEnumeratorCall = Call(
                enumeratedValues,
                enumerableType.GetPublicInstanceMethod("GetEnumerator"));

            return enumValuesVariable.AssignTo(getValuesEnumeratorCall);
        }

        private static Expression GetLoopExitCheck(
            Expression valuesEnumerator,
            out LabelTarget loopBreakTarget)
        {
            var enumeratorMoveNext = Call(
                valuesEnumerator,
                typeof(IEnumerator).GetPublicInstanceMethod("MoveNext"));

            loopBreakTarget = Label();

            return IfThen(Not(enumeratorMoveNext), Break(loopBreakTarget));
        }

        private static Expression GetIsNumericTest(Expression stringValue)
        {
            return Call(
                typeof(char).GetPublicStaticMethod("IsDigit", typeof(string), typeof(int)),
                stringValue,
                ToNumericConverter<int>.Zero);
        }

        public static Expression GetStringParseCall(Expression sourceValue, Type underlyingEnumType)
        {
            return Call(
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
                (valueSoFar, enumValue) => Condition(
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
                (valueSoFar, enumValue) => Condition(
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
                            Value = (Expression)targetEnumValues.First(pairedMemberName, (pmn, tv) => tv.Member.Name == pmn),
                            IsCustom = true
                        };
                    }

                    return new
                    {
                        Value = targetEnumValues
                            .FirstOrDefault(sv.Member.Name, (name, tv) => tv.Member.Name.EqualsIgnoreCase(name)) ??
                             fallbackValue,
                        IsCustom = false
                    };
                });

            var enumPairsConversion = sourceEnumValues
                .Project(enumPairs, (eps, sv) => new
                {
                    SourceValue = sv,
                    PairedValue = eps[sv]
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

                        return Condition(
                            Equal(sourceValue, enumData.SourceValue.GetConversionTo(sourceValue.Type)),
                            enumData.PairedValue.Value.GetConversionTo(fallbackValue.Type),
                            valueSoFar);
                    });

            return enumPairsConversion;
        }

        private static IList<MemberExpression> GetEnumValues(Type enumType)
            => enumType.GetPublicStaticFields().Project(f => Field(null, f)).ToList();

        private static Expression GetNumericToEnumConversion(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableSourceType,
            Type nonNullableTargetEnumType,
            bool useSingleStatement)
        {
            if (!useSingleStatement)
            {
                return GetEnumMappingSwitchBlock(
                    sourceValue,
                    fallbackValue,
                    nonNullableTargetEnumType,
                   (enumValue, enumNumericType) => new[] { ChangeType(enumValue, enumNumericType) },
                   (switchValue, enumNumericType) => switchValue.GetConversionTo(enumNumericType));
            }

            var targetEnumType = fallbackValue.Type;
            var underlyingEnumType = Enum.GetUnderlyingType(nonNullableTargetEnumType);
            var convertedNumericValue = sourceValue.GetConversionTo(underlyingEnumType);
            var validEnumValues = GetEnumValuesConstant(targetEnumType, underlyingEnumType);

            var validValueOrFallback = Condition(
                GetIsValidEnumValueCheck(convertedNumericValue, validEnumValues),
                sourceValue.GetConversionTo(nonNullableTargetEnumType).GetConversionTo(targetEnumType),
                fallbackValue);

            if (sourceValue.Type == nonNullableSourceType)
            {
                return validValueOrFallback;
            }

            var nonNullValidValueOrFallback = Condition(
                sourceValue.GetIsNotDefaultComparison(),
                validValueOrFallback,
                fallbackValue);

            return nonNullValidValueOrFallback;
        }

        private static Expression GetEnumMappingSwitchBlock(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableTargetEnumType,
            Func<object, Type, object[]> caseTestValuesFactory,
            Func<Expression, Type, Expression> switchValueFactory = null)
        {
            var targetEnumType = fallbackValue.Type;
            var enumNumericType = Enum.GetUnderlyingType(nonNullableTargetEnumType);

            var returnTarget = Label(targetEnumType, "Return");

            var enumSwitchCases = nonNullableTargetEnumType.GetEnumValuesArray(v =>
            {
                var enumValue = ChangeType(v, nonNullableTargetEnumType);

                var caseTestValues = caseTestValuesFactory
                    .Invoke(v, enumNumericType)
                    .ProjectToArray<object, Expression>(Constant);

                return SwitchCase(
                    Return(returnTarget, Constant(enumValue).GetConversionTo(targetEnumType)),
                    caseTestValues);
            });

            var mappingExpressions = new List<Expression>();

            Expression switchValue;

            if (sourceValue.Type.CanBeNull())
            {
                mappingExpressions.Add(IfThen(
                    sourceValue.GetIsDefaultComparison(),
                    Return(returnTarget, fallbackValue)));

                switchValue = sourceValue.Type.IsNullableType()
                    ? sourceValue.GetNullableValueAccess()
                    : sourceValue;
            }
            else
            {
                switchValue = sourceValue;
            }

            if (switchValueFactory != null)
            {
                switchValue = switchValueFactory.Invoke(switchValue, enumNumericType);
            }

            mappingExpressions.Add(Switch(switchValue, enumSwitchCases));
            mappingExpressions.Add(Label(returnTarget, fallbackValue));

            return Block(mappingExpressions);
        }

        private static Expression GetIsValidEnumValueCheck(Expression value, Expression validEnumValues)
            => Call(validEnumValues, validEnumValues.Type.GetPublicInstanceMethod("Contains"), value);

        private static Expression GetStringValueConversion(
            Expression sourceValue,
            Expression fallbackValue,
            Type nonNullableTargetEnumType,
            bool useSingleStatement)
        {
            if (!useSingleStatement &&
                 sourceValue.Type.GetNonNullableType() == typeof(char))
            {
                return GetEnumMappingSwitchBlock(
                    sourceValue,
                    fallbackValue,
                    nonNullableTargetEnumType,
                    (enumValue, enumNumericType) => new object[]
                    {
                        ChangeType(enumValue, enumNumericType).ToString()[0]
                    });
            }

            if (sourceValue.Type != typeof(string))
            {
                sourceValue = ToStringConverter.GetConversion(sourceValue);
            }

            if (!useSingleStatement)
            {
                return GetEnumMappingSwitchBlock(
                    sourceValue,
                    fallbackValue,
                    nonNullableTargetEnumType,
                    (enumValue, enumNumericType) => new object[]
                    {
                        ChangeType(enumValue, enumNumericType).ToString(),
                        enumValue.ToString().ToUpperInvariant()
                    },
                    (switchValue, _) => Call(switchValue, typeof(string)
                        .GetPublicInstanceMethod("ToUpperInvariant")));
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

            var numericOrNameConversion = Condition(
                isNumericTest,
                numericConversion,
                nameMatchingConversion);

            var convertedValueOrDefault = Condition(
                StringExpressionExtensions.GetIsNullOrWhiteSpaceCall(sourceValue),
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
            var validEnumValues = nonNullableTargetEnumType
                .GetEnumValuesArray(v => ChangeType(v, underlyingEnumType).ToString())
                .ToConstantExpression(typeof(ICollection<string>));

            var parsedString = GetStringParseCall(sourceValue, underlyingEnumType);

            var validValueOrFallback = Condition(
                GetIsValidEnumValueCheck(sourceValue, validEnumValues),
                parsedString.GetConversionTo(fallbackValue.Type),
                fallbackValue);

            return validValueOrFallback;
        }
    }
}