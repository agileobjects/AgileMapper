﻿namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Caching;
    using Extensions;
    using NetStandardPolyfills;

    internal class DerivedTypesCache
    {
        private static readonly Type[] _noTypes = Constants.NoTypeArguments;

        private readonly List<Assembly> _assemblies;
        private readonly ICache<Assembly, IEnumerable<Type>> _typesByAssembly;
        private readonly ICache<Type, ICollection<Type>> _derivedTypesByType;

        public DerivedTypesCache(CacheSet cacheSet)
        {
            _assemblies = new List<Assembly>();
            _typesByAssembly = cacheSet.CreateScoped<Assembly, IEnumerable<Type>>();
            _derivedTypesByType = cacheSet.CreateScoped<Type, ICollection<Type>>();
        }

        public void AddAssemblies(Assembly[] assemblies)
        {
            _assemblies.AddRange(assemblies.Except(_assemblies));
        }

        public ICollection<Type> GetTypesDerivedFrom(Type type)
        {
            if (type.IsSealed() || type.IsFromBcl())
            {
                return _noTypes;
            }

            return _derivedTypesByType.GetOrAdd(type, GetDerivedTypesForType);
        }

        private ICollection<Type> GetDerivedTypesForType(Type type)
        {
            var typeAssembly = type.GetAssembly();

            var assemblies = _assemblies.Count > 0
                ? _assemblies.Concat(typeAssembly).Distinct()
                : new[] { typeAssembly };

            var assemblyTypes = assemblies
                .SelectMany(assembly => _typesByAssembly
                    .GetOrAdd(assembly, GetRelevantTypesFromAssembly));

            var derivedTypes = assemblyTypes.Where(t => t.IsDerivedFrom(type)).ToList();

            if (derivedTypes.Count != 0)
            {
                derivedTypes.Sort(TypeComparer.MostToLeastDerived);
            }

            return derivedTypes.ToArray();
        }

        private static IEnumerable<Type> GetRelevantTypesFromAssembly(Assembly assembly)
        {
            return GetTypesFromAssembly(assembly)
                .Where(t => t.IsClass() && !t.IsAbstract())
                .ToArray();
        }

        private static IEnumerable<Type> GetTypesFromAssembly(Assembly assembly)
        {
            try
            {
                IEnumerable<Type> types = assembly.GetTypes();

                if (Constants.ReflectionNotPermitted)
                {
                    types = types.Where(t => t.IsPublic());
                }

                return types;
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.WhereNotNull();
            }
        }
    }
}