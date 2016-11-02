namespace AgileObjects.AgileMapper.Members
{
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;

    internal class PreserveExistingValueDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new PreserveExistingValueDataSourceFactory();

        public IDataSource Create(IMemberMappingData mappingData)
            => new PreserveExistingValueDataSource(mappingData.MapperData);

        private class PreserveExistingValueDataSource : DataSourceBase
        {
            public PreserveExistingValueDataSource(IMemberMapperData mapperData)
                : base(
                      mapperData.SourceMember,
                      mapperData.TargetMember.IsReadable
                          ? mapperData.GetTargetMemberAccess()
                          : Constants.EmptyExpression,
                      mapperData)
            {
            }

            public override Expression GetValueOption(Expression valueSoFar)
                => Expression.Condition(Value.GetIsNotDefaultComparison(), Value, valueSoFar);
        }
    }
}