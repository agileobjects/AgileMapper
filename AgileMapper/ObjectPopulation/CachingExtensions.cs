namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching;
    using Members;

    internal static class CachingExtensions
    {
        public static ParameterExpression GetOrCreateParameter(this Type type, string name = null)
        {
            if (type == null)
            {
                return null;
            }

            var cache = GlobalContext.Instance
                .Cache
                .CreateScoped<TypeKey, ParameterExpression>(default(HashCodeComparer<TypeKey>));

            var parameter = cache.GetOrAdd(
                TypeKey.ForParameter(type, name),
                key => Parameters.Create(key.Type, key.Name));

            return parameter;
        }
    }
}