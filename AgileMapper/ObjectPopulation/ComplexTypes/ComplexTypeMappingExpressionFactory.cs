namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using DataSources.Factories;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    internal class ComplexTypeMappingExpressionFactory : MappingExpressionFactoryBase
    {
        public static readonly MappingExpressionFactoryBase Instance = new ComplexTypeMappingExpressionFactory();

        private readonly PopulationExpressionFactoryBase _memberInitPopulationFactory;
        private readonly PopulationExpressionFactoryBase _multiStatementPopulationFactory;
        private readonly IList<ISourceShortCircuitFactory> _shortCircuitFactories;

        public ComplexTypeMappingExpressionFactory()
        {
            _memberInitPopulationFactory = new MemberInitPopulationExpressionFactory();
            _multiStatementPopulationFactory = new MultiStatementPopulationExpressionFactory();

            _shortCircuitFactories = new[]
            {
                SourceDictionaryShortCircuitFactory.Instance
            };
        }

        protected override bool TargetCannotBeMapped(IObjectMappingData mappingData, out string reason)
        {
            if (mappingData.MapperData.TargetCouldBePopulated())
            {
                // If a target complex type is readonly or unconstructable 
                // we still try to map to it using an existing non-null value:
                return base.TargetCannotBeMapped(mappingData, out reason);
            }

            if (mappingData.IsTargetConstructable())
            {
                return base.TargetCannotBeMapped(mappingData, out reason);
            }

            var targetType = mappingData.MapperData.TargetType;

            if (targetType.IsAbstract() && DerivedTypesExistForTarget(mappingData.MapperData))
            {
                return base.TargetCannotBeMapped(mappingData, out reason);
            }

            reason = "Cannot construct an instance of " + targetType.GetFriendlyName();
            return true;
        }

        private static bool DerivedTypesExistForTarget(IMemberMapperData mapperData)
        {
            var configuredImplementationTypePairs = mapperData
                .MapperContext
                .UserConfigurations
                .DerivedTypes
                .GetImplementationTypePairsFor(mapperData);

            return configuredImplementationTypePairs.Any() ||
                   mapperData.GetDerivedTargetTypes().Any();
        }

        #region Short-Circuits

        protected override bool ShortCircuitMapping(MappingCreationContext context)
        {
            var derivedTypeDataSources = DerivedComplexTypeDataSourcesFactory.CreateFor(context.MappingData);

            if (derivedTypeDataSources.None())
            {
                return false;
            }

            var derivedTypeDataSourceSet = DataSourceSet.For(
                derivedTypeDataSources,
                context.MapperData,
                ValueExpressionBuilders.ValueSequence);

            var mapping = derivedTypeDataSourceSet.BuildValue();

            if (derivedTypeDataSources.Last().IsConditional && !context.MapperData.TargetType.IsAbstract())
            {
                context.MappingExpressions.Add(mapping);
                return false;
            }

            var shortCircuitReturns = GetShortCircuitReturns(context.MappingData).ToArray();

            if (shortCircuitReturns.Any())
            {
                context.MappingExpressions.AddRange(shortCircuitReturns);
            }

            if (mapping.NodeType == ExpressionType.Goto)
            {
                mapping = ((GotoExpression)mapping).Value;
            }
            else
            {
                context.MappingExpressions.Add(mapping);
                mapping = context.MapperData.GetTargetTypeDefault();
            }

            context.MappingExpressions.Add(context.MapperData.GetReturnLabel(mapping));
            return true;
        }

        protected override IEnumerable<Expression> GetShortCircuitReturns(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (SourceObjectCouldBeNull(mapperData))
            {
                yield return Expression.IfThen(
                    mapperData.SourceObject.GetIsDefaultComparison(),
                    GetReturnNull(mapperData));
            }

            var alreadyMappedShortCircuit = GetAlreadyMappedObjectShortCircuitOrNull(mapperData);

            if (alreadyMappedShortCircuit != null)
            {
                yield return alreadyMappedShortCircuit;
            }

            if (TryGetShortCircuitFactory(mapperData, out var sourceShortCircuitFactory))
            {
                yield return sourceShortCircuitFactory.GetShortCircuit(mappingData);
            }
        }

        private static bool SourceObjectCouldBeNull(IMemberMapperData mapperData)
        {
            if (mapperData.Context.IsForDerivedType)
            {
                return false;
            }

            if (mapperData.SourceType.IsValueType())
            {
                return false;
            }

            if (mapperData.RuleSet.Settings.SourceElementsCouldBeNull && mapperData.TargetMemberIsEnumerableElement())
            {
                return !mapperData.HasSameSourceAsParent();
            }

            return false;
        }

        private static Expression GetReturnNull(ObjectMapperData mapperData)
            => Expression.Return(mapperData.ReturnLabelTarget, mapperData.GetTargetTypeDefault());

        private static Expression GetAlreadyMappedObjectShortCircuitOrNull(ObjectMapperData mapperData)
        {
            if (!mapperData.RuleSet.Settings.AllowObjectTracking ||
                !mapperData.CacheMappedObjects ||
                 mapperData.TargetTypeHasNotYetBeenMapped ||
                 mapperData.SourceType.IsDictionary())
            {
                return null;
            }

            var tryGetMethod = typeof(IObjectMappingDataUntyped)
                .GetPublicInstanceMethod("TryGet")
                .MakeGenericMethod(mapperData.SourceType, mapperData.TargetType);

            var tryGetCall = Expression.Call(
                mapperData.EntryPointMapperData.MappingDataObject,
                tryGetMethod,
                mapperData.SourceObject,
                mapperData.TargetInstance);

            var ifTryGetReturn = Expression.IfThen(
                tryGetCall,
                Expression.Return(mapperData.ReturnLabelTarget, mapperData.TargetInstance));

            return ifTryGetReturn;
        }

        private bool TryGetShortCircuitFactory(ObjectMapperData mapperData, out ISourceShortCircuitFactory applicableFactory)
            => _shortCircuitFactories.TryFindMatch(mapperData, (md, f) => f.IsFor(md), out applicableFactory);

        #endregion

        protected override IEnumerable<Expression> GetObjectPopulation(MappingCreationContext context)
        {
            var expressionFactory = context.MapperData.UseMemberInitialisations()
                ? _memberInitPopulationFactory
                : _multiStatementPopulationFactory;

            return expressionFactory.GetPopulation(context);
        }
    }
}