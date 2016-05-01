namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;

    internal class ConfiguredDataSource : SourceMemberDataSourceBase
    {
        public ConfiguredDataSource(Expression value, Expression sourceObject)
            : base(value, sourceObject)
        {
        }
    }
}