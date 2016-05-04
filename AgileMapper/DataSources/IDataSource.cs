namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;

    internal interface IDataSource
    {
        Expression GetConditionOrNull(IMemberMappingContext context);

        IEnumerable<Expression> NestedSourceMemberAccesses { get; }

        Expression Value { get; }
    }
}
