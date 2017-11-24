namespace AgileObjects.AgileMapper.Queryables.Settings
{
    using System.Linq;
    using NetStandardPolyfills;

    internal static class QueryProviderSettings
    {
        private static readonly IQueryProviderSettings _ef5Settings = new Ef5QueryProviderSettings();
        private static readonly IQueryProviderSettings _ef6Settings = new Ef6QueryProviderSettings();
        private static readonly IQueryProviderSettings _efCore2Settings = new EfCore2QueryProviderSettings();
        private static readonly IQueryProviderSettings _defaultSettings = new DefaultQueryProviderSettings();

        public static IQueryProviderSettings For(IQueryable queryable)
        {
            if (queryable.Provider == null)
            {
                return _defaultSettings;
            }

            var queryableProviderAssemblyName = queryable.Provider.GetType().GetAssembly().GetName();
            var queryableProviderName = queryableProviderAssemblyName.FullName;

            if (queryableProviderName.Contains("EntityFrameworkCore"))
            {
                return _efCore2Settings;
            }

            if (queryableProviderName.Contains("EntityFramework"))
            {
                switch (queryableProviderAssemblyName.Version.Major)
                {
                    case 5:
                        return _ef5Settings;

                    case 6:
                        return _ef6Settings;
                }
            }

            return _defaultSettings;
        }
    }
}