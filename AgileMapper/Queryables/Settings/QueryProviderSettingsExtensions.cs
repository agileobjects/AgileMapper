namespace AgileObjects.AgileMapper.Queryables.Settings
{
    using ObjectPopulation;

    internal static class QueryProviderSettingsExtensions
    {
        public static IQueryProviderSettings GetQueryProviderSettings(this IObjectMappingData mappingData)
        {
            var queryProviderType = ((QueryProjectorKey)mappingData.MapperKey).QueryProviderType;
            var providerSettings = QueryProviderSettings.For(queryProviderType);

            return providerSettings;
        }
    }
}