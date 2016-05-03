namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq.Expressions;
    using Api.Configuration;

    internal class ConfiguredDataSource : SourceMemberDataSourceBase
    {
        public ConfiguredDataSource(
            Expression value,
            Expression sourceObject,
            Func<IConfigurationContext, Expression> conditionFactory)
            : base(value, sourceObject, conditionFactory)
        {
        }
    }
}