namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Configuration;
    using DataSources;
    using Extensions;
    using Members;

    internal class ComplexTypeTypeTestsFactory
    {
        public static Expression CreateFor(ObjectMapperData mapperData)
        {
            if (mapperData.IsPartOfDerivedTypeMapping || mapperData.HasSameSourceAsParent())
            {
                return Constants.EmptyExpression;
            }

            var derivedSourceTypes = mapperData.MapperContext
                .DerivedTypes
                .GetTypesDerivedFrom(mapperData.SourceType)
                .ToArray();

            var derivedTypePairs = mapperData.MapperContext
                .UserConfigurations
                .DerivedTypes
                .GetDerivedTypePairsFor(mapperData)
                .ToArray();

            if (derivedSourceTypes.None() && derivedTypePairs.None())
            {
                return Constants.EmptyExpression;
            }

            var typeTests = new List<Expression>();

            bool declaredTypeHasUnconditionalTypePair;

            AddDeclaredSourceTypeTests(
                derivedTypePairs,
                mapperData,
                typeTests,
                out declaredTypeHasUnconditionalTypePair);

            if (declaredTypeHasUnconditionalTypePair)
            {
                return typeTests.First();
            }

            var typedObjectVariables = new List<ParameterExpression>();

            AddDerivedSourceTypeTests(
                derivedSourceTypes,
                mapperData,
                typedObjectVariables,
                typeTests);

            return Expression.Block(typedObjectVariables, typeTests);
        }

        private static void AddDeclaredSourceTypeTests(
            IEnumerable<DerivedTypePair> derivedTypePairs,
            ObjectMapperData mapperData,
            ICollection<Expression> typeTests,
            out bool declaredTypeHasUnconditionalTypePair)
        {
            foreach (var derivedTypePair in derivedTypePairs)
            {
                var condition = GetTypePairCondition(derivedTypePair, mapperData);

                var mapping = InlineMappingFactory.GetRuntimeTypedMapping(
                    mapperData,
                    mapperData.SourceObject,
                    derivedTypePair.DerivedTargetType);

                var returnMappingResult = Expression.Return(mapperData.ReturnLabelTarget, mapping);
                declaredTypeHasUnconditionalTypePair = (condition == null);

                if (declaredTypeHasUnconditionalTypePair)
                {
                    typeTests.Add(returnMappingResult);
                    return;
                }

                var ifConditionThenMap = Expression.IfThen(condition, returnMappingResult);

                typeTests.Add(ifConditionThenMap);
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

            if (condition == null)
            {
                return pairCondition;
            }

            return Expression.AndAlso(pairCondition, condition);
        }

        private static void AddDerivedSourceTypeTests(
            IEnumerable<Type> derivedSourceTypes,
            ObjectMapperData mapperData,
            ICollection<ParameterExpression> typedObjectVariables,
            ICollection<Expression> typeTests)
        {
            foreach (var derivedSourceType in derivedSourceTypes)
            {
                var typedVariableName = "source" + derivedSourceType.GetVariableNameInPascalCase();
                var typedVariable = Expression.Variable(derivedSourceType, typedVariableName);
                var typeAsConversion = Expression.TypeAs(mapperData.SourceObject, derivedSourceType);
                var typedVariableAssignment = Expression.Assign(typedVariable, typeAsConversion);

                var targetType = mapperData.TargetType.GetRuntimeTargetType(derivedSourceType);

                var condition = GetTypePairCondition(typedVariable, derivedSourceType, ref targetType, mapperData);

                var mapping = InlineMappingFactory
                    .GetRuntimeTypedMapping(mapperData, typedVariable, targetType);

                var returnMappingResult = Expression.Return(mapperData.ReturnLabelTarget, mapping);
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