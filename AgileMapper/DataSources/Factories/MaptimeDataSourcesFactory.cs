namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;

    internal static class MaptimeDataSourcesFactory
    {
        private static readonly MaptimeDataSourcesFactorySource[] _mapTimeDataSourceFactorySources =
        {
            DictionaryDataSourceFactory.TryGet
        };

        public static IEnumerable<IDataSource> Create(DataSourceFindContext context)
        {
            if (!UseMaptimeDataSources(context, out var maptimeDataSources))
            {
                yield break;
            }

            context.StopFind = true;

            foreach (var maptimeDataSource in maptimeDataSources)
            {
                yield return context.GetFinalDataSource(maptimeDataSource);

                if (maptimeDataSource.IsConditional)
                {
                    continue;
                }

                yield break;
            }

            yield return context.GetFallbackDataSource();
        }

        private static bool UseMaptimeDataSources(
            DataSourceFindContext context,
            out IEnumerable<IDataSource> maptimeDataSources)
        {
            foreach (var mapTimeDataSourceFactorySource in _mapTimeDataSourceFactorySources)
            {
                if (mapTimeDataSourceFactorySource.Invoke(context.MemberMapperData, out var factory))
                {
                    maptimeDataSources = factory.Invoke(context.MemberMappingData);
                    return true;
                }
            }

            maptimeDataSources = null;
            return false;
        }
    }
}