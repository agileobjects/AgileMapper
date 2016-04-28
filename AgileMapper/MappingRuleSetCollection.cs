namespace AgileObjects.AgileMapper
{
    using PopulationProcessing;

    internal class MappingRuleSetCollection
    {
        public MappingRuleSetCollection()
        {
            CreateNew = new MappingRuleSet(
                "CreateNew",
                DefaultNullNestedSourceMemberStrategy.Instance,
                new[]
                {
                    NullNestedSourceMemberPopulationGuarder.Instance
                });
        }

        public MappingRuleSet CreateNew { get; }
    }
}