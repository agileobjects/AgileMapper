namespace AgileObjects.AgileMapper.Queryables
{
    using System;
    using System.Linq;
    using NetStandardPolyfills;

    internal class QueryProviderSettings
    {
        #region Instances

        #region EF5

        private static readonly QueryProviderSettings _ef5Settings = new QueryProviderSettings(InitEf5Settings)
        {
        };

        private static void InitEf5Settings(QueryProviderSettings settings, IQueryProvider provider)
        {
        }

        #endregion

        #region EF6

        private static readonly QueryProviderSettings _ef6Settings = new QueryProviderSettings(InitEf6Settings)
        {
        };

        private static void InitEf6Settings(QueryProviderSettings settings, IQueryProvider provider)
        {
        }

        #endregion

        #region EFCore2

        private static readonly QueryProviderSettings _efCore2Settings = new QueryProviderSettings(InitEfCore2Settings)
        {
            SupportsStringEqualsIgnoreCase = true
        };

        private static void InitEfCore2Settings(QueryProviderSettings settings, IQueryProvider provider)
        {
        }

        #endregion

        private static readonly QueryProviderSettings _fallbackSettings = new QueryProviderSettings(null)
        {
        };

        #endregion

        private readonly object _initLock = new object();
        private readonly Action<QueryProviderSettings, IQueryProvider> _initAction;
        private bool _isInitialised;

        private QueryProviderSettings(Action<QueryProviderSettings, IQueryProvider> initAction)
        {
            _initAction = initAction;
        }

        public static QueryProviderSettings For(IQueryable queryable)
        {
            var queryableProviderAssemblyName = queryable.Provider.GetType().GetAssembly().GetName();
            var queryableProviderName = queryableProviderAssemblyName.FullName;

            if (queryableProviderName.Contains("EntityFrameworkCore"))
            {
                return _efCore2Settings.Initialised(queryable);
            }

            if (queryableProviderName.Contains("EntityFramework"))
            {
                switch (queryableProviderAssemblyName.Version.Major)
                {
                    case 5:
                        return _ef5Settings.Initialised(queryable);

                    case 6:
                        return _ef6Settings.Initialised(queryable);
                }
            }

            return _fallbackSettings;
        }

        private QueryProviderSettings Initialised(IQueryable queryable)
        {
            if (_isInitialised || (_initAction == null))
            {
                return this;
            }

            lock (_initLock)
            {
                _isInitialised = true;
                _initAction.Invoke(this, queryable.Provider);
            }

            return this;
        }

        public bool SupportsStringEqualsIgnoreCase { get; private set; }
    }
}