namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;

    internal class AdHocDataSource : DataSourceBase
    {
        public AdHocDataSource(IDataSource wrappedDataSource, Expression value)
            : base(
                wrappedDataSource.SourceMember,
                wrappedDataSource.Variables,
                value,
                wrappedDataSource.Condition)
        {
        }
    }
}