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
            IMemberPopulationGuardFactory populationGuardFactory,
            IDataSourceFactory fallbackDataSourceFactory)
        {
            Name = name;
            Settings = settings;
            EnumerablePopulationStrategy = enumerablePopulationStrategy;
            RecursiveMemberMappingStrategy = recursiveMemberMappingStrategy;
            PopulationGuardFactory = populationGuardFactory;
            FallbackDataSourceFactory = fallbackDataSourceFactory;
        }

        public string Name { get; }

        public MappingRuleSetSettings Settings { get; }

        public IEnumerablePopulationStrategy EnumerablePopulationStrategy { get; }

        public IRecursiveMemberMappingStrategy RecursiveMemberMappingStrategy { get; }

        public IMemberPopulationGuardFactory PopulationGuardFactory { get; }

        public IDataSourceFactory FallbackDataSourceFactory { get; }
    }
}