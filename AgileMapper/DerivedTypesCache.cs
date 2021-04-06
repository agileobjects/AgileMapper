namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Caching;
    using Extensions;
    using Extensions.Internal;
    using NetStandardPolyfills;

    internal class DerivedTypesCache
    {
        private static readonly object _assembliesLock = new object();

        private readonly List<Assembly> _assemblies;
        private readonly ICache<Assembly, IEnumerable<Type>> _typesByAssembly;
        private readonly ICache<Type, IList<Type>> _derivedTypesByType;

        public DerivedTypesCache(CacheSet cacheSet)
        {
            _assemblies = new List<Assembly>();
            _typesByAssembly = cacheSet.CreateScopedWithHashCodes<Assembly, IEnumerable<Type>>();
            _derivedTypesByType = cacheSet.CreateScopedWithHashCodes<Type, IList<Type>>();
        }

        public void AddAssemblies(Assembly[] assemblies)
        {
            lock (_assembliesLock)
            {
                _assemblies.AddRange(assemblies.Except(_assemblies));
            }
        }

        public IList<Type> GetTypesDerivedFrom(Type baseType)
        {
            if (baseType.IsInterface() || baseType.IsSealed() || baseType.IsFromBcl())
            {
                return Constants.EmptyTypeArray;
            }

            return _derivedTypesByType.GetOrAdd(baseType, GetDerivedTypesForType);
        }

        private IList<Type> GetDerivedTypesForType(Type baseType)
        {
            var typeAssemblies = new[] { baseType.GetAssembly() };

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

            var derivedTypes = assemblyTypes
                .Filter(baseType, (bt, dt) => dt.IsDerivedFrom(bt))
                .ToArray();

            switch (derivedTypes.Length)
            {
                case 0:
                    return Constants.EmptyTypeArray;

                case 1:
                    return derivedTypes;

                default:
                    return derivedTypes
                        .OrderBy(t => t, TypeComparer.MostToLeastDerived)
                        .ToArray();
            }
        }

        private static IEnumerable<Type> GetRelevantTypesFromAssembly(Assembly assembly)
            => assembly.QueryTypes().Filter(t => t.IsClass() && !t.IsAbstract()).ToArray();

        internal void Reset() => _derivedTypesByType.Empty();
    }
}