namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.Expression;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.Expression;
#endif
    using Configuration;
    using Extensions;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using static Constants;

    internal static class DerivedComplexTypeMappingsFactory
    {
        public static Expression CreateFor(IObjectMappingData declaredTypeMappingData)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;

            if (DoNotMapDerivedTypes(declaredTypeMapperData))
            {
                return EmptyExpression;
            }

            var derivedSourceTypes = GetDerivedSourceTypesIfNecessary(declaredTypeMapperData);
            var hasDerivedSourceTypes = derivedSourceTypes.Any();
            var hasNoDerivedSourceTypes = !hasDerivedSourceTypes;

            var derivedTargetTypes = GetDerivedTargetTypesIfNecessary(declaredTypeMapperData);
            var hasDerivedTargetTypes = derivedTargetTypes.Any();

            var derivedTypePairs = GetTypePairsFor(declaredTypeMapperData, declaredTypeMapperData);
            var hasDerivedTypePairs = derivedTypePairs.Any();

            if (hasNoDerivedSourceTypes && !hasDerivedTargetTypes && !hasDerivedTypePairs)
            {
                return EmptyExpression;
            }

            var derivedTypeMappingExpressions = new List<Expression>();

            if (hasDerivedTypePairs)
            {
                AddDeclaredSourceTypeMappings(
                    derivedTypePairs,
                    declaredTypeMappingData,
                    derivedTypeMappingExpressions,
                    out var declaredTypeHasUnconditionalTypePair);

                if (declaredTypeHasUnconditionalTypePair && hasNoDerivedSourceTypes)
                {
                    return derivedTypeMappingExpressions.First();
                }
            }

            var typedObjectVariables = new List<ParameterExpression>();

            if (hasDerivedSourceTypes)
            {
                AddDerivedSourceTypeMappings(
                    derivedSourceTypes,
                    declaredTypeMappingData,
                    typedObjectVariables,
                    derivedTypeMappingExpressions);
            }

            if (hasDerivedTargetTypes)
            {
                AddDerivedTargetTypeMappings(
                    declaredTypeMappingData,
                    derivedTargetTypes,
                    derivedTypeMappingExpressions);
            }

            if (derivedTypeMappingExpressions.None())
            {
                return EmptyExpression;
            }

            return typedObjectVariables.Any()
                ? Block(typedObjectVariables, derivedTypeMappingExpressions)
                : derivedTypeMappingExpressions.HasOne()
                    ? derivedTypeMappingExpressions.First()
                    : Block(derivedTypeMappingExpressions);
        }

        private static bool DoNotMapDerivedTypes(IMemberMapperData mapperData)
        {
            if (mapperData.Context.IsForDerivedType)
            {
                return !mapperData.TargetType.IsInterface();
            }

            return mapperData.HasSameSourceAsParent();
        }

        private static ICollection<Type> GetDerivedSourceTypesIfNecessary(IMemberMapperData mapperData)
        {
            return mapperData.RuleSet.Settings.CheckDerivedSourceTypes
                ? mapperData.GetDerivedSourceTypes()
                : EmptyTypeArray;
        }

        private static ICollection<Type> GetDerivedTargetTypesIfNecessary(IMemberMapperData mapperData)
        {
            return mapperData.TargetCouldBePopulated()
                ? mapperData.GetDerivedTargetTypes()
                : EmptyTypeArray;
        }

        private static void AddDeclaredSourceTypeMappings(
            IEnumerable<DerivedTypePair> derivedTypePairs,
            IObjectMappingData declaredTypeMappingData,
            ICollection<Expression> derivedTypeMappingExpressions,
            out bool declaredTypeHasUnconditionalTypePair)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;

            derivedTypePairs = derivedTypePairs
                .OrderBy(tp => tp.DerivedSourceType, TypeComparer.MostToLeastDerived);

            foreach (var derivedTypePair in derivedTypePairs)
            {
                var condition = GetTypePairCondition(derivedTypePair, declaredTypeMapperData);

                var sourceValue = GetDerivedTypeSourceValue(
                    derivedTypePair,
                    declaredTypeMappingData,
                    out var sourceValueCondition);

                var derivedTypeMapping = DerivedMappingFactory.GetDerivedTypeMapping(
                    declaredTypeMappingData,
                    sourceValue,
                    derivedTypePair.DerivedTargetType);

                if (sourceValueCondition != null)
                {
                    derivedTypeMapping = Condition(
                        sourceValueCondition,
                        derivedTypeMapping,
                        derivedTypeMapping.Type.ToDefaultExpression());
                }

                var returnMappingResult = Return(declaredTypeMapperData.ReturnLabelTarget, derivedTypeMapping);

                if (condition == null)
                {
                    declaredTypeHasUnconditionalTypePair = true;
                    derivedTypeMappingExpressions.Add(returnMappingResult);
                    return;
                }

                var ifConditionThenMap = IfThen(condition, returnMappingResult);

                derivedTypeMappingExpressions.Add(ifConditionThenMap);
            }

            declaredTypeHasUnconditionalTypePair = false;
        }

        private static Expression GetTypePairCondition(DerivedTypePair derivedTypePair, IMemberMapperData declaredTypeMapperData)
        {
            var condition = GetTargetValidCheckOrNull(derivedTypePair.DerivedTargetType, declaredTypeMapperData);

            if (!derivedTypePair.HasConfiguredCondition)
            {
                return condition;
            }

            var pairCondition = derivedTypePair.GetConditionOrNull(declaredTypeMapperData);

            return (condition != null) ? AndAlso(pairCondition, condition) : pairCondition;
        }

        private static Expression GetDerivedTypeSourceValue(
            DerivedTypePair derivedTypePair,
            IObjectMappingData declaredTypeMappingData,
            out Expression sourceValueCondition)
        {
            if (!derivedTypePair.IsImplementationPairing)
            {
                sourceValueCondition = null;
                return declaredTypeMappingData.MapperData.SourceObject;
            }

            var implementationMappingData = declaredTypeMappingData
                .WithTypes(derivedTypePair.DerivedSourceType, derivedTypePair.DerivedTargetType);

            if (implementationMappingData.IsTargetConstructable())
            {
                sourceValueCondition = null;
                return declaredTypeMappingData.MapperData.SourceObject;
            }

            // Derived Type is an implementation Type for an unconstructable target Type,
            // and is itself unconstructable; only way we get here is if a ToTarget data
            // source has been configured:
            var toTargetDataSource = implementationMappingData
                .GetToTargetDataSourceOrNullForTargetType();

            sourceValueCondition = toTargetDataSource.IsConditional
                ? toTargetDataSource.Condition.Replace(
                    implementationMappingData.MapperData.SourceObject,
                    declaredTypeMappingData.MapperData.SourceObject,
                    ExpressionEvaluation.Equivalator)
                : null;

            return toTargetDataSource.Value.Replace(
                implementationMappingData.MapperData.SourceObject,
                declaredTypeMappingData.MapperData.SourceObject,
                ExpressionEvaluation.Equivalator);
        }

        private static void AddDerivedSourceTypeMappings(
            IEnumerable<Type> derivedSourceTypes,
            IObjectMappingData declaredTypeMappingData,
            ICollection<ParameterExpression> typedObjectVariables,
            IList<Expression> derivedTypeMappingExpressions)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;
            var insertionOffset = derivedTypeMappingExpressions.Count;

            var orderedDerivedSourceTypes = derivedSourceTypes
                .OrderBy(t => t, TypeComparer.MostToLeastDerived);

            foreach (var derivedSourceType in orderedDerivedSourceTypes)
            {
                var derivedSourceCheck = new DerivedSourceTypeCheck(derivedSourceType);
                var typedVariableAssignment = derivedSourceCheck.GetTypedVariableAssignment(declaredTypeMapperData);

                typedObjectVariables.Add(derivedSourceCheck.TypedVariable);
                derivedTypeMappingExpressions.Insert(typedVariableAssignment, insertionOffset);

                var targetType = declaredTypeMapperData.TargetType.GetRuntimeTargetType(derivedSourceType);

                var outerCondition = derivedSourceCheck.TypeCheck;
                outerCondition = AppendTargetValidCheckIfAppropriate(outerCondition, targetType, declaredTypeMapperData);

                var derivedTypePairs = GetTypePairsFor(derivedSourceType, targetType, declaredTypeMapperData);

                Expression ifSourceVariableIsDerivedTypeThenMap;

                if (derivedTypePairs.None())
                {
                    ifSourceVariableIsDerivedTypeThenMap = GetIfConditionThenMapExpression(
                        declaredTypeMappingData,
                        outerCondition,
                        derivedSourceCheck.TypedVariable,
                        targetType);

                    derivedTypeMappingExpressions.Insert(ifSourceVariableIsDerivedTypeThenMap, insertionOffset);
                    continue;
                }

                var hasUnconditionalDerivedTargetTypeMapping = HasUnconditionalDerivedTargetTypeMapping(
                    derivedTypePairs,
                    declaredTypeMapperData,
                    out var unconditionalDerivedTargetType,
                    out var groupedTypePairs);

                if (hasUnconditionalDerivedTargetTypeMapping)
                {
                    ifSourceVariableIsDerivedTypeThenMap = GetIfConditionThenMapExpression(
                        declaredTypeMappingData,
                        outerCondition,
                        derivedSourceCheck.TypedVariable,
                        unconditionalDerivedTargetType);

                    derivedTypeMappingExpressions.Insert(ifSourceVariableIsDerivedTypeThenMap, insertionOffset);
                    continue;
                }

                ifSourceVariableIsDerivedTypeThenMap = GetMapFromConditionOrDefaultExpression(
                    declaredTypeMappingData,
                    outerCondition,
                    derivedSourceCheck.TypedVariable,
                    groupedTypePairs,
                    targetType);

                derivedTypeMappingExpressions.Insert(ifSourceVariableIsDerivedTypeThenMap, insertionOffset);
            }
        }

        private static bool HasUnconditionalDerivedTargetTypeMapping(
            IEnumerable<DerivedTypePair> derivedTypePairs,
            IMemberMapperData declaredTypeMapperData,
            out Type unconditionalDerivedTargetType,
            out TypePairGroup[] groupedTypePairs)
        {
            groupedTypePairs = derivedTypePairs
                .GroupBy(tp => tp.DerivedTargetType)
                .Project(group => new TypePairGroup(group))
                .OrderBy(tp => tp.DerivedTargetType, TypeComparer.MostToLeastDerived)
                .ToArray();

            var unconditionalTypePairs = groupedTypePairs
                .Filter(tpg => tpg.TypePairs.None(tp => tp.HasConfiguredCondition));

            foreach (var unconditionalTypePair in unconditionalTypePairs)
            {
                var typePairsCondition = GetTargetValidCheckOrNull(
                    unconditionalTypePair.DerivedTargetType,
                    declaredTypeMapperData);

                if (typePairsCondition == null)
                {
                    unconditionalDerivedTargetType = unconditionalTypePair.DerivedTargetType;
                    return true;
                }
            }

            unconditionalDerivedTargetType = null;
            return false;
        }

        private static void AddDerivedTargetTypeMappings(
            IObjectMappingData declaredTypeMappingData,
            IEnumerable<Type> derivedTargetTypes,
            ICollection<Expression> derivedTypeMappingExpressions)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;

            derivedTargetTypes = derivedTargetTypes
                .OrderBy(t => t, TypeComparer.MostToLeastDerived);

            foreach (var derivedTargetType in derivedTargetTypes)
            {
                var targetTypeCondition = GetTargetIsDerivedTypeCheck(derivedTargetType, declaredTypeMapperData);

                var ifDerivedTargetTypeThenMap = GetIfConditionThenMapExpression(
                    declaredTypeMappingData,
                    targetTypeCondition,
                    declaredTypeMapperData.SourceObject,
                    derivedTargetType);

                derivedTypeMappingExpressions.AddUnlessNullOrEmpty(ifDerivedTargetTypeThenMap);
            }
        }

        private static Expression GetIfConditionThenMapExpression(
            IObjectMappingData mappingData,
            Expression condition,
            Expression sourceValue,
            Type targetType)
        {
            var returnMappingResult = GetReturnMappingResultExpression(mappingData, sourceValue, targetType);

            if (returnMappingResult == EmptyExpression)
            {
                return EmptyExpression;
            }

            var ifConditionThenMap = IfThen(condition, returnMappingResult);

            return ifConditionThenMap;
        }

        private static Expression GetReturnMappingResultExpression(
            IObjectMappingData mappingData,
            Expression sourceValue,
            Type targetType)
        {
            var mapping = DerivedMappingFactory.GetDerivedTypeMapping(mappingData, sourceValue, targetType);

            if (mapping == EmptyExpression)
            {
                return mapping;
            }

            var returnMappingResult = Return(mappingData.MapperData.ReturnLabelTarget, mapping);

            return returnMappingResult;
        }

        private static Expression GetMapFromConditionOrDefaultExpression(
            IObjectMappingData mappingData,
            Expression condition,
            Expression typedVariable,
            IEnumerable<TypePairGroup> typePairGroups,
            Type targetType)
        {
            var mappingExpressions = new List<Expression>();

            foreach (var typePairGroup in typePairGroups)
            {
                var typePairsCondition =
                    GetTypePairsCondition(typePairGroup.TypePairs, mappingData.MapperData) ??
                    GetTargetValidCheckOrNull(typePairGroup.DerivedTargetType, mappingData.MapperData);

                var ifTypePairsConditionThenMap = GetIfConditionThenMapExpression(
                    mappingData,
                    typePairsCondition,
                    typedVariable,
                    typePairGroup.DerivedTargetType);

                mappingExpressions.Add(ifTypePairsConditionThenMap);
            }

            var mapToDeclaredTargetType =
                GetReturnMappingResultExpression(mappingData, typedVariable, targetType);

            mappingExpressions.Add(mapToDeclaredTargetType);

            var ifSourceVariableIsDerivedTypeThenMap = IfThen(condition, Block(mappingExpressions));

            return ifSourceVariableIsDerivedTypeThenMap;
        }

        private static ICollection<DerivedTypePair> GetTypePairsFor(
            Type derivedSourceType,
            Type targetType,
            IMemberMapperData mapperData)
        {
            var pairTestMapperData = new BasicMapperData(
                mapperData.RuleSet,
                derivedSourceType,
                targetType,
                mapperData.TargetMember.WithType(targetType),
                mapperData.Parent);

            return GetTypePairsFor(pairTestMapperData, mapperData);
        }

        private static ICollection<DerivedTypePair> GetTypePairsFor(IBasicMapperData pairTestMapperData, IMemberMapperData mapperData)
        {
            var derivedTypePairs = mapperData.MapperContext.UserConfigurations
                .DerivedTypes
                .GetDerivedTypePairsFor(pairTestMapperData, mapperData.MapperContext);

            return derivedTypePairs;
        }

        private static Expression GetTypePairsCondition(
            IEnumerable<DerivedTypePair> derivedTypePairs,
            IMemberMapperData mapperData)
        {
            var conditionalPairs = derivedTypePairs
                .Filter(pair => pair.HasConfiguredCondition)
                .ToArray();

            var pairConditions = conditionalPairs.Chain(
                firstPair => firstPair.GetConditionOrNull(mapperData),
                (conditionSoFar, pair) => OrElse(
                    conditionSoFar,
                    pair.GetConditionOrNull(mapperData)));

            return pairConditions;
        }

        private static Expression AppendTargetValidCheckIfAppropriate(
            Expression condition,
            Type targetType,
            IMemberMapperData mapperData)
        {
            if (targetType == mapperData.TargetType)
            {
                return condition;
            }

            var targetIsValid = GetTargetValidCheckOrNull(targetType, mapperData);

            if (targetIsValid == null)
            {
                return condition;
            }

            condition = AndAlso(condition, targetIsValid);

            return condition;
        }

        private static Expression GetTargetValidCheckOrNull(Type targetType, IMemberMapperData mapperData)
        {
            if (!mapperData.TargetMember.IsReadable || mapperData.TargetIsDefinitelyUnpopulated())
            {
                return null;
            }

            var targetIsOfDerivedType = GetTargetIsDerivedTypeCheck(targetType, mapperData);

            if (mapperData.TargetIsDefinitelyPopulated())
            {
                return targetIsOfDerivedType;
            }

            var targetIsNull = mapperData.TargetObject.GetIsDefaultComparison();
            var targetIsValid = OrElse(targetIsNull, targetIsOfDerivedType);

            return targetIsValid;
        }

        private static Expression GetTargetIsDerivedTypeCheck(Type targetType, IMemberMapperData mapperData)
            => TypeIs(mapperData.TargetObject, targetType);

        private static void Insert(this IList<Expression> mappingExpressions, Expression mapping, int insertionOffset)
        {
            var insertionIndex = mappingExpressions.Count - insertionOffset;
            mappingExpressions.Insert(insertionIndex, mapping);
        }

        private class TypePairGroup
        {
            public TypePairGroup(IGrouping<Type, DerivedTypePair> typePairGroup)
            {
                DerivedTargetType = typePairGroup.Key;
                TypePairs = typePairGroup.ToArray();
            }

            public Type DerivedTargetType { get; }

            public IList<DerivedTypePair> TypePairs { get; }
        }
    }
}