namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;

    internal interface IDataSource
    {
        IQualifiedMember SourceMember { get; }

        bool IsSuccessful { get; }

        IEnumerable<ParameterExpression> Variables { get; }

        IEnumerable<Expression> NestedAccesses { get; }

        Expression Value { get; }

        Expression GetIfGuardedPopulation(IMemberMappingContext context);

        Expression GetElseGuardedPopulation(Expression populationSoFar, IMemberMappingContext context);
    }
}
