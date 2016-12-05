namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;
    using Members;

    internal class ExistingOrDefaultValueDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new ExistingOrDefaultValueDataSourceFactory();

        public IDataSource Create(IChildMemberMappingData mappingData)
            => new ExistingMemberValueOrEmptyDataSource(mappingData.MapperData);

        private class ExistingMemberValueOrEmptyDataSource : DataSourceBase
        {
            public ExistingMemberValueOrEmptyDataSource(IMemberMapperData mapperData)
                : base(mapperData.SourceMember, GetValue(mapperData), mapperData)
            {
            }

            private static Expression GetValue(IMemberMapperData mapperData)
            {
                if (mapperData.TargetMember.IsEnumerable)
                {
                    return mapperData.GetFallbackCollectionValue();
                }

                if (mapperData.TargetMember.IsReadable)
                {
                    return mapperData.GetTargetMemberAccess();
                }

                return Expression.Default(mapperData.TargetMember.Type);
            }
        }
    }
}