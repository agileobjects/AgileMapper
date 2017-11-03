namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Extensions;
    using Members;
    using NetStandardPolyfills;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

    internal class ComplexTypeMappingExpressionFactory : MappingExpressionFactoryBase
    {
        private readonly ComplexTypeConstructionFactory _constructionFactory;
        private readonly PopulationExpressionFactoryBase _structPopulationFactory;
        private readonly PopulationExpressionFactoryBase _classPopulationFactory;
        private readonly IEnumerable<ISourceShortCircuitFactory> _shortCircuitFactories;

        public ComplexTypeMappingExpressionFactory(MapperContext mapperContext)
        {
            _constructionFactory = new ComplexTypeConstructionFactory(mapperContext);
            _structPopulationFactory = new StructPopulationExpressionFactory(_constructionFactory);
            _classPopulationFactory = new ClassPopulationExpressionFactory(_constructionFactory);

            _shortCircuitFactories = new[]
            {
                SourceDictionaryShortCircuitFactory.Instance
            };
        }

        public override bool IsFor(IObjectMappingData mappingData) => true;

        protected override bool TargetCannotBeMapped(IObjectMappingData mappingData, out Expression nullMappingBlock)
        {
            if (mappingData.MapperData.TargetCouldBePopulated())
            {
                // If a target complex type is readonly or unconstructable 
                // we still try to map to it using an existing non-null value:
                return base.TargetCannotBeMapped(mappingData, out nullMappingBlock);
            }

            if (_constructionFactory.GetNewObjectCreation(mappingData) != null)
            {
                return base.TargetCannotBeMapped(mappingData, out nullMappingBlock);
            }

            var targetType = mappingData.MapperData.TargetType;

            nullMappingBlock = Expression.Block(
                ReadableExpression.Comment("Cannot construct an instance of " + targetType.GetFriendlyName()),
                targetType.ToDefaultExpression());

            return true;
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

            if (mapperData.TargetMemberIsEnumerableElement())
            {
                return !mapperData.HasSameSourceAsParent();
            }

            return false;
        }

        private static Expression GetAlreadyMappedObjectShortCircuitOrNull(ObjectMapperData mapperData)
        {
            if (!mapperData.CacheMappedObjects || mapperData.TargetTypeHasNotYetBeenMapped)
            {
                return null;
            }

            // ReSharper disable once PossibleNullReferenceException
            var tryGetMethod = typeof(IObjectMappingDataUntyped).GetMethod("TryGet")
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
        {
            applicableFactory = _shortCircuitFactories.FirstOrDefault(f => f.IsFor(mapperData));
            return applicableFactory != null;
        }

        #endregion

        protected override Expression GetDerivedTypeMappings(IObjectMappingData mappingData)
            => DerivedComplexTypeMappingsFactory.CreateFor(mappingData);

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData)
        {
            var expressionFactory = mappingData.MapperData.TargetMemberIsUserStruct()
                ? _structPopulationFactory
                : _classPopulationFactory;

            return expressionFactory.GetPopulation(mappingData);
        }

        protected override Expression GetReturnValue(ObjectMapperData mapperData) => mapperData.TargetInstance;

        public override void Reset() => _constructionFactory.Reset();
    }
}