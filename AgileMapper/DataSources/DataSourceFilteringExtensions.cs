namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Configuration.MemberIgnores.SourceValueFilters;
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
                    dataSource.IsFallback ? dataSources[i - 1].SourceMember : dataSource.SourceMember,
                    dataSource,
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
            => ApplyFilter(dataSource.SourceMember, dataSource, mapperData);

        private static IDataSource ApplyFilter(
            IQualifiedMember sourceMember,
            IDataSource dataSource,
            IMemberMapperData mapperData)
        {
            if (DoNotApplyFilter(sourceMember, dataSource, mapperData))
            {
                return dataSource;
            }

            var filters = mapperData.GetSourceValueFilters(sourceMember.Type);

            if (filters.None())
            {
                return dataSource;
            }

            var contextMapperData = mapperData.IsEntryPoint || (sourceMember != mapperData.SourceMember)
                ? mapperData
                : mapperData.Parent;

            var rawSourceValue = sourceMember
                .RelativeTo(contextMapperData.SourceMember)
                .GetQualifiedAccess(contextMapperData.SourceObject);

            var filterConditions = filters.GetFilterConditionsOrNull(rawSourceValue, contextMapperData);

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

        private static bool DoNotApplyFilter(
            IQualifiedMember sourceMember,
            IDataSource dataSource,
            IBasicMapperData mapperData)
        {
            if (!dataSource.IsValid)
            {
                return true;
            }

            // Non-simple enumerable elements will be filtered out elsewhere,
            // unless they're being runtime-typed:
            return !sourceMember.IsSimple && !mapperData.IsEntryPoint &&
                    mapperData.TargetMemberIsEnumerableElement();
        }

        public static Expression GetFilterConditionsOrNull(
            this IList<ConfiguredSourceValueFilter> filters,
            Expression sourceValue,
            IMemberMapperData mapperData)
        {
            return filters.HasOne()
                ? filters.First().GetConditionOrNull(sourceValue, mapperData)
                : filters
                    .ProjectToArray(
                        new { sourceValue, mapperData },
                       (d, filter) => filter.GetConditionOrNull(d.sourceValue, d.mapperData))
                    .AndTogether();
        }
    }
}