namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using Caching;
    using Members;

    internal static class CachingExtensions
    {
        public static IObjectMapper GetOrAddMapper(
            this ICache<ObjectMapperKeyBase, IObjectMapper> objectMapperCache,
            IObjectMappingData mappingData)
        {
            var mapper = objectMapperCache.GetOrAdd(
                mappingData.MapperKey,
                key => key.MappingData.Mapper);

            return mapper;
        }

        public static ParameterExpression GetOrCreateParameter(this Type type, string name = null)
        {
            var cache = GlobalContext.Instance.Cache.CreateScoped<TypeKey, ParameterExpression>();

            var parameter = cache.GetOrAdd(
                TypeKey.ForParameter(type, name),
                key => Parameters.Create(key.Type, key.Name));

            return parameter;
        }
    }
}