namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal class ConfiguredDataSource : SourceMemberDataSourceBase
    {
        public ConfiguredDataSource(
            Expression value,
            IMemberMappingContext context,
            Func<ParameterExpression, Expression> conditionFactory)
            : base(value, context, conditionFactory)
        {
        }
    }
}