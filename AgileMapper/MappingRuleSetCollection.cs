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
                NullValueProviderFactory.Instance,
                new IPopulationProcessor[] { });

            Merge = new MappingRuleSet(
                Constants.Merge,
                ComplexTypeMappingShortCircuitStrategy.SourceAndExistingAreNull,
                MergeEnumerablePopulationStrategy.Instance,
                NullValueProviderFactory.Instance,
                new[] { PopulatedMemberPopulationGuarder.Instance });

            Overwrite = new MappingRuleSet(
                Constants.Overwrite,
                ComplexTypeMappingShortCircuitStrategy.SourceIsNull,
                OverwriteEnumerablePopulationStrategy.Instance,
                OverwriteFallbackValueProviderFactory.Instance,
                new IPopulationProcessor[] { });
        }

        public MappingRuleSet CreateNew { get; }

        public MappingRuleSet Merge { get; }

        public MappingRuleSet Overwrite { get; set; }
    }
}