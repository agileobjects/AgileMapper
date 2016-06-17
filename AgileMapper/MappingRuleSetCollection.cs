namespace AgileObjects.AgileMapper
{
    using DataSources;
    using Members;
    using ObjectPopulation;

    internal class MappingRuleSetCollection
    {
        public MappingRuleSetCollection()
        {
            CreateNew = new MappingRuleSet(
                Constants.CreateNew,
                ComplexTypeMappingShortCircuitStrategy.SourceIsNull,
                CopySourceEnumerablePopulationStrategy.Instance,
                NullDataSourceFactory.Instance,
                ExistingOrDefaultValueDataSourceFactory.Instance);

            Merge = new MappingRuleSet(
                Constants.Merge,
                ComplexTypeMappingShortCircuitStrategy.SourceAndExistingAreNull,
                MergeEnumerablePopulationStrategy.Instance,
                PreserveExistingValueDataSourceFactory.Instance,
                ExistingOrDefaultValueDataSourceFactory.Instance);

            Overwrite = new MappingRuleSet(
                Constants.Overwrite,
                ComplexTypeMappingShortCircuitStrategy.SourceIsNull,
                OverwriteEnumerablePopulationStrategy.Instance,
                NullDataSourceFactory.Instance,
                DefaultValueDataSourceFactory.Instance);
        }

        public MappingRuleSet CreateNew { get; }

        public MappingRuleSet Merge { get; }

        public MappingRuleSet Overwrite { get; set; }
    }
}