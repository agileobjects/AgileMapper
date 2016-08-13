namespace AgileObjects.AgileMapper.Members
{
    internal interface IMapperCreationData
    {
        MappingRuleSet RuleSet { get; }

        IQualifiedMember SourceMember { get; }

        QualifiedMember TargetMember { get; }
    }
}