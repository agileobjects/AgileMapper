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
                DefaultNullNestedSourceMemberStrategy.Instance,
                new[]
                {
                    NullNestedSourceMemberPopulationGuarder.Instance
                });

            Merge = new MappingRuleSet(
                Constants.Merge,
                ComplexTypeMappingShortCircuitStrategy.SourceAndExistingAreNull,
                CopySourceEnumerablePopulationStrategy.Instance,
                DefaultNullNestedSourceMemberStrategy.Instance,
                new[]
                {
                    PopulatedMemberPopulationGuarder.Instance,
                    NullNestedSourceMemberPopulationGuarder.Instance
                });

            Overwrite = new MappingRuleSet(
                Constants.Overwrite,
                ComplexTypeMappingShortCircuitStrategy.SourceIsNull,
                OverwriteEnumerablePopulationStrategy.Instance,
                OverwriteNullNestedSourceMemberStrategy.Instance,
                new[]
                {
                    NullNestedSourceMemberPopulationGuarder.Instance
                });
        }

        public MappingRuleSet CreateNew { get; }

        public MappingRuleSet Merge { get; }

        public MappingRuleSet Overwrite { get; set; }
    }
}