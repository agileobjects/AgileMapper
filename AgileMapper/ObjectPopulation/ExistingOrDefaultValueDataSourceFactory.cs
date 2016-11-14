namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;
    using Members;

    internal class ExistingOrDefaultValueDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new ExistingOrDefaultValueDataSourceFactory();

        public IDataSource Create(IMemberMappingData mappingData)
            => mappingData.MapperData.TargetMember.IsReadable
                ? new ExistingMemberValueOrEmptyDataSource(mappingData.MapperData)
                : DefaultValueDataSourceFactory.Instance.Create(mappingData);

        private class ExistingMemberValueOrEmptyDataSource : DataSourceBase
        {
            public ExistingMemberValueOrEmptyDataSource(IMemberMapperData mapperData)
                : base(mapperData.SourceMember, GetValue(mapperData), mapperData)
            {
            }

            private static Expression GetValue(IMemberMapperData mapperData)
            {
                var existingValue = mapperData.GetTargetMemberAccess();

                if (!mapperData.TargetMember.IsEnumerable)
                {
                    return existingValue;
                }

                var existingValueNotNull = existingValue.GetIsNotDefaultComparison();
                var emptyEnumerable = mapperData.TargetMember.GetEmptyInstanceCreation();

                return Expression.Condition(
                    existingValueNotNull,
                    existingValue,
                    emptyEnumerable,
                    existingValue.Type);
            }
        }
    }
}