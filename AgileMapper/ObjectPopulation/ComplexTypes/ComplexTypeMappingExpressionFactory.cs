namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System.Collections.Generic;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
    using ShortCircuits;

    internal class ComplexTypeMappingExpressionFactory : MappingExpressionFactoryBase
    {
        public static readonly MappingExpressionFactoryBase Instance = new ComplexTypeMappingExpressionFactory();

        private readonly PopulationExpressionFactoryBase _memberInitPopulationFactory;
        private readonly PopulationExpressionFactoryBase _multiStatementPopulationFactory;

        private ComplexTypeMappingExpressionFactory()
        {
            _memberInitPopulationFactory = new MemberInitPopulationExpressionFactory();
            _multiStatementPopulationFactory = new MultiStatementPopulationExpressionFactory();
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

        protected override IEnumerable<AlternateMappingFactory> AlternateMappingFactories
        {
            get
            {
                foreach (var mappingFactory in base.AlternateMappingFactories)
                {
                    yield return mappingFactory;
                }

                yield return DerivedComplexTypeMappingFactory.GetMappingOrNull;
            }
        }

        protected override IEnumerable<ShortCircuitFactory> ShortCircuitFactories
        {
            get
            {
                yield return NullSourceShortCircuitFactory.GetShortCircuitOrNull;
                yield return AlreadyMappedObjectShortCircuitFactory.GetShortCircuitOrNull;
                yield return SourceDictionaryShortCircuitFactory.GetShortCircuitOrNull;
            }
        }

        #endregion

        protected override void AddObjectPopulation(MappingCreationContext context)
        {
            var expressionFactory = context.MapperData.UseMemberInitialisations()
                ? _memberInitPopulationFactory
                : _multiStatementPopulationFactory;

            expressionFactory.AddPopulation(context);
        }
    }
}