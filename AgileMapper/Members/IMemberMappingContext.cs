namespace AgileObjects.AgileMapper.Members
{
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal interface IMemberMappingContext
    {
        IObjectMappingContext Parent { get; }

        ParameterExpression Parameter { get; }

        string RuleSetName { get; }

        QualifiedMember TargetMember { get; }

        Expression SourceObject { get; }

        Expression ExistingObject { get; }

        Expression EnumerableIndex { get; }

        ParameterExpression InstanceVariable { get; }

        NestedAccessFinder NestedAccessFinder { get; }
    }
}