namespace AgileObjects.AgileMapper
{
    using DataSources;
    using Members.Population;
    using ObjectPopulation.Enumerables;

    internal class MappingRuleSetSettings
    {
        public bool RootHasPopulatedTarget { get; set; }

        public bool SourceElementsCouldBeNull { get; set; }

        public bool UseTryCatch { get; set; }
    }

    internal class MappingRuleSet
    {
        public MappingRuleSet(
            string name,
            MappingRuleSetSettings settings,
            IEnumerablePopulationStrategy enumerablePopulationStrategy,
            IMemberPopulationGuardFactory populationGuardFactory,
            IDataSourceFactory fallbackDataSourceFactory)
        {
            Name = name;
            Settings = settings;
            EnumerablePopulationStrategy = enumerablePopulationStrategy;
            PopulationGuardFactory = populationGuardFactory;
            FallbackDataSourceFactory = fallbackDataSourceFactory;
        }

        public string Name { get; }

        public MappingRuleSetSettings Settings { get; }

        public IEnumerablePopulationStrategy EnumerablePopulationStrategy { get; }

        public IMemberPopulationGuardFactory PopulationGuardFactory { get; }

        public IDataSourceFactory FallbackDataSourceFactory { get; }
    }
}