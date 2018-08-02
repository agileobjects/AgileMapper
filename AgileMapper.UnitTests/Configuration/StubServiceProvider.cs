namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetStandardPolyfills;

    internal class StubServiceProvider
    {
        private readonly Dictionary<Type, object> _services;

        public StubServiceProvider(params object[] services)
        {
            _services = services
                .SelectMany(s => new[] { s.GetType() }.Concat(s.GetType().GetAllInterfaces()).Select(t => new
                {
                    Service = s,
                    Type = t
                }))
                .ToDictionary(d => d.Type, d => d.Service);
        }

        public object GetService(Type serviceType)
        {
            if (_services.TryGetValue(serviceType, out var service))
            {
                return service;
            }

            return _services[serviceType] = Activator.CreateInstance(serviceType);
        }
    }
}