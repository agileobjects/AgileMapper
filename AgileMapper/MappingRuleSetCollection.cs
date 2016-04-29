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
                CopySourceEnumerablePopulationStrategy.Instance,
                DefaultNullNestedSourceMemberStrategy.Instance,
                new[]
                {
                    NullNestedSourceMemberPopulationGuarder.Instance
                });

            Merge = new MappingRuleSet(
                Constants.Merge,
                CopySourceEnumerablePopulationStrategy.Instance,
                DefaultNullNestedSourceMemberStrategy.Instance,
                new[]
                {
                    PopulatedMemberPopulationGuarder.Instance,
                    NullNestedSourceMemberPopulationGuarder.Instance
                });

            Overwrite = new MappingRuleSet(
                Constants.Overwrite,
                CopySourceEnumerablePopulationStrategy.Instance,
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