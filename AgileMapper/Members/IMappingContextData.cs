namespace AgileObjects.AgileMapper.Members
{
    internal interface IMappingContextData
    {
        MappingRuleSet RuleSet { get; }

        IQualifiedMember SourceMember { get; }

        QualifiedMember TargetMember { get; }
    }
}