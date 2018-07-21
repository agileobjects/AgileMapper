namespace AgileObjects.AgileMapper.Configuration
{
    using System;

    internal class ConfiguredServiceProvider
    {
        private readonly Func<Type, object> _unnamedServiceFactory;
        private readonly Func<Type, string, object> _namedServiceFactory;

        public ConfiguredServiceProvider(Func<Type, object> serviceFactory)
        {
            _unnamedServiceFactory = serviceFactory;
        }

        public ConfiguredServiceProvider(Func<Type, string, object> serviceFactory)
        {
            _namedServiceFactory = serviceFactory;
        }

        public TService GetService<TService>(string name)
        {
            return string.IsNullOrEmpty(name)
                ? (TService)_unnamedServiceFactory.Invoke(typeof(TService))
                : (TService)_namedServiceFactory.Invoke(typeof(TService), name);
        }
    }
}