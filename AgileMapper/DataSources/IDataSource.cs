namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;

    internal interface IDataSource
    {
        IQualifiedMember SourceMember { get; }

        bool IsValid { get; }

        bool IsConditional { get; }

        Expression Condition { get; }

        IEnumerable<ParameterExpression> Variables { get; }

        Expression Value { get; }

        Expression GetValueOption(Expression valueSoFar);
    }
}
