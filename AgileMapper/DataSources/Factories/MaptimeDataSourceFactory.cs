namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;
    using Extensions.Internal;

    internal static class MaptimeDataSourceFactory
    {
        private static readonly IMaptimeDataSourceFactory[] _mapTimeDataSourceFactories =
        {
            default(DictionaryDataSourceFactory)
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
            var applicableFactory = _mapTimeDataSourceFactories
                .FirstOrDefault(factory => factory.IsFor(context.MapperData));

            if (applicableFactory == null)
            {
                maptimeDataSources = null;
                return false;
            }

            maptimeDataSources = applicableFactory.Create(context.ChildMappingData);
            return true;
        }
    }
}