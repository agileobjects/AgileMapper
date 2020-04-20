namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;
    using Extensions.Internal;

    internal static class ConfiguredDataSourcesFactory
    {
        public static IEnumerable<IDataSource> Create(DataSourceFindContext context)
        {
            if (context.ConfiguredDataSources.None())
            {
                yield break;
            }

            foreach (var dataSource in context.ConfiguredDataSources)
            {
                yield return context.GetFinalDataSource(dataSource);

                if (!dataSource.IsConditional)
                {
                    yield break;
                }
            }
        }
    }
}