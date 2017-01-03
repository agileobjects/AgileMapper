namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;

    internal class NullDataSource : DataSourceBase
    {
        public NullDataSource(Expression value)
            : base(null, value)
        {
        }

        public override bool IsValid => false;
    }
}