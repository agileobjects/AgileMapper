namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal static class CachingExtensions
    {
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