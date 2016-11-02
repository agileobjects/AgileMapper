namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using DataSources;
    using Members;

    internal class DefaultValueDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new DefaultValueDataSourceFactory();

        public IDataSource Create(IMemberMappingData mappingData)
            => new DefaultValueDataSource(mappingData.MapperData);

        private class DefaultValueDataSource : DataSourceBase
        {
            public DefaultValueDataSource(IMemberMapperData mapperData)
                : base(mapperData.SourceMember, Expression.Default(mapperData.TargetMember.Type))
            {
            }
        }
    }
}