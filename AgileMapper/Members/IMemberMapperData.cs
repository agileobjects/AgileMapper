namespace AgileObjects.AgileMapper.Members
{
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal interface IMemberMapperData : IBasicMapperData
    {
        MapperContext MapperContext { get; }

        new ObjectMapperData Parent { get; }

        MapperDataContext Context { get; }

        Expression ParentObject { get; }

        ParameterExpression MappingDataObject { get; }

        IQualifiedMember SourceMember { get; }

        Expression SourceObject { get; }

        Expression TargetObject { get; }

        Expression CreatedObject { get; }

        Expression EnumerableIndex { get; }

        Expression TargetInstance { get; }

        NestedAccessFinder NestedAccessFinder { get; }
    }
}