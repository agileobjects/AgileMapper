namespace AgileObjects.AgileMapper
{
    using PopulationProcessing;

    internal class MappingRuleSetCollection
    {
        public MappingRuleSetCollection()
        {
            CreateNew = new MappingRuleSet(
                Constants.CreateNew,
                DefaultNullNestedSourceMemberStrategy.Instance,
                new[]
                {
                    NullNestedSourceMemberPopulationGuarder.Instance
                });

            Merge = new MappingRuleSet(
                Constants.Merge,
                DefaultNullNestedSourceMemberStrategy.Instance,
                new[]
                {
                    PopulatedMemberPopulationGuarder.Instance,
                    NullNestedSourceMemberPopulationGuarder.Instance
                });
        }

        public MappingRuleSet CreateNew { get; }

        public MappingRuleSet Merge { get; }
    }
}