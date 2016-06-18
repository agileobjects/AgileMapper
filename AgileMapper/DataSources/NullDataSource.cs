namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq;
    using System.Linq.Expressions;

    internal class NullDataSource : DataSourceBase
    {
        public static readonly IDataSource Default = new NullDataSource(Constants.EmptyExpression);

        public NullDataSource(Expression value)
            : base(null, Enumerable.Empty<Expression>(), Enumerable.Empty<ParameterExpression>(), value)
        {
        }

        public override bool IsValid => false;
    }
}