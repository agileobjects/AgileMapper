namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;

    internal class AdHocDataSource : DataSourceBase
    {
        public AdHocDataSource(IQualifiedMember targetMember, Expression value)
            : base(targetMember, value)
        {
        }
    }
}