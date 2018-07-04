namespace AgileObjects.AgileMapper.DataSources.Finders
{
    using System.Collections.Generic;
    using System.Linq;
    using Members;

    internal struct DataSourceFinder
    {
        public static DataSourceSet FindFor(IChildMemberMappingData childMappingData)
        {
            var findContext = new DataSourceFindContext(childMappingData);
            var validDataSources = EnumerateDataSources(findContext).ToArray();

            return new DataSourceSet(findContext.MapperData, validDataSources);
        }

        private static IEnumerable<IDataSource> EnumerateDataSources(DataSourceFindContext context)
        {
            foreach (var finder in EnumerateFinders())
            {
                foreach (var dataSource in finder.FindFor(context))
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

        private static IEnumerable<IDataSourceFinder> EnumerateFinders()
        {
            yield return default(ConfiguredDataSourceFinder);
            yield return default(MaptimeDataSourceFinder);
            yield return default(SourceMemberDataSourceFinder);
            yield return default(MetaMemberDataSourceFinder);
        }
    }
}