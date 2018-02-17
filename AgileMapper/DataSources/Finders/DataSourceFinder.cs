namespace AgileObjects.AgileMapper.DataSources.Finders
{
    using System.Collections.Generic;
    using System.Linq;
    using Members;

    internal static class DataSourceFinder
    {
        private static readonly IDataSourceFinder[] _finders =
        {
            new ConfiguredDataSourceFinder(),
            new MaptimeDataSourceFinder(),
            new SourceMemberDataSourceFinder()
        };

        public static DataSourceSet FindFor(IChildMemberMappingData childMappingData)
        {
            var findContext = new DataSourceFindContext(childMappingData);

            var validDataSources = EnumerateDataSources(findContext)
                .Where(ds => ds.IsValid)
                .ToArray();

            return new DataSourceSet(findContext.MapperData, validDataSources);
        }

        private static IEnumerable<IDataSource> EnumerateDataSources(DataSourceFindContext context)
        {
            foreach (var finder in _finders)
            {
                foreach (var dataSource in finder.FindFor(context))
                {
                    yield return dataSource;
                }

                if (context.FindComplete)
                {
                    yield break;
                }
            }
        }
    }
}