namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions.Internal;
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

            return DataSourceSet.For(rootDataSource, rootMappingData.MapperData);
        }

        public static DataSourceSet CreateFor(IChildMemberMappingData childMappingData)
        {
            var findContext = new DataSourceFindContext(childMappingData);
            var validDataSources = EnumerateDataSources(findContext).ToArray();

            return DataSourceSet.For(validDataSources, findContext.MapperData);
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
                }

                if (context.StopFind)
                {
                    yield break;
                }
            }
        }
    }
}