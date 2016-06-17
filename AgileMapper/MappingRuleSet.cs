namespace AgileObjects.AgileMapper
{
    using DataSources;
    using ObjectPopulation;

    internal class MappingRuleSet
    {
        public MappingRuleSet(
            string name,
            IComplexTypeMappingShortCircuitStrategy complexTypeMappingShortCircuitStrategy,
            IEnumerablePopulationStrategy enumerablePopulationStrategy,
            IDataSourceFactory initialDataSourceFactory,
            IDataSourceFactory fallbackDataSourceFactory)
        {
            Name = name;
            ComplexTypeMappingShortCircuitStrategy = complexTypeMappingShortCircuitStrategy;
            EnumerablePopulationStrategy = enumerablePopulationStrategy;
            InitialDataSourceFactory = initialDataSourceFactory;
            FallbackDataSourceFactory = fallbackDataSourceFactory;
        }

        public string Name { get; }

        public IComplexTypeMappingShortCircuitStrategy ComplexTypeMappingShortCircuitStrategy { get; }

        public IEnumerablePopulationStrategy EnumerablePopulationStrategy { get; }

        public IDataSourceFactory InitialDataSourceFactory { get; }

        public IDataSourceFactory FallbackDataSourceFactory { get; }
    }
}