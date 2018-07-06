namespace AgileObjects.AgileMapper.Members
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using ObjectPopulation;

    internal interface IMemberMapperData : IBasicMapperData
    {
        MapperContext MapperContext { get; }

        bool IsEntryPoint { get; }

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

        ExpressionInfoFinder ExpressionInfoFinder { get; }
    }
}