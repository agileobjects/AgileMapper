namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Configuration;
    using Extensions;
    using Members;

    internal class DerivedComplexTypeMappingsFactory
    {
        public static Expression CreateFor(IObjectMappingData declaredTypeMappingData)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;

            if (declaredTypeMapperData.Context.IsForDerivedType || declaredTypeMapperData.HasSameSourceAsParent())
            {
                return Constants.EmptyExpression;
            }

            var derivedSourceTypes = declaredTypeMapperData.MapperContext
                .DerivedTypes
                .GetTypesDerivedFrom(declaredTypeMapperData.SourceType)
                .ToArray();

            var derivedTypePairs = declaredTypeMapperData.MapperContext
                .UserConfigurations
                .DerivedTypes
                .GetDerivedTypePairsFor(declaredTypeMapperData)
                .ToArray();

            if (derivedSourceTypes.None() && derivedTypePairs.None())
            {
                return Constants.EmptyExpression;
            }

            var derivedTypeMappings = new List<DerivedTypeMapping>();

            bool declaredTypeHasUnconditionalTypePair;

            AddDeclaredSourceTypeMappings(
                derivedTypePairs,
                declaredTypeMappingData,
                derivedTypeMappings,
                out declaredTypeHasUnconditionalTypePair);

            if (declaredTypeHasUnconditionalTypePair && derivedSourceTypes.None())
            {
                return derivedTypeMappings.First().Mapping;
            }

            var typedObjectVariables = new List<ParameterExpression>();

            AddDerivedSourceTypeMappings(
                derivedSourceTypes,
                declaredTypeMappingData,
                typedObjectVariables,
                derivedTypeMappings);

            var mappingExpressions = derivedTypeMappings
                .OrderBy(mapping => mapping.SourceType, TypeComparer.Instance)
                .ThenBy(mapping => mapping.Priority)
                .Select(mapping => mapping.Mapping)
                .ToArray();

            return Expression.Block(typedObjectVariables, mappingExpressions);
        }

        private static void AddDeclaredSourceTypeMappings(
            IEnumerable<DerivedTypePair> derivedTypePairs,
            IObjectMappingData declaredTypeMappingData,
            ICollection<DerivedTypeMapping> derivedTypeMappings,
            out bool declaredTypeHasUnconditionalTypePair)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;

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
                    derivedTypeMappings.Add(DerivedTypeMapping.Unconditional(derivedTypePair, returnMappingResult));
                    return;
                }

                var ifConditionThenMap = Expression.IfThen(condition, returnMappingResult);

                derivedTypeMappings.Add(DerivedTypeMapping.Conditional(derivedTypePair, ifConditionThenMap));
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
            ICollection<DerivedTypeMapping> derivedTypeMappings)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;

            foreach (var derivedSourceType in derivedSourceTypes)
            {
                var typedVariableName = "source" + derivedSourceType.GetVariableNameInPascalCase();
                var typedVariable = Expression.Variable(derivedSourceType, typedVariableName);
                var typeAsConversion = Expression.TypeAs(declaredTypeMapperData.SourceObject, derivedSourceType);
                var typedVariableAssignment = Expression.Assign(typedVariable, typeAsConversion);

                typedObjectVariables.Add(typedVariable);
                derivedTypeMappings.Add(typedVariableAssignment);

                var targetType = declaredTypeMapperData.TargetType.GetRuntimeTargetType(derivedSourceType);

                var condition = typedVariable.GetIsNotDefaultComparison();
                condition = AppendTargetValidCheckIfAppropriate(condition, targetType, declaredTypeMapperData);

                var derivedTypePairs = GetTypePairsFor(derivedSourceType, targetType, declaredTypeMapperData);
                Expression typePairsCondition, ifSourceVariableIsDerivedTypeThenMap;

                if (derivedTypePairs.None())
                {
                    ifSourceVariableIsDerivedTypeThenMap = GetIfConditionThenMapExpression(
                        declaredTypeMappingData,
                        condition,
                        typedVariable,
                        targetType);

                    derivedTypeMappings.Add(DerivedTypeMapping
                        .Conditional(derivedSourceType, ifSourceVariableIsDerivedTypeThenMap));
                    continue;
                }

                var derivedTargetType = derivedTypePairs[0].DerivedTargetType;

                if (derivedTypePairs.All(tp => !tp.HasConfiguredCondition))
                {
                    typePairsCondition = GetTargetValidCheckOrNull(derivedTargetType, declaredTypeMapperData);

                    if (typePairsCondition == null)
                    {
                        ifSourceVariableIsDerivedTypeThenMap = GetIfConditionThenMapExpression(
                            declaredTypeMappingData,
                            condition,
                            typedVariable,
                            derivedTargetType);

                        derivedTypeMappings.Add(DerivedTypeMapping
                            .Conditional(derivedSourceType, ifSourceVariableIsDerivedTypeThenMap));
                        continue;
                    }

                    ifSourceVariableIsDerivedTypeThenMap = GetMapFromConditionOrDefaultExpression(
                        declaredTypeMappingData,
                        condition,
                        typePairsCondition,
                        typedVariable,
                        targetType,
                        derivedTargetType);

                    derivedTypeMappings.Add(DerivedTypeMapping
                        .Conditional(derivedSourceType, ifSourceVariableIsDerivedTypeThenMap));
                    continue;
                }

                typePairsCondition = GetTypePairsCondition(derivedTypePairs, declaredTypeMapperData);

                ifSourceVariableIsDerivedTypeThenMap = GetMapFromConditionOrDefaultExpression(
                    declaredTypeMappingData,
                    condition,
                    typePairsCondition,
                    typedVariable,
                    targetType,
                    derivedTargetType);

                derivedTypeMappings.Add(DerivedTypeMapping
                    .Conditional(derivedSourceType, ifSourceVariableIsDerivedTypeThenMap));
            }
        }

        private static Expression GetIfConditionThenMapExpression(
            IObjectMappingData mappingData,
            Expression condition,
            Expression typedVariable,
            Type targetType)
        {
            var returnMappingResult = GetReturnMappingResultExpression(mappingData, typedVariable, targetType);
            var ifConditionThenMap = Expression.IfThen(condition, returnMappingResult);

            return ifConditionThenMap;
        }

        private static Expression GetReturnMappingResultExpression(
            IObjectMappingData mappingData,
            Expression typedVariable,
            Type targetType)
        {
            var mapping = DerivedMappingFactory.GetDerivedTypeMapping(mappingData, typedVariable, targetType);
            var returnMappingResult = Expression.Return(mappingData.MapperData.ReturnLabelTarget, mapping);

            return returnMappingResult;
        }

        private static Expression GetMapFromConditionOrDefaultExpression(
            IObjectMappingData mappingData,
            Expression condition,
            Expression typePairsCondition,
            Expression typedVariable,
            Type targetType,
            Type derivedTargetType)
        {
            var ifTypePairsConditionThenMap = GetIfConditionThenMapExpression(
                mappingData,
                typePairsCondition,
                typedVariable,
                derivedTargetType);

            var mapToDerivedType = GetReturnMappingResultExpression(
                mappingData,
                typedVariable,
                targetType);

            var ifSourceVariableIsDerivedTypeThenMap = Expression.IfThen(
                condition,
                Expression.Block(ifTypePairsConditionThenMap, mapToDerivedType));

            return ifSourceVariableIsDerivedTypeThenMap;
        }

        private static IList<DerivedTypePair> GetTypePairsFor(
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

            var targetIsNull = mapperData.TargetObject.GetIsDefaultComparison();
            var targetIsOfDerivedType = Expression.TypeIs(mapperData.TargetObject, targetType);
            var targetIsValid = Expression.OrElse(targetIsNull, targetIsOfDerivedType);

            return targetIsValid;
        }

        #region Helper Class

        private class DerivedTypeMapping
        {
            private DerivedTypeMapping(Type sourceType, Expression mapping, bool? isConditional)
            {
                SourceType = sourceType;
                Mapping = mapping;
                Priority = isConditional.HasValue ? isConditional.Value ? 1 : 2 : 0;
            }

            public static implicit operator DerivedTypeMapping(BinaryExpression expression)
                => new DerivedTypeMapping(expression.Right.Type, expression, isConditional: null);

            public static DerivedTypeMapping Unconditional(DerivedTypePair typePair, Expression mapping)
                => new DerivedTypeMapping(typePair.DerivedSourceType, mapping, isConditional: false);

            public static DerivedTypeMapping Conditional(DerivedTypePair typePair, Expression mapping)
                => Conditional(typePair.DerivedSourceType, mapping);

            public static DerivedTypeMapping Conditional(Type sourceType, Expression mapping)
                => new DerivedTypeMapping(sourceType, mapping, isConditional: true);

            public Type SourceType { get; }

            public Expression Mapping { get; }

            public int Priority { get; }
        }

        #endregion
    }
}