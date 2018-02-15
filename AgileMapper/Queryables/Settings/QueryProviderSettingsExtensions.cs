namespace AgileObjects.AgileMapper.Queryables.Settings
{
    using ObjectPopulation;

    internal static class QueryProviderSettingsExtensions
    {
        public static IQueryProviderSettings GetQueryProviderSettings(this IObjectMappingData mappingData)
        {
            while (!mappingData.IsRoot)
            {
                mappingData = mappingData.Parent;
            }

            var queryProviderType = ((QueryProjectorKey)mappingData.MapperKey).QueryProviderType;
            var providerSettings = QueryProviderSettings.For(queryProviderType);

            return providerSettings;
        }
    }
}