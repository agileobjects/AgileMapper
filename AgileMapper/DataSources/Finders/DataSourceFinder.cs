namespace AgileObjects.AgileMapper.DataSources.Finders
{
    using System.Collections.Generic;
    using System.Linq;
    using Members;

    internal static class DataSourceFinder
    {
        private static readonly IDataSourceFinder[] _finders =
        {
            default(ConfiguredDataSourceFinder),
            default(MaptimeDataSourceFinder),
            default(SourceMemberDataSourceFinder),
            default(MetaMemberDataSourceFinder),
            default(ConfiguredRootSourceDataSourceFinder)
        };

        public static DataSourceSet FindFor(IChildMemberMappingData childMappingData)
        {
            var findContext = new DataSourceFindContext(childMappingData);
            var validDataSources = EnumerateDataSources(findContext).ToArray();

            return new DataSourceSet(findContext.MapperData, validDataSources);
        }

        private static IEnumerable<IDataSource> EnumerateDataSources(DataSourceFindContext context)
        {
            foreach (var finder in _finders)
            {
                foreach (var dataSource in finder.FindFor(context))
                {
                    if (dataSource.IsValid)
                    {
                        yield return dataSource;

                        if (!dataSource.IsConditional)
                        {
                            yield break;
                        }
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