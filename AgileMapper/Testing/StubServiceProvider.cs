namespace AgileObjects.AgileMapper.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetStandardPolyfills;

    /// <summary>
    /// A stub service provider implementation, providing a simple GetService(Type) method for a set of predefined objects.
    /// </summary>
    public class StubServiceProvider
    {
        private readonly Dictionary<Type, object> _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="StubServiceProvider"/> class using the given <paramref name="services"/>.
        /// Services will be cached in a Dictionary{Type, object} against their concrete and implemented interface Types; if more
        /// than one supplied object is of the same Type or implements the same interface, an Exception will be thrown.
        /// </summary>
        /// <param name="services">The objects to store and later make available in the service provider.</param>
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

        /// <summary>
        /// Gets the stored object of the given <paramref name="serviceType"/>, or attempts to create
        /// one using its parameterless constructor, if none was supplied in the constructor. If the
        /// Type was not supplied in the constructor and has no parameterless constructor, an Exception
        /// is thrown.
        /// </summary>
        /// <param name="serviceType">The Type of the service to retrieve.</param>
        /// <returns>
        /// The stored object of the given <paramref name="serviceType"/>, or ones created using
        /// its parameterless constructor, if none was supplied in the constructor.
        /// </returns>
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