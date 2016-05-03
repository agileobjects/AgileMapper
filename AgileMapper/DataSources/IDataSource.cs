namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Api.Configuration;

    internal interface IDataSource
    {
        Expression GetConditionOrNull(IConfigurationContext context);

        IEnumerable<Expression> NestedSourceMemberAccesses { get; }

        Expression Value { get; }
    }
}
