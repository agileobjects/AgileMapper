namespace AgileObjects.AgileMapper.Members
{
    internal interface IBasicMapperData : ITypePair
    {
        MappingRuleSet RuleSet { get; }

        bool IsRoot { get; }

        IBasicMapperData Parent { get; }

        IQualifiedMember SourceMember { get; }
        
        QualifiedMember TargetMember { get; }

        bool HasCompatibleTypes(ITypePair typePair);
    }
}