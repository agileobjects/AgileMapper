namespace AgileObjects.AgileMapper.Members
{
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;

    internal class PreserveExistingValueDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new PreserveExistingValueDataSourceFactory();

        public IDataSource Create(MemberMapperData data) => new PreserveExistingValueDataSource(data);

        private class PreserveExistingValueDataSource : DataSourceBase
        {
            public PreserveExistingValueDataSource(MemberMapperData data)
                : base(
                      data.SourceMember,
                      data.TargetMember.IsReadable
                          ? data.TargetMember.GetAccess(data.InstanceVariable)
                          : Constants.EmptyExpression,
                      data)
            {
            }

            public override Expression GetValueOption(Expression valueSoFar)
                => Expression.Condition(Value.GetIsNotDefaultComparison(), Value, valueSoFar);
        }
    }
}