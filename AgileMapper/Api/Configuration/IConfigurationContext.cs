namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal interface IConfigurationContext
    {
        IConfigurationContext Parent { get; }

        string RuleSetName { get; }

        QualifiedMember TargetMember { get; }

        Expression SourceObject { get; }

        Type SourceObjectType { get; }

        Expression ExistingObject { get; }

        Type ExistingObjectType { get; }

        Expression TargetVariable { get; }
    }
}