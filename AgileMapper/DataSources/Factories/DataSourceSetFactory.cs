namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;
    using System.Linq;
    using Members;
    using ObjectPopulation;

    internal static class DataSourceSetFactory
    {
        private static readonly IDataSourceFactory[] _childDataSourceFactories =
        {
            default(ConfiguredDataSourceFactory),
            default(MaptimeDataSourceFactory),
            default(SourceMemberDataSourceFactory),
            default(MetaMemberDataSourceFactory)
        };

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
                foreach (var dataSource in finder.CreateFor(context))
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