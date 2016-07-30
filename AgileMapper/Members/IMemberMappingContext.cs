namespace AgileObjects.AgileMapper.Members
{
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal interface IMemberMappingContext : IMappingData
    {
        MapperContext MapperContext { get; }

        MappingContext MappingContext { get; }

        new IObjectMappingContext Parent { get; }

        ParameterExpression Parameter { get; }

        IQualifiedMember SourceMember { get; }

        Expression SourceObject { get; }

        Expression TargetObject { get; }

        Expression EnumerableIndex { get; }

        ParameterExpression InstanceVariable { get; }

        NestedAccessFinder NestedAccessFinder { get; }
    }
}