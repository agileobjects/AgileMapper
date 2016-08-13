namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal interface IDataSource
    {
        IQualifiedMember SourceMember { get; }

        bool IsValid { get; }

        bool IsConditional { get; }

        Expression Condition { get; }

        IEnumerable<ParameterExpression> Variables { get; }

        IEnumerable<IObjectMapper> InlineObjectMappers { get; }

        Expression Value { get; }

        Expression GetValueOption(Expression valueSoFar);
    }
}
