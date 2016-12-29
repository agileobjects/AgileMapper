namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Configuration;
    using Extensions;
    using Members;

    internal static class DerivedComplexTypeMappingsFactory
    {
        public static Expression CreateFor(IObjectMappingData declaredTypeMappingData)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;

            if (declaredTypeMapperData.Context.IsForDerivedType || declaredTypeMapperData.HasSameSourceAsParent())
            {
                return Constants.EmptyExpression;
            }

            var derivedSourceTypes = GetDerivedTypes(declaredTypeMapperData.SourceType, declaredTypeMapperData);
            var derivedTargetTypes = GetDerivedTargetTypesIfNecessary(declaredTypeMappingData);
            var derivedTypePairs = GetTypePairsFor(declaredTypeMapperData, declaredTypeMapperData);

            if (derivedSourceTypes.None() && derivedTargetTypes.None() && derivedTypePairs.None())
            {
                return Constants.EmptyExpression;
            }

            var derivedTypeMappingExpressions = new List<Expression>();

            bool declaredTypeHasUnconditionalTypePair;

            AddDeclaredSourceTypeMappings(
                derivedTypePairs,
                declaredTypeMappingData,
                derivedTypeMappingExpressions,
                out declaredTypeHasUnconditionalTypePair);

            if (declaredTypeHasUnconditionalTypePair && derivedSourceTypes.None())
            {
                return derivedTypeMappingExpressions.First();
            }

            var typedObjectVariables = new List<ParameterExpression>();

            AddDerivedSourceTypeMappings(
                derivedSourceTypes,
                declaredTypeMappingData,
                typedObjectVariables,
                derivedTypeMappingExpressions);

            AddDerivedTargetTypeMappings(
                declaredTypeMappingData,
                derivedTargetTypes,
                derivedTypeMappingExpressions);

            return typedObjectVariables.Any()
                ? Expression.Block(typedObjectVariables, derivedTypeMappingExpressions)
                : Expression.Block(derivedTypeMappingExpressions);
        }

        private static ICollection<Type> GetDerivedTypes(Type parentType, IMemberMapperData mapperData)
            => mapperData.MapperContext.DerivedTypes.GetTypesDerivedFrom(parentType).ToArray();

        private static ICollection<Type> GetDerivedTargetTypesIfNecessary(IObjectMappingData mappingData)
        {
            if (mappingData.MapperData.Context.IsForNewElement)
            {
                return Enumerable<Type>.EmptyArray;
            }

            if (mappingData.IsRoot && !mappingData.MappingContext.RuleSet.RootHasPopulatedTarget)
            {
                return Enumerable<Type>.EmptyArray;
            }

            return GetDerivedTypes(mappingData.MapperData.TargetType, mappingData.MapperData);
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

                var derivedTypeMapping = DerivedMappingFactory.GetDerivedTypeMapping(
                    declaredTypeMappingData,
                    declaredTypeMapperData.SourceObject,
                    derivedTypePair.DerivedTargetType);

                var returnMappingResult = Expression.Return(declaredTypeMapperData.ReturnLabelTarget, derivedTypeMapping);
                declaredTypeHasUnconditionalTypePair = (condition == null);

                if (declaredTypeHasUnconditionalTypePair)
                {
                    derivedTypeMappingExpressions.Add(returnMappingResult);
                    return;
                }

                var ifConditionThenMap = Expression.IfThen(condition, returnMappingResult);

                derivedTypeMappingExpressions.Add(ifConditionThenMap);
            }

            declaredTypeHasUnconditionalTypePair = false;
        }

        private static Expression GetTypePairCondition(DerivedTypePair derivedTypePair, IMemberMapperData mapperData)
        {
            var condition = GetTargetValidCheckOrNull(derivedTypePair.DerivedTargetType, mapperData);

            if (!derivedTypePair.HasConfiguredCondition)
            {
                return condition;
            }

            var pairCondition = derivedTypePair.GetConditionOrNull(mapperData);

            return (condition != null) ? Expression.AndAlso(pairCondition, condition) : pairCondition;
        }

        private static void AddDerivedSourceTypeMappings(
            IEnumerable<Type> derivedSourceTypes,
            IObjectMappingData declaredTypeMappingData,
            ICollection<ParameterExpression> typedObjectVariables,
            IList<Expression> derivedTypeMappingExpressions)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;
            var insertionOffset = derivedTypeMappingExpressions.Count;

            derivedSourceTypes = derivedSourceTypes
                .OrderBy(t => t, TypeComparer.MostToLeastDerived);

            foreach (var derivedSourceType in derivedSourceTypes)
            {
                var typedVariableName = "source" + derivedSourceType.GetVariableNameInPascalCase();
                var typedVariable = Expression.Variable(derivedSourceType, typedVariableName);
                var typeAsConversion = Expression.TypeAs(declaredTypeMapperData.SourceObject, derivedSourceType);
                var typedVariableAssignment = typedVariable.AssignTo(typeAsConversion);

                typedObjectVariables.Add(typedVariable);
                derivedTypeMappingExpressions.Insert(typedVariableAssignment, insertionOffset);

                var targetType = declaredTypeMapperData.TargetType.GetRuntimeTargetType(derivedSourceType);

                var outerCondition = typedVariable.GetIsNotDefaultComparison();
                outerCondition = AppendTargetValidCheckIfAppropriate(outerCondition, targetType, declaredTypeMapperData);

                var derivedTypePairs = GetTypePairsFor(derivedSourceType, targetType, declaredTypeMapperData);

                Expression ifSourceVariableIsDerivedTypeThenMap;

                if (derivedTypePairs.None())
                {
                    ifSourceVariableIsDerivedTypeThenMap = GetIfConditionThenMapExpression(
                        declaredTypeMappingData,
                        outerCondition,
                        typedVariable,
                        targetType);

                    derivedTypeMappingExpressions.Insert(ifSourceVariableIsDerivedTypeThenMap, insertionOffset);
                    continue;
                }

                var groupedTypePairs = derivedTypePairs
                    .GroupBy(tp => tp.DerivedTargetType)
                    .Select(group => new TypePairGroup(group))
                    .OrderBy(tp => tp.DerivedTargetType, TypeComparer.MostToLeastDerived)
                    .ToArray();

                var unconditionalDerivedTargetTypeMapping = groupedTypePairs
                    .Where(tpg => tpg.TypePairs.None(tp => tp.HasConfiguredCondition))
                    .Select(tpg => new
                    {
                        tpg.DerivedTargetType,
                        TypePairsCondition = GetTargetValidCheckOrNull(tpg.DerivedTargetType, declaredTypeMapperData)
                    })
                    .FirstOrDefault(d => d.TypePairsCondition == null);

                if (unconditionalDerivedTargetTypeMapping != null)
                {
                    ifSourceVariableIsDerivedTypeThenMap = GetIfConditionThenMapExpression(
                        declaredTypeMappingData,
                        outerCondition,
                        typedVariable,
                        unconditionalDerivedTargetTypeMapping.DerivedTargetType);

                    derivedTypeMappingExpressions.Insert(ifSourceVariableIsDerivedTypeThenMap, insertionOffset);
                    continue;
                }

                ifSourceVariableIsDerivedTypeThenMap = GetMapFromConditionOrDefaultExpression(
                    declaredTypeMappingData,
                    outerCondition,
                    typedVariable,
                    groupedTypePairs,
                    targetType);

                derivedTypeMappingExpressions.Insert(ifSourceVariableIsDerivedTypeThenMap, insertionOffset);
            }
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

                derivedTypeMappingExpressions.Add(ifDerivedTargetTypeThenMap);
            }
        }

        private static Expression GetIfConditionThenMapExpression(
            IObjectMappingData mappingData,
            Expression condition,
            Expression sourceValue,
            Type targetType)
        {
            var returnMappingResult = GetReturnMappingResultExpression(mappingData, sourceValue, targetType);
            var ifConditionThenMap = Expression.IfThen(condition, returnMappingResult);

            return ifConditionThenMap;
        }

        private static Expression GetReturnMappingResultExpression(
            IObjectMappingData mappingData,
            Expression sourceValue,
            Type targetType)
        {
            var mapping = DerivedMappingFactory.GetDerivedTypeMapping(mappingData, sourceValue, targetType);
            var returnMappingResult = Expression.Return(mappingData.MapperData.ReturnLabelTarget, mapping);

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

            var ifSourceVariableIsDerivedTypeThenMap = Expression
                .IfThen(condition, Expression.Block(mappingExpressions));

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

        private static IList<DerivedTypePair> GetTypePairsFor(IBasicMapperData pairTestMapperData, IMemberMapperData mapperData)
        {
            var derivedTypePairs = mapperData.MapperContext.UserConfigurations
                .DerivedTypes
                .GetDerivedTypePairsFor(pairTestMapperData, mapperData.MapperContext)
                .ToArray();

            return derivedTypePairs;
        }

        private static Expression GetTypePairsCondition(
            IEnumerable<DerivedTypePair> derivedTypePairs,
            IMemberMapperData mapperData)
        {
            var conditionalPairs = derivedTypePairs
                .Where(pair => pair.HasConfiguredCondition)
                .ToArray();

            var pairConditions = conditionalPairs.Chain(
                firstPair => firstPair.GetConditionOrNull(mapperData),
                (conditionSoFar, pair) => Expression.OrElse(
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

            condition = Expression.AndAlso(condition, targetIsValid);

            return condition;
        }

        private static Expression GetTargetValidCheckOrNull(Type targetType, IMemberMapperData mapperData)
        {
            if (mapperData.Context.IsForNewElement || !mapperData.TargetMember.IsReadable)
            {
                return null;
            }

            if (mapperData.IsRoot && !mapperData.RuleSet.RootHasPopulatedTarget)
            {
                return null;
            }

            var targetIsOfDerivedType = GetTargetIsDerivedTypeCheck(targetType, mapperData);

            if (mapperData.IsRoot && mapperData.RuleSet.RootHasPopulatedTarget)
            {
                return targetIsOfDerivedType;
            }

            var targetIsNull = mapperData.TargetObject.GetIsDefaultComparison();
            var targetIsValid = Expression.OrElse(targetIsNull, targetIsOfDerivedType);

            return targetIsValid;
        }

        private static Expression GetTargetIsDerivedTypeCheck(Type targetType, IMemberMapperData mapperData)
            => Expression.TypeIs(mapperData.TargetObject, targetType);

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

            public ICollection<DerivedTypePair> TypePairs { get; }
        }
    }
}