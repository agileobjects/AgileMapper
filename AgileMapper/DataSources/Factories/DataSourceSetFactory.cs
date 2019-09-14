namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions.Internal;
    using MappingRoot;
    using ObjectPopulation;

    internal static class DataSourceSetFactory
    {
        private static readonly IMappingRootDataSourceFactory[] _mappingRootDataSourceFactories =
        {
            new QueryProjectionRootDataSourceFactory(),
            new EnumMappingRootDataSourceFactory(),
            new DictionaryMappingRootDataSourceFactory(),
            new EnumerableMappingRootDataSourceFactory(),
            new ComplexTypeMappingRootDataSourceFactory()
        };

        private static readonly DataSourcesFactory[] _childDataSourceFactories =
        {
            ConfiguredDataSourcesFactory.Create,
            MaptimeDataSourcesFactory.Create,
            SourceMemberDataSourcesFactory.Create,
            MetaMemberDataSourcesFactory.Create
        };

        public static IDataSourceSet CreateFor(IObjectMappingData rootMappingData)
        {
            var rootDataSourceFactory = _mappingRootDataSourceFactories
                .First(rootMappingData, (rmd, mef) => mef.IsFor(rmd));

            var rootDataSource = rootDataSourceFactory.CreateFor(rootMappingData);

            return DataSourceSet.For(rootDataSource, rootMappingData.MapperData);
        }

        public static IDataSourceSet CreateFor(DataSourceFindContext findContext)
        {
            var validDataSources = EnumerateDataSources(findContext).ToArray();

            return DataSourceSet.For(validDataSources, findContext.MemberMapperData);
        }

        private static IEnumerable<IDataSource> EnumerateDataSources(DataSourceFindContext context)
        {
            foreach (var factory in _childDataSourceFactories)
            {
                foreach (var dataSource in factory.Invoke(context))
                {
                    if (!dataSource.IsValid)
                    {
                        continue;
                    }

                    yield return dataSource;

                    if (!dataSource.IsConditional)
                    {
                        yield break;
                    }

                    ++context.DataSourceIndex;
                }

                if (context.StopFind)
                {
                    yield break;
                }
            }
        }
    }
}