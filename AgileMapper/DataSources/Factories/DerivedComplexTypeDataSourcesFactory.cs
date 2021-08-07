namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System;
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
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ReadableExpressions;
#if NET35
    using static Microsoft.Scripting.Ast.Expression;
#else
    using static System.Linq.Expressions.Expression;
#endif
    using static Constants;
    using static TypeComparer;

    internal static class DerivedComplexTypeDataSourcesFactory
    {
        public static IList<IDataSource> CreateFor(IObjectMappingData declaredTypeMappingData)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;

            if (DoNotMapDerivedTypes(declaredTypeMapperData))
            {
                return Enumerable<IDataSource>.EmptyArray;
            }

            var derivedSourceTypes = GetDerivedSourceTypesIfNecessary(declaredTypeMapperData);
            var hasDerivedSourceTypes = derivedSourceTypes.Any();
            var hasNoDerivedSourceTypes = !hasDerivedSourceTypes;

            var derivedTargetTypes = GetDerivedTargetTypesIfNecessary(declaredTypeMapperData);
            var hasDerivedTargetTypes = derivedTargetTypes.Any();

            var declaredSourceTypePairs = declaredTypeMapperData.GetDerivedTypePairs();
            var hasDeclaredSourceTypePairs = declaredSourceTypePairs.Any();

            if (hasNoDerivedSourceTypes && !hasDerivedTargetTypes && !hasDeclaredSourceTypePairs)
            {
                return Enumerable<IDataSource>.EmptyArray;
            }

            var derivedTypeDataSources = new List<IDataSource>();

            if (hasDeclaredSourceTypePairs)
            {
                AddDeclaredSourceTypeDataSources(
                    declaredSourceTypePairs,
                    declaredTypeMappingData,
                    derivedTypeDataSources);

                if (hasNoDerivedSourceTypes && !derivedTypeDataSources.Last().IsConditional)
                {
                    return derivedTypeDataSources;
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
                    derivedTargetTypes,
                    declaredTypeMappingData,
                    derivedTypeDataSources);
            }

            return derivedTypeDataSources;
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
                    derivedTypeMapping = derivedTypeMapping.ToIfFalseDefaultCondition(sourceValueCondition);
                }

                var returnMappingResult = declaredTypeMapperData.GetReturnExpression(derivedTypeMapping);

                var derivedTypeMappingDataSource = new DerivedComplexTypeDataSource(
                    derivedTypeMappingData.MapperData.SourceMember,
                    condition,
                    returnMappingResult);

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
                .WithDerivedTypes(derivedTypePair.DerivedSourceType, derivedTypePair.DerivedTargetType);

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

            if (((ICollection<Type>)derivedSourceTypes).Count > 1)
            {
                derivedSourceTypes = derivedSourceTypes.OrderBy(t => t, MostToLeastDerived);
            }

            foreach (var derivedSourceType in derivedSourceTypes)
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

                    goto AddDataSourceIfValid;
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

                    goto AddDataSourceIfValid;
                }

                sourceVariableIsDerivedTypeDataSource = GetMapFromConditionOrDefaultDataSource(
                    declaredTypeMappingData,
                    outerCondition,
                    derivedSourceCheck,
                    groupedTypePairs,
                    targetType);

            AddDataSourceIfValid:
                if (sourceVariableIsDerivedTypeDataSource.IsValid)
                {
                    derivedTypeDataSources.Insert(sourceVariableIsDerivedTypeDataSource, insertionOffset);
                }
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

                if (derivedTypeMapping == EmptyExpression)
                {
                    continue;
                }

                var typePairDataSource = new DerivedComplexTypeDataSource(
                    derivedTypeMappingData.MapperData.SourceMember,
                    typePairsCondition,
                    derivedTypeMapping);

                typePairDataSources.Add(typePairDataSource);
            }

            var derivedTargetTypeDataSources = DataSourceSet.For(
                typePairDataSources,
                declaredTypeMappingData,
                ValueExpressionBuilders.ValueSequence);

            derivedTypeMapping = GetReturnMappingResultExpression(
                declaredTypeMappingData,
                derivedSourceCheck.TypedVariable,
                targetType,
                out derivedTypeMappingData);

            var derivedTypeMappings = derivedTargetTypeDataSources.BuildValue();

            if (derivedTypeMappings != EmptyExpression)
            {
                if (derivedTypeMapping != EmptyExpression)
                {
                    derivedTypeMappings = Block(derivedTypeMappings, derivedTypeMapping);
                }
            }
            else if (derivedTypeMapping != EmptyExpression)
            {
                derivedTypeMappings = derivedTypeMapping;
            }
            else
            {
                return new NullDataSource(ReadableExpression.Comment("No data sources"));
            }

            return new DerivedComplexTypeDataSource(
                derivedTypeMappingData.MapperData.SourceMember,
                derivedSourceCheck,
                condition,
                derivedTypeMappings,
                declaredTypeMapperData);
        }

        private static Expression GetTypePairsCondition(
            this IMemberMapperData mapperData,
            IList<DerivedTypePair> derivedTypePairs)
        {
            var conditionalPairs = derivedTypePairs
                .FilterToArray(pair => pair.HasConfiguredCondition);

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

            var targetIsValid = mapperData.GetTargetValidCheckOrNull(targetType);

            if (targetIsValid == null)
            {
                return condition;
            }

            condition = AndAlso(condition, targetIsValid);

            return condition;
        }

        private static bool HasUnconditionalDerivedTargetTypeMapping(
            IList<DerivedTypePair> derivedTypePairs,
            IMemberMapperData declaredTypeMapperData,
            out Type unconditionalDerivedTargetType,
            out TypePairGroup[] groupedTypePairs)
        {
            IEnumerable<TypePairGroup> unconditionalTypePairs;

            if (derivedTypePairs.Count == 1)
            {
                var typePairGroup = new TypePairGroup(derivedTypePairs);
                groupedTypePairs = new[] { typePairGroup };

                if (typePairGroup.TypePairs.Any(tp => tp.HasConfiguredCondition))
                {
                    unconditionalDerivedTargetType = null;
                    return false;
                }

                unconditionalTypePairs = groupedTypePairs;
            }
            else
            {
                groupedTypePairs = derivedTypePairs
                    .GroupBy(tp => tp.DerivedTargetType)
                    .Project(group => new TypePairGroup(group))
                    .OrderBy(tp => tp.DerivedTargetType, MostToLeastDerived)
                    .ToArray();

                unconditionalTypePairs = groupedTypePairs
                    .FilterToArray(tpg => tpg.TypePairs.None(tp => tp.HasConfiguredCondition));
            }

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

        private static IList<DerivedTypePair> GetTypePairsFor(
            Type derivedSourceType,
            Type targetType,
            IMemberMapperData mapperData)
        {
            var pairTestMapperData = new QualifiedMemberContext(
                mapperData.RuleSet,
                derivedSourceType,
                targetType,
                mapperData.TargetMember.WithType(targetType),
                mapperData.Parent,
                mapperData.MapperContext);

            return pairTestMapperData.GetDerivedTypePairs();
        }

        private static void AddDerivedTargetTypeDataSources(
            ICollection<Type> derivedTargetTypes,
            IObjectMappingData declaredTypeMappingData,
            ICollection<IDataSource> derivedTypeDataSources)
        {
            var declaredTypeMapperData = declaredTypeMappingData.MapperData;

            if (declaredTypeMapperData.TargetMemberNullOrInaccessible())
            {
                return;
            }

            IEnumerable<Type> orderedDerivedTargetTypes = derivedTargetTypes;

            if (derivedTargetTypes.Count > 1)
            {
                orderedDerivedTargetTypes = orderedDerivedTargetTypes.OrderBy(t => t, MostToLeastDerived);
            }

            foreach (var derivedTargetType in orderedDerivedTargetTypes)
            {
                var targetTypeCondition = declaredTypeMapperData.GetTargetIsDerivedTypeCheck(derivedTargetType);

                var derivedTypeMapping = GetReturnMappingResultExpression(
                    declaredTypeMappingData,
                    declaredTypeMapperData.SourceObject,
                    derivedTargetType,
                    out var derivedTypeMappingData);

                if (derivedTypeMapping == EmptyExpression)
                {
                    continue;
                }

                var derivedTargetTypeDataSource = new DerivedComplexTypeDataSource(
                    derivedTypeMappingData.MapperData.SourceMember,
                    targetTypeCondition,
                    derivedTypeMapping);

                derivedTypeDataSources.Add(derivedTargetTypeDataSource);
            }
        }

        private static IDataSource GetReturnMappingResultDataSource(
            IObjectMappingData declaredTypeMappingData,
            Expression condition,
            DerivedSourceTypeCheck derivedSourceCheck,
            Type targetType)
        {
            var derivedTypeMapping = GetReturnMappingResultExpression(
                declaredTypeMappingData,
                derivedSourceCheck.TypedVariable,
                targetType,
                out var derivedTypeMappingData);

            return new DerivedComplexTypeDataSource(
                derivedTypeMappingData.MapperData.SourceMember,
                derivedSourceCheck,
                condition,
                derivedTypeMapping,
                declaredTypeMappingData.MapperData);
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
                ? declaredTypeMappingData.MapperData.GetReturnExpression(mapping)
                : mapping;
        }

        private static Expression GetTargetValidCheckOrNull(this IMemberMapperData mapperData, Type targetType)
        {
            if (mapperData.TargetMemberNullOrInaccessible())
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

        private static bool TargetMemberNullOrInaccessible(this IMemberMapperData mapperData)
            => !mapperData.TargetMember.IsReadable || mapperData.TargetIsDefinitelyUnpopulated();

        private static Expression GetTargetIsDerivedTypeCheck(this IMemberMapperData mapperData, Type targetType)
            => TypeIs(mapperData.TargetObject, targetType);

        private class DerivedComplexTypeDataSource : DataSourceBase
        {
            private readonly Expression _typedVariableAssignment;

            public DerivedComplexTypeDataSource(
                IQualifiedMember sourceMember,
                Expression condition,
                Expression value)
                : base(sourceMember, EmptyParameters, value, condition)
            {
            }

            public DerivedComplexTypeDataSource(
                IQualifiedMember sourceMember,
                DerivedSourceTypeCheck derivedSourceCheck,
                Expression condition,
                Expression value,
                IMemberMapperData declaredTypeMapperData)
                : base(sourceMember, new[] { derivedSourceCheck.TypedVariable }, value, condition)
            {
                _typedVariableAssignment = derivedSourceCheck
                    .GetTypedVariableAssignment(declaredTypeMapperData);
            }

            public override Expression AddSourceCondition(Expression value)
            {
                return (_typedVariableAssignment != null)
                       ? Block(_typedVariableAssignment, value)
                       : base.AddSourceCondition(value);
            }
        }

        private class TypePairGroup
        {
            public TypePairGroup(IList<DerivedTypePair> typePairs)
            {
                DerivedTargetType = typePairs[0].DerivedTargetType;
                TypePairs = typePairs;
            }

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