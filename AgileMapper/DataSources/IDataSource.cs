namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal interface IDataSource
    {
        IEnumerable<ValueProvider> GetValueProviders(IMemberMappingContext context);

        Expression GetConditionOrNull(IMemberMappingContext context);

        IEnumerable<Expression> NestedAccesses { get; }

        Expression Value { get; }
    }
}
