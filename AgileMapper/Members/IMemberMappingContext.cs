namespace AgileObjects.AgileMapper.Members
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using DataSources;
    using ObjectPopulation;

    internal interface IMemberMappingContext
    {
        MapperContext MapperContext { get; }

        MappingContext MappingContext { get; }

        IObjectMappingContext Parent { get; }

        ParameterExpression Parameter { get; }

        string RuleSetName { get; }

        IQualifiedMember SourceMember { get; }

        Expression SourceObject { get; }

        int SourceObjectDepth { get; }

        IQualifiedMember TargetMember { get; }

        Expression ExistingObject { get; }

        Expression EnumerableIndex { get; }

        ParameterExpression InstanceVariable { get; }

        NestedAccessFinder NestedAccessFinder { get; }

        IEnumerable<IDataSource> GetDataSources();

        Expression WrapInTry(Expression expression);
    }
}