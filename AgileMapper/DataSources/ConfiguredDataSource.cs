namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;

    internal class ConfiguredDataSource : DataSourceBase
    {
        public ConfiguredDataSource(Expression value, IMemberMappingContext context, Expression condition)
            : base(new ConfiguredQualifiedMember(value), value, context, condition)
        {
        }
    }
}