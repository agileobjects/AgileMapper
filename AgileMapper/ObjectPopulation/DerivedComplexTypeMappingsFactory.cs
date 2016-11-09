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

            var derivedTypeMappings = new List<Expression>();

            bool declaredTypeHasUnconditionalTypePair;

            AddDeclaredSourceTypeMappings(
                derivedTypePairs,
                declaredTypeMappingData,
                derivedTypeMappings,
                out declaredTypeHasUnconditionalTypePair);

            if (declaredTypeHasUnconditionalTypePair)
            {
                return derivedTypeMappings.First();
            }

            var typedObjectVariables = new List<ParameterExpression>();

            AddDerivedSourceTypeMappings(
                derivedSourceTypes,
                declaredTypeMappingData,
                typedObjectVariables,
                derivedTypeMappings);

            return Expression.Block(typedObjectVariables, derivedTypeMappings);
        }

        private static void AddDeclaredSourceTypeMappings(
            IEnumerable<DerivedTypePair> derivedTypePairs,
            IObjectMappingData declaredTypeMappingData,
            ICollection<Expression> derivedTypeMappings,
            out bool declaredTypeHasUnconditionalTypePair)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;

            foreach (var derivedTypePair in derivedTypePairs)
            {
                var condition = GetTypePairCondition(derivedTypePair, declaredTypeMapperData);

                var derivedTypeMapping = MappingFactory.GetDerivedTypeMapping(
                    declaredTypeMappingData,
                    declaredTypeMapperData.SourceObject,
                    derivedTypePair.DerivedTargetType);

                var returnMappingResult = Expression.Return(declaredTypeMapperData.ReturnLabelTarget, derivedTypeMapping);
                declaredTypeHasUnconditionalTypePair = (condition == null);

                if (declaredTypeHasUnconditionalTypePair)
                {
                    derivedTypeMappings.Add(returnMappingResult);
                    return;
                }

                var ifConditionThenMap = Expression.IfThen(condition, returnMappingResult);

                derivedTypeMappings.Add(ifConditionThenMap);
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
            ICollection<Expression> typeTests)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;

            foreach (var derivedSourceType in derivedSourceTypes)
            {
                var typedVariableName = "source" + derivedSourceType.GetVariableNameInPascalCase();
                var typedVariable = Expression.Variable(derivedSourceType, typedVariableName);
                var typeAsConversion = Expression.TypeAs(declaredTypeMapperData.SourceObject, derivedSourceType);
                var typedVariableAssignment = Expression.Assign(typedVariable, typeAsConversion);

                var targetType = declaredTypeMapperData.TargetType.GetRuntimeTargetType(derivedSourceType);

                var condition = GetTypePairCondition(typedVariable, derivedSourceType, ref targetType, declaredTypeMapperData);

                var mapping = MappingFactory
                    .GetDerivedTypeMapping(declaredTypeMappingData, typedVariable, targetType);

                var returnMappingResult = Expression.Return(declaredTypeMapperData.ReturnLabelTarget, mapping);
                var ifConditionThenMap = Expression.IfThen(condition, returnMappingResult);

                typedObjectVariables.Add(typedVariable);
                typeTests.Add(typedVariableAssignment);
                typeTests.Add(ifConditionThenMap);
            }
        }

        private static Expression GetTypePairCondition(
            Expression typedVariable,
            Type derivedSourceType,
            ref Type targetType,
            IMemberMapperData mapperData)
        {
            Expression condition = typedVariable.GetIsNotDefaultComparison();

            var derivedTypePairs = GetTypePairsFor(derivedSourceType, targetType, mapperData);

            if (derivedTypePairs.Any())
            {
                targetType = derivedTypePairs[0].DerivedTargetType;
                condition = AppendTypePairConditionIfRequired(condition, derivedTypePairs, mapperData);
            }

            condition = AppendTargetValidCheckIfAppropriate(condition, targetType, mapperData);

            return condition;
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
                .GetDerivedTypePairsFor(pairTestMapperData)
                .ToArray();

            return derivedTypePairs;
        }

        private static Expression AppendTypePairConditionIfRequired(
            Expression condition,
            IEnumerable<DerivedTypePair> derivedTypePairs,
            IMemberMapperData mapperData)
        {
            var conditionalPairs = derivedTypePairs
                .Where(pair => pair.HasConfiguredCondition)
                .ToArray();

            if (conditionalPairs.None())
            {
                return condition;
            }

            var pairConditions = conditionalPairs.Skip(1).Aggregate(
                conditionalPairs[0].GetConditionOrNull(mapperData),
                (conditionSoFar, pair) => Expression.OrElse(
                    conditionSoFar,
                    pair.GetConditionOrNull(mapperData)));

            return Expression.AndAlso(condition, pairConditions);
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
            if (!mapperData.TargetMember.IsReadable)
            {
                return null;
            }

            var targetIsNull = mapperData.TargetObject.GetIsDefaultComparison();
            var targetIsOfDerivedType = Expression.TypeIs(mapperData.TargetObject, targetType);
            var targetIsValid = Expression.OrElse(targetIsNull, targetIsOfDerivedType);

            return targetIsValid;
        }
    }
}