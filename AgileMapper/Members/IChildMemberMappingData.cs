namespace AgileObjects.AgileMapper.Members
{
    using ObjectPopulation;

    internal interface IChildMemberMappingData
    {
        MappingRuleSet RuleSet { get; }

        IObjectMappingData Parent { get; }

        IMemberMapperData MapperData { get; }
    }
}