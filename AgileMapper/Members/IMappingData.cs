namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal interface IMappingData
    {
        IMappingData Parent { get; }

        string RuleSetName { get; }

        Type SourceType { get; }

        Type TargetType { get; }

        QualifiedMember TargetMember { get; }
    }
}