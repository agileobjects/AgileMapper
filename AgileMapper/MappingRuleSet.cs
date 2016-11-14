namespace AgileObjects.AgileMapper
{
    using DataSources;
    using ObjectPopulation;

    internal class MappingRuleSet
    {
        public MappingRuleSet(
            string name,
            bool rootHasPopulatedTarget,
            IEnumerablePopulationStrategy enumerablePopulationStrategy,
            IDataSourceFactory initialDataSourceFactory,
            IDataSourceFactory fallbackDataSourceFactory)
        {
            Name = name;
            RootHasPopulatedTarget = rootHasPopulatedTarget;
            EnumerablePopulationStrategy = enumerablePopulationStrategy;
            InitialDataSourceFactory = initialDataSourceFactory;
            FallbackDataSourceFactory = fallbackDataSourceFactory;
        }

        public string Name { get; }

        public bool RootHasPopulatedTarget { get; }

        public IEnumerablePopulationStrategy EnumerablePopulationStrategy { get; }

        public IDataSourceFactory InitialDataSourceFactory { get; }

        public IDataSourceFactory FallbackDataSourceFactory { get; }
    }
}