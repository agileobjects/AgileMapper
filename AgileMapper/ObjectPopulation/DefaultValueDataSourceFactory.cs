namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;
    using Members;

    internal class DefaultValueDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new DefaultValueDataSourceFactory();

        public IDataSource Create(IMemberMapperData mapperData)
            => new DefaultValueDataSource(mapperData);

        private class DefaultValueDataSource : DataSourceBase
        {
            public DefaultValueDataSource(IMemberMapperData mapperData)
                : base(mapperData.SourceMember, mapperData.TargetMember.Type.ToDefaultExpression())
            {
            }
        }
    }
}