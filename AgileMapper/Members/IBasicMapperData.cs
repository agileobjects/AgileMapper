namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal interface IBasicMapperData
    {
        MappingRuleSet RuleSet { get; }

        bool IsRoot { get; }

        IBasicMapperData Parent { get; }

        Type SourceType { get; }

        Type TargetType { get; }

        QualifiedMember TargetMember { get; }
    }
}