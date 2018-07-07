namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Caching;
    using Extensions.Internal;
    using NetStandardPolyfills;

    internal class DerivedTypesCache
    {
        private static readonly object _assembliesLock = new object();

        private readonly List<Assembly> _assemblies;
        private readonly ICache<Assembly, IEnumerable<Type>> _typesByAssembly;
        private readonly ICache<Type, ICollection<Type>> _derivedTypesByType;

        public DerivedTypesCache(CacheSet cacheSet)
        {
            _assemblies = new List<Assembly>();
            _typesByAssembly = cacheSet.CreateScoped<Assembly, IEnumerable<Type>>(default(ReferenceEqualsComparer<Assembly>));
            _derivedTypesByType = cacheSet.CreateScoped<Type, ICollection<Type>>(default(ReferenceEqualsComparer<Type>));
        }

        public void AddAssemblies(Assembly[] assemblies)
        {
            lock (_assembliesLock)
            {
                _assemblies.AddRange(assemblies.Except(_assemblies));
            }
        }

        public ICollection<Type> GetTypesDerivedFrom(Type type)
        {
            if (type.IsSealed() || type.IsFromBcl())
            {
                return Enumerable<Type>.EmptyArray;
            }

            return _derivedTypesByType.GetOrAdd(type, GetDerivedTypesForType);
        }

        private ICollection<Type> GetDerivedTypesForType(Type type)
        {
            var typeAssemblies = new[] { type.GetAssembly() };

            Assembly[] assemblies;

            lock (_assembliesLock)
            {
                assemblies = _assemblies.Any()
                    ? _assemblies.Concat(typeAssemblies).Distinct().ToArray()
                    : typeAssemblies;
            }

            var assemblyTypes = assemblies
                .SelectMany(assembly => _typesByAssembly
                    .GetOrAdd(assembly, GetRelevantTypesFromAssembly));

            var derivedTypes = assemblyTypes.Filter(t => t.IsDerivedFrom(type)).ToArray();

            if (derivedTypes.None())
            {
                return Enumerable<Type>.EmptyArray;
            }

            var derivedTypesList = derivedTypes
                .OrderBy(t => t, TypeComparer.MostToLeastDerived)
                .ToArray();

            return derivedTypesList;
        }

        private static IEnumerable<Type> GetRelevantTypesFromAssembly(Assembly assembly)
        {
            return assembly
                .QueryTypes()
                .Filter(t => t.IsClass() && !t.IsAbstract())
                .ToArray();
        }

        internal void Reset() => _derivedTypesByType.Empty();
    }
}