namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;
    using System.Linq;
    using MappingRoot;
    using Members;
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

        private static readonly DataSourceFactory[] _childDataSourceFactories =
        {
            ConfiguredDataSourceFactory.Create,
            MaptimeDataSourceFactory.Create,
            SourceMemberDataSourceFactory.Create,
            MetaMemberDataSourceFactory.Create
        };

        public static DataSourceSet CreateFor(IObjectMappingData rootMappingData)
        {
            var rootDataSourceFactory = _mappingRootDataSourceFactories
                .First(mef => mef.IsFor(rootMappingData));

            var rootDataSource = rootDataSourceFactory.CreateFor(rootMappingData);

            return new DataSourceSet(rootMappingData.MapperData, rootDataSource);
        }

        public static DataSourceSet CreateFor(IChildMemberMappingData childMappingData)
        {
            var findContext = new DataSourceFindContext(childMappingData);
            var validDataSources = EnumerateDataSources(findContext).ToArray();

            return new DataSourceSet(findContext.MapperData, validDataSources);
        }

        private static IEnumerable<IDataSource> EnumerateDataSources(DataSourceFindContext context)
        {
            foreach (var finder in _childDataSourceFactories)
            {
                foreach (var dataSource in finder.Invoke(context))
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
                }

                if (context.StopFind)
                {
                    yield break;
                }
            }
        }
    }
}