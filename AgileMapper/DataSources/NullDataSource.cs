namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;

    internal class NullDataSource : DataSourceBase
    {
        public NullDataSource(Expression value)
            : base(default(IQualifiedMember), value)
        {
        }

        public override bool IsValid => false;
    }
}