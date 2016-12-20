namespace AgileObjects.AgileMapper
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

        private readonly ICache<Assembly, IEnumerable<Type>> _typesByAssembly;
        private readonly ICache<Type, ICollection<Type>> _derivedTypesByType;

        public DerivedTypesCache()
        {
            _typesByAssembly = GlobalContext.Instance.Cache.CreateScoped<Assembly, IEnumerable<Type>>();
            _derivedTypesByType = GlobalContext.Instance.Cache.CreateScoped<Type, ICollection<Type>>();
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
            var assemblyTypes = _typesByAssembly.GetOrAdd(type.GetAssembly(), GetRelevantTypesFromAssembly);
            var derivedTypes = assemblyTypes.Where(t => t.IsDerivedFrom(type)).ToList();

            if (derivedTypes.Count != 0)
            {
                derivedTypes.Sort(TypeComparer.MostToLeastDerived);
            }

            return derivedTypes;
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