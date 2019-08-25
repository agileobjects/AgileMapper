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

    internal static class FilteredValueDataSource
    {
        public static IList<IDataSource> Create(
            IList<IDataSource> dataSources,
            IMemberMapperData mapperData)
        {
            var dataSourceCount = dataSources.Count;
            var filteredDataSources = new IDataSource[dataSourceCount];

            for (var i = 0; i < dataSourceCount; ++i)
            {
                var dataSource = dataSources[i];

                var filteredDataSource = Create(
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

        public static IDataSource Create(IDataSource dataSource, IMemberMapperData mapperData)
            => Create(dataSource, dataSource.SourceMember, mapperData);

        public static IDataSource Create(
            IDataSource dataSource,
            IQualifiedMember sourceMember,
            IMemberMapperData mapperData)
        {
            var filter = mapperData
                .MapperContext
                .UserConfigurations
                .GetSourceValueFilterOrNull(mapperData);

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

            filterCondition = Expression.Not(filterCondition);

            if (dataSource.IsConditional)
            {
                return new FilteredDataSourceWrapper(dataSource, filterCondition);
            }

            return new AdHocDataSource(sourceMember, dataSource.Value, filterCondition);
        }

        private class FilteredDataSourceWrapper : DataSourceBase
        {
            private readonly Expression _filterCondition;

            public FilteredDataSourceWrapper(IDataSource wrappedDataSource, Expression filterCondition)
                : base(wrappedDataSource, wrappedDataSource.Value)
            {
                _filterCondition = filterCondition;
            }

            public override Expression Finalise(Expression memberPopulation, Expression alternatePopulation)
            {
                return base.Finalise(
                    Expression.IfThen(_filterCondition, memberPopulation),
                    alternatePopulation);
            }
        }
    }
}