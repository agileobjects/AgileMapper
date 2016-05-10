namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal interface IDataSource
    {
        Expression GetConditionOrNull(ParameterExpression contextParameter);

        IEnumerable<Expression> NestedSourceMemberAccesses { get; }

        Expression Value { get; }
    }
}
