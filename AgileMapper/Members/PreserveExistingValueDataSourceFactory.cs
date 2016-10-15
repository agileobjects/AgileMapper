namespace AgileObjects.AgileMapper.Members
{
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;

    internal class PreserveExistingValueDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new PreserveExistingValueDataSourceFactory();

        public IDataSource Create(MemberMapperData mapperData) => new PreserveExistingValueDataSource(mapperData);

        private class PreserveExistingValueDataSource : DataSourceBase
        {
            public PreserveExistingValueDataSource(MemberMapperData mapperData)
                : base(
                      mapperData.SourceMember,
                      mapperData.TargetMember.IsReadable
                          ? mapperData.TargetMember.GetAccess(mapperData.InstanceVariable)
                          : Constants.EmptyExpression,
                      mapperData)
            {
            }

            public override Expression GetValueOption(Expression valueSoFar)
                => Expression.Condition(Value.GetIsNotDefaultComparison(), Value, valueSoFar);
        }
    }
}