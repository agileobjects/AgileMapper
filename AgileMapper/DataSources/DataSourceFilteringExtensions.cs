namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;

    internal static class DataSourceFilteringExtensions
    {
        public static IList<IDataSource> WithFilters(
            this IList<IDataSource> dataSources,
            IMemberMapperData mapperData)
        {
            var dataSourceCount = dataSources.Count;
            var filteredDataSources = new IDataSource[dataSourceCount];

            for (var i = 0; i < dataSourceCount; ++i)
            {
                var dataSource = dataSources[i];

                var filteredDataSource = filteredDataSources[i] = ApplyFilter(
                    dataSource,
                    dataSource.IsFallback ? dataSources[i - 1].SourceMember : dataSource.SourceMember,
                    mapperData);

                if (!dataSource.IsFallback)
                {
                    continue;
                }

                if (filteredDataSource != null)
                {
                    break;
                }

                var filteredDataSourcesWithoutFallback = new IDataSource[dataSourceCount - 1];

                filteredDataSourcesWithoutFallback.CopyFrom(filteredDataSources);

                return filteredDataSourcesWithoutFallback;
            }

            return filteredDataSources;
        }

        public static IDataSource WithFilter(this IDataSource dataSource, IMemberMapperData mapperData)
            => ApplyFilter(dataSource, dataSource.SourceMember, mapperData);

        private static IDataSource ApplyFilter(
            IDataSource dataSource,
            IQualifiedMember sourceMember,
            IMemberMapperData mapperData)
        {
            var filter = mapperData.GetSourceValueFilterOrNull();

            if (filter == null)
            {
                return dataSource;
            }

            var rawSourceValue = sourceMember
                .RelativeTo(mapperData.SourceMember)
                .GetQualifiedAccess(mapperData.SourceObject);

            var filterCondition = filter.GetConditionOrNull(rawSourceValue);

            if (filterCondition == null)
            {
                return dataSource;
            }

            if (dataSource.IsConditional)
            {
                filterCondition = Expression.AndAlso(dataSource.Condition, filterCondition);
            }

            return new AdHocDataSource(sourceMember, dataSource.Value, filterCondition);
        }
    }
}