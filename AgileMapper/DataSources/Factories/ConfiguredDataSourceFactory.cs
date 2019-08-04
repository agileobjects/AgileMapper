namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;
    using Extensions.Internal;

    internal static class ConfiguredDataSourceFactory
    {
        public static IEnumerable<IDataSource> Create(DataSourceFindContext context)
        {
            if (context.ConfiguredDataSources.None())
            {
                yield break;
            }

            foreach (var configuredDataSource in context.ConfiguredDataSources)
            {
                yield return context.GetFinalDataSource(configuredDataSource);

                if (!configuredDataSource.IsConditional)
                {
                    yield break;
                }

                ++context.DataSourceIndex;
            }
        }
    }
}