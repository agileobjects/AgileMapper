namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using Members;

    internal interface IConfigurationContext
    {
        IConfigurationContext Parent { get; }

        string RuleSetName { get; }

        QualifiedMember TargetMember { get; }

        Type SourceObjectType { get; }

        Type ExistingObjectType { get; }
    }
}