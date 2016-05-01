namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal interface IDataSource
    {
        IEnumerable<Expression> NestedSourceMemberAccesses { get; }

        Expression Value { get; }
    }
}
