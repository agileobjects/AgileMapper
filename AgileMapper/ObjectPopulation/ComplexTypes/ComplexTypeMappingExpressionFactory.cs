namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
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

        private ComplexTypeMappingExpressionFactory()
        {
            _memberInitPopulationFactory = new MemberInitPopulationExpressionFactory();
            _multiStatementPopulationFactory = new MultiStatementPopulationExpressionFactory();

            _shortCircuitFactories = new[]
            {
                SourceDictionaryShortCircuitFactory.Instance
            };
        }

        public override bool IsFor(IObjectMappingData mappingData) => true;

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

            if (targetType.IsAbstract() && DerivedTypesExistForTarget(mappingData))
            {
                return base.TargetCannotBeMapped(mappingData, out reason);
            }

            reason = "Cannot construct an instance of " + targetType.GetFriendlyName();
            return true;
        }

        private static bool DerivedTypesExistForTarget(IObjectMappingData mappingData)
        {
            var configuredImplementationTypePairs = mappingData
                .MapperData
                .MapperContext
                .UserConfigurations
                .DerivedTypes
                .GetImplementationTypePairsFor(mappingData.MapperData, mappingData.MapperData.MapperContext);

            return configuredImplementationTypePairs.Any() ||
                   mappingData.MapperData.GetDerivedTargetTypes().Any();
        }

        #region Short-Circuits

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (SourceObjectCouldBeNull(mapperData))
            {
                yield return Expression.IfThen(mapperData.SourceObject.GetIsDefaultComparison(), returnNull);
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
            => _shortCircuitFactories.TryFindMatch(f => f.IsFor(mapperData), out applicableFactory);

        #endregion

        protected override Expression GetDerivedTypeMappings(IObjectMappingData mappingData)
            => DerivedComplexTypeMappingsFactory.CreateFor(mappingData);

        protected override IEnumerable<Expression> GetObjectPopulation(MappingCreationContext context)
        {
            var expressionFactory = context.MapperData.UseMemberInitialisations()
                ? _memberInitPopulationFactory
                : _multiStatementPopulationFactory;

            return expressionFactory.GetPopulation(context);
        }
    }
}