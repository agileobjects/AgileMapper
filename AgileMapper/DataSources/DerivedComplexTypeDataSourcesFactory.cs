namespace AgileObjects.AgileMapper.DataSources
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
    using ObjectPopulation;
    using static Constants;
    using static TypeComparer;

    internal static class DerivedComplexTypeDataSourcesFactory
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

            var derivedTypeDataSources = new List<IDataSource>();

            if (hasDerivedTypePairs)
            {
                AddDeclaredSourceTypeDataSources(
                    derivedTypePairs,
                    declaredTypeMappingData,
                    derivedTypeDataSources);

                if (hasNoDerivedSourceTypes && !derivedTypeDataSources.Last().IsConditional)
                {
                    goto ReturnDerivedTypeMappings;
                }
            }

            if (hasDerivedSourceTypes)
            {
                AddDerivedSourceTypeDataSources(
                    derivedSourceTypes,
                    declaredTypeMappingData,
                    derivedTypeDataSources);
            }

            if (hasDerivedTargetTypes)
            {
                AddDerivedTargetTypeDataSources(
                    declaredTypeMappingData,
                    derivedTargetTypes,
                    derivedTypeDataSources);
            }

        ReturnDerivedTypeMappings:

            var derivedTypeDataSourceSet = DataSourceSet.For(
                derivedTypeDataSources,
                declaredTypeMapperData,
                ValueExpressionBuilders.SequentialValues);

            return derivedTypeDataSourceSet.BuildValue();
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

        private static ICollection<DerivedTypePair> GetTypePairsFor(IBasicMapperData pairTestMapperData, IMemberMapperData mapperData)
        {
            var derivedTypePairs = mapperData.MapperContext.UserConfigurations
                .DerivedTypes
                .GetDerivedTypePairsFor(pairTestMapperData, mapperData.MapperContext);

            return derivedTypePairs;
        }

        private static void AddDeclaredSourceTypeDataSources(
            IEnumerable<DerivedTypePair> derivedTypePairs,
            IObjectMappingData declaredTypeMappingData,
            ICollection<IDataSource> derivedTypeDataSources)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;

            derivedTypePairs = derivedTypePairs
                .OrderBy(tp => tp.DerivedSourceType, MostToLeastDerived);

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
                    derivedTypePair.DerivedTargetType,
                    out var derivedTypeMappingData);

                if (sourceValueCondition != null)
                {
                    derivedTypeMapping = Condition(
                        sourceValueCondition,
                        derivedTypeMapping,
                        derivedTypeMapping.Type.ToDefaultExpression());
                }

                var returnMappingResult = Return(declaredTypeMapperData.ReturnLabelTarget, derivedTypeMapping);

                var derivedTypeMappingDataSource = new AdHocDataSource(
                    derivedTypeMappingData.MapperData.SourceMember,
                    returnMappingResult,
                    condition);

                derivedTypeDataSources.Add(derivedTypeMappingDataSource);

                if (!derivedTypeMappingDataSource.IsConditional)
                {
                    return;
                }
            }
        }

        private static Expression GetTypePairCondition(DerivedTypePair derivedTypePair, IMemberMapperData declaredTypeMapperData)
        {
            var condition = declaredTypeMapperData.GetTargetValidCheckOrNull(derivedTypePair.DerivedTargetType);

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

        private static void AddDerivedSourceTypeDataSources(
            IEnumerable<Type> derivedSourceTypes,
            IObjectMappingData declaredTypeMappingData,
            IList<IDataSource> derivedTypeDataSources)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;
            var insertionOffset = derivedTypeDataSources.Count;

            var orderedDerivedSourceTypes = derivedSourceTypes
                .OrderBy(t => t, MostToLeastDerived);

            foreach (var derivedSourceType in orderedDerivedSourceTypes)
            {
                var derivedSourceCheck = new DerivedSourceTypeCheck(derivedSourceType);
                var targetType = declaredTypeMapperData.TargetType.GetRuntimeTargetType(derivedSourceType);

                var outerCondition = derivedSourceCheck.TypeCheck;
                outerCondition = AppendTargetValidCheckIfAppropriate(outerCondition, targetType, declaredTypeMapperData);

                var derivedTypePairs = GetTypePairsFor(derivedSourceType, targetType, declaredTypeMapperData);

                IDataSource sourceVariableIsDerivedTypeDataSource;

                if (derivedTypePairs.None())
                {
                    sourceVariableIsDerivedTypeDataSource = GetReturnMappingResultDataSource(
                        declaredTypeMappingData,
                        outerCondition,
                        derivedSourceCheck,
                        targetType);

                    derivedTypeDataSources.Insert(sourceVariableIsDerivedTypeDataSource, insertionOffset);
                    continue;
                }

                var hasUnconditionalDerivedTargetTypeMapping = HasUnconditionalDerivedTargetTypeMapping(
                    derivedTypePairs,
                    declaredTypeMapperData,
                    out var unconditionalDerivedTargetType,
                    out var groupedTypePairs);

                if (hasUnconditionalDerivedTargetTypeMapping)
                {
                    sourceVariableIsDerivedTypeDataSource = GetReturnMappingResultDataSource(
                        declaredTypeMappingData,
                        outerCondition,
                        derivedSourceCheck,
                        unconditionalDerivedTargetType);

                    derivedTypeDataSources.Insert(sourceVariableIsDerivedTypeDataSource, insertionOffset);
                    continue;
                }

                sourceVariableIsDerivedTypeDataSource = GetMapFromConditionOrDefaultDataSource(
                    declaredTypeMappingData,
                    outerCondition,
                    derivedSourceCheck,
                    groupedTypePairs,
                    targetType);

                derivedTypeDataSources.Insert(sourceVariableIsDerivedTypeDataSource, insertionOffset);
            }
        }

        private static IDataSource GetMapFromConditionOrDefaultDataSource(
            IObjectMappingData declaredTypeMappingData,
            Expression condition,
            DerivedSourceTypeCheck derivedSourceCheck,
            IEnumerable<TypePairGroup> typePairGroups,
            Type targetType)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;
            var typePairDataSources = new List<IDataSource>();

            Expression derivedTypeMapping;
            IObjectMappingData derivedTypeMappingData;

            foreach (var typePairGroup in typePairGroups)
            {
                var typePairsCondition =
                    declaredTypeMapperData.GetTypePairsCondition(typePairGroup.TypePairs) ??
                    declaredTypeMapperData.GetTargetValidCheckOrNull(typePairGroup.DerivedTargetType);

                derivedTypeMapping = GetReturnMappingResultExpression(
                    declaredTypeMappingData,
                    derivedSourceCheck.TypedVariable,
                    typePairGroup.DerivedTargetType,
                    out derivedTypeMappingData);

                var typePairDataSource = new AdHocDataSource(
                    derivedTypeMappingData.MapperData.SourceMember,
                    derivedTypeMapping,
                    typePairsCondition);

                typePairDataSources.Add(typePairDataSource);
            }

            var derivedTargetTypeDataSources = DataSourceSet.For(
                typePairDataSources,
                declaredTypeMapperData,
                ValueExpressionBuilders.SequentialValues);

            derivedTypeMapping = GetReturnMappingResultExpression(
                declaredTypeMappingData,
                derivedSourceCheck.TypedVariable,
                targetType,
                out derivedTypeMappingData);

            var derivedTypeMappings = Block(
                derivedSourceCheck.GetTypedVariableAssignment(declaredTypeMapperData),
                derivedTargetTypeDataSources.BuildValue(),
                derivedTypeMapping);

            return new AdHocDataSource(
                derivedTypeMappingData.MapperData.SourceMember,
                derivedTypeMappings,
                condition,
                derivedSourceCheck.TypedVariable);
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

            var targetIsValid = mapperData.GetTargetValidCheckOrNull(targetType);

            if (targetIsValid == null)
            {
                return condition;
            }

            condition = AndAlso(condition, targetIsValid);

            return condition;
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
                .OrderBy(tp => tp.DerivedTargetType, MostToLeastDerived)
                .ToArray();

            var unconditionalTypePairs = groupedTypePairs
                .Filter(tpg => tpg.TypePairs.None(tp => tp.HasConfiguredCondition));

            foreach (var unconditionalTypePair in unconditionalTypePairs)
            {
                var typePairsCondition = declaredTypeMapperData
                    .GetTargetValidCheckOrNull(unconditionalTypePair.DerivedTargetType);

                if (typePairsCondition == null)
                {
                    unconditionalDerivedTargetType = unconditionalTypePair.DerivedTargetType;
                    return true;
                }
            }

            unconditionalDerivedTargetType = null;
            return false;
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

        private static void AddDerivedTargetTypeDataSources(
            IObjectMappingData declaredTypeMappingData,
            IEnumerable<Type> derivedTargetTypes,
            ICollection<IDataSource> derivedTypeDataSources)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;

            derivedTargetTypes = derivedTargetTypes.OrderBy(t => t, MostToLeastDerived);

            foreach (var derivedTargetType in derivedTargetTypes)
            {
                var targetTypeCondition = declaredTypeMapperData.GetTargetIsDerivedTypeCheck(derivedTargetType);

                var derivedTypeMapping = GetReturnMappingResultExpression(
                    declaredTypeMappingData,
                    declaredTypeMapperData.SourceObject,
                    derivedTargetType,
                    out var derivedTypeMappingData);

                var derivedTargetTypeDataSouce = new AdHocDataSource(
                    derivedTypeMappingData.MapperData.SourceMember,
                    derivedTypeMapping,
                    targetTypeCondition);

                derivedTypeDataSources.Add(derivedTargetTypeDataSouce);
            }
        }

        private static IDataSource GetReturnMappingResultDataSource(
            IObjectMappingData declaredTypeMappingData,
            Expression condition,
            DerivedSourceTypeCheck derivedSourceCheck,
            Type targetType)
        {
            var typedVariableAssignment = derivedSourceCheck
                .GetTypedVariableAssignment(declaredTypeMappingData.MapperData);

            var derivedTypeMapping = GetReturnMappingResultExpression(
                declaredTypeMappingData,
                derivedSourceCheck.TypedVariable,
                targetType,
                out var derivedTypeMappingData);

            return new AdHocDataSource(
                derivedTypeMappingData.MapperData.SourceMember,
                Block(typedVariableAssignment, derivedTypeMapping),
                condition,
                derivedSourceCheck.TypedVariable);
        }

        private static Expression GetReturnMappingResultExpression(
            IObjectMappingData declaredTypeMappingData,
            Expression sourceValue,
            Type targetType,
            out IObjectMappingData derivedTypeMappingData)
        {
            var mapping = DerivedMappingFactory.GetDerivedTypeMapping(
                declaredTypeMappingData,
                sourceValue,
                targetType,
                out derivedTypeMappingData);

            return (mapping != EmptyExpression)
                ? Return(declaredTypeMappingData.MapperData.ReturnLabelTarget, mapping)
                : mapping;
        }
    }

    internal class TypePairGroup
    {
        public TypePairGroup(IGrouping<Type, DerivedTypePair> typePairGroup)
        {
            DerivedTargetType = typePairGroup.Key;
            TypePairs = typePairGroup.ToArray();
        }

        public Type DerivedTargetType { get; }

        public IList<DerivedTypePair> TypePairs { get; }
    }

    internal static class DerivedTypeMappingExtensions
    {
        public static Expression GetTypePairsCondition(
            this IMemberMapperData mapperData,
            IEnumerable<DerivedTypePair> derivedTypePairs)
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

        public static Expression GetTargetValidCheckOrNull(this IMemberMapperData mapperData, Type targetType)
        {
            if (!mapperData.TargetMember.IsReadable || mapperData.TargetIsDefinitelyUnpopulated())
            {
                return null;
            }

            var targetIsOfDerivedType = mapperData.GetTargetIsDerivedTypeCheck(targetType);

            if (mapperData.TargetIsDefinitelyPopulated())
            {
                return targetIsOfDerivedType;
            }

            var targetIsNull = mapperData.TargetObject.GetIsDefaultComparison();
            var targetIsValid = OrElse(targetIsNull, targetIsOfDerivedType);

            return targetIsValid;
        }

        public static Expression GetTargetIsDerivedTypeCheck(this IMemberMapperData mapperData, Type targetType)
            => TypeIs(mapperData.TargetObject, targetType);
    }
}