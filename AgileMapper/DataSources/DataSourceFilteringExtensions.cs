namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Configuration;
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
            if (!dataSource.IsValid)
            {
                return dataSource;
            }

            var filters = mapperData.GetSourceValueFilters(sourceMember.Type);

            if (filters.None())
            {
                return dataSource;
            }

            var rawSourceValue = (sourceMember != mapperData.SourceMember)
                ? sourceMember
                    .RelativeTo(mapperData.SourceMember)
                    .GetQualifiedAccess(mapperData.SourceObject)
                : mapperData.SourceObject;

            var filterConditions = filters.GetFilterConditionsOrNull(rawSourceValue);

            if (filterConditions == null)
            {
                return dataSource;
            }

            if (dataSource.IsConditional)
            {
                filterConditions = Expression.AndAlso(dataSource.Condition, filterConditions);
            }

            return new AdHocDataSource(sourceMember, dataSource.Value, filterConditions);
        }

        public static Expression GetFilterConditionsOrNull(
            this IList<ConfiguredSourceValueFilter> filters,
            Expression sourceValue)
        {
            return filters.HasOne()
                ? filters.First().GetConditionOrNull(sourceValue)
                : filters
                    .ProjectToArray(sourceValue, (sv, filter) => filter.GetConditionOrNull(sv))
                    .AndTogether();
        }
    }
}