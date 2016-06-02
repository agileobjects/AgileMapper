namespace AgileObjects.AgileMapper
{
    using ObjectPopulation;
    using PopulationProcessing;

    internal class MappingRuleSetCollection
    {
        public MappingRuleSetCollection()
        {
            CreateNew = new MappingRuleSet(
                Constants.CreateNew,
                ComplexTypeMappingShortCircuitStrategy.SourceIsNull,
                CopySourceEnumerablePopulationStrategy.Instance,
                ExistingOrDefaultValueDataSourceFactory.Instance,
                NullPopulationProcessor.Instance);

            Merge = new MappingRuleSet(
                Constants.Merge,
                ComplexTypeMappingShortCircuitStrategy.SourceAndExistingAreNull,
                MergeEnumerablePopulationStrategy.Instance,
                ExistingOrDefaultValueDataSourceFactory.Instance,
                PopulatedMemberPopulationGuarder.Instance);

            Overwrite = new MappingRuleSet(
                Constants.Overwrite,
                ComplexTypeMappingShortCircuitStrategy.SourceIsNull,
                OverwriteEnumerablePopulationStrategy.Instance,
                DefaultValueDataSourceFactory.Instance,
                NullPopulationProcessor.Instance);
        }

        public MappingRuleSet CreateNew { get; }

        public MappingRuleSet Merge { get; }

        public MappingRuleSet Overwrite { get; set; }
    }
}