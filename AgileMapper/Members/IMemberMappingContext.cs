namespace AgileObjects.AgileMapper.Members
{
    using System.Linq.Expressions;

    internal interface IMemberMappingContext
    {
        IMemberMappingContext Parent { get; }

        string RuleSetName { get; }

        QualifiedMember TargetMember { get; }

        Expression SourceObject { get; }

        Expression ExistingObject { get; }

        Expression EnumerableIndex { get; }

        ParameterExpression TargetVariable { get; }

        NestedAccessFinder NestedAccessFinder { get; }
    }
}