namespace AgileObjects.AgileMapper.Members
{
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;

    internal class PreserveExistingValueDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new PreserveExistingValueDataSourceFactory();

        public IDataSource Create(IChildMemberMappingData mappingData)
            => new PreserveExistingValueDataSource(mappingData.MapperData);

        private class PreserveExistingValueDataSource : DataSourceBase
        {
            public PreserveExistingValueDataSource(IMemberMapperData mapperData)
                : this(
                      mapperData.SourceMember,
                      mapperData.TargetMember.IsReadable
                          ? mapperData.GetTargetMemberAccess()
                          : Constants.EmptyExpression)
            {
            }

            private PreserveExistingValueDataSource(
                IQualifiedMember sourceMember,
                Expression value)
                : base(
                      sourceMember,
                      Enumerable.Empty<ParameterExpression>(),
                      value,
                      (value != Constants.EmptyExpression)
                        ? value.GetIsNotDefaultComparison()
                        : Constants.EmptyExpression)
            {
            }
        }
    }
}