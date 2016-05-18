namespace AgileObjects.AgileMapper
{
    using DataSources;
    using ObjectPopulation;
    using PopulationProcessing;

    internal class MappingRuleSet
    {
        public MappingRuleSet(
            string name,
            IComplexTypeMappingShortCircuitStrategy complexTypeMappingShortCircuitStrategy,
            IEnumerablePopulationStrategy enumerablePopulationStrategy,
            IDataSourceFactory fallbackDataSourceFactory,
            IPopulationProcessor memberPopulationProcessor)
        {
            Name = name;
            ComplexTypeMappingShortCircuitStrategy = complexTypeMappingShortCircuitStrategy;
            EnumerablePopulationStrategy = enumerablePopulationStrategy;
            FallbackDataSourceFactory = fallbackDataSourceFactory;
            MemberPopulationProcessor = memberPopulationProcessor;
        }

        public string Name { get; }

        public IComplexTypeMappingShortCircuitStrategy ComplexTypeMappingShortCircuitStrategy { get; }

        public IEnumerablePopulationStrategy EnumerablePopulationStrategy { get; }

        public IDataSourceFactory FallbackDataSourceFactory { get; }

        public IPopulationProcessor MemberPopulationProcessor { get; }
    }
}