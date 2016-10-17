namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using DataSources;
    using Members;

    internal class ExistingOrDefaultValueDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new ExistingOrDefaultValueDataSourceFactory();

        public IDataSource Create(IMemberMapperData mapperData)
            => mapperData.TargetMember.IsReadable
                ? new ExistingMemberValueOrEmptyDataSource(mapperData)
                : DefaultValueDataSourceFactory.Instance.Create(mapperData);

        private class ExistingMemberValueOrEmptyDataSource : DataSourceBase
        {
            public ExistingMemberValueOrEmptyDataSource(IMemberMapperData mapperData)
                : base(mapperData.SourceMember, GetValue(mapperData), mapperData)
            {
            }

            private static Expression GetValue(IMemberMapperData mapperData)
            {
                var existingValue = mapperData.TargetMember.GetAccess(mapperData.InstanceVariable);

                if (!mapperData.TargetMember.IsEnumerable)
                {
                    return existingValue;
                }

                var emptyEnumerable = mapperData.TargetMember.GetEmptyInstanceCreation();

                return Expression.Coalesce(existingValue, emptyEnumerable);
            }
        }
    }
}