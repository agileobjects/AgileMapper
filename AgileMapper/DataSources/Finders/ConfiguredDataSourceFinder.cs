namespace AgileObjects.AgileMapper.DataSources.Finders
{
    using System.Collections.Generic;
    using Extensions.Internal;

    internal struct ConfiguredDataSourceFinder : IDataSourceFinder
    {
        public IEnumerable<IDataSource> FindFor(DataSourceFindContext context)
        {
            if (NoDataSourcesAreConfigured(context))
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

        private static bool NoDataSourcesAreConfigured(DataSourceFindContext context)
        {
            context.ConfiguredDataSources = context
                .MapperData
                .MapperContext
                .UserConfigurations
                .GetDataSources(context.MapperData);

            return context.ConfiguredDataSources.None();
        }
    }
}