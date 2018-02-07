namespace AgileObjects.AgileMapper
{
    using DataSources;
    using Members.Population;
    using ObjectPopulation.Enumerables;
    using ObjectPopulation.Recursion;

    internal class MappingRuleSet
    {
        public MappingRuleSet(
            string name,
            MappingRuleSetSettings settings,
            IEnumerablePopulationStrategy enumerablePopulationStrategy,
            IRecursiveMemberMappingStrategy recursiveMemberMappingStrategy,
            IMemberPopulationFactory populationFactory,
            IDataSourceFactory fallbackDataSourceFactory)
        {
            Name = name;
            Settings = settings;
            EnumerablePopulationStrategy = enumerablePopulationStrategy;
            RecursiveMemberMappingStrategy = recursiveMemberMappingStrategy;
            PopulationFactory = populationFactory;
            FallbackDataSourceFactory = fallbackDataSourceFactory;
        }

        public string Name { get; }

        public MappingRuleSetSettings Settings { get; }

        public IEnumerablePopulationStrategy EnumerablePopulationStrategy { get; }

        public IRecursiveMemberMappingStrategy RecursiveMemberMappingStrategy { get; }

        public IMemberPopulationFactory PopulationFactory { get; }

        public IDataSourceFactory FallbackDataSourceFactory { get; }
    }
}