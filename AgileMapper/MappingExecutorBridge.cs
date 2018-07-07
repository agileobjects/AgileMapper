namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Api;
    using Caching;
    using Extensions.Internal;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class MappingExecutorBridge<TSource>
    {
        private static readonly ParameterExpression _selectorParameter =
            Parameters.Create(typeof(ITargetSelector<TSource>));

        private static readonly MethodInfo _typedToANewMethod = typeof(ITargetSelector<TSource>)
            .GetPublicInstanceMethods("ToANew")
            .First(m => m.IsGenericMethod && m.GetParameters().None());

        private static readonly ICache<Type, Func<ITargetSelector<TSource>, object>> _createNewCallersByTargetType =
            GlobalContext.Instance.Cache.CreateScoped<Type, Func<ITargetSelector<TSource>, object>>();

        public static object CreateNew(Type resultType, ITargetSelector<TSource> selector)
        {
            var typedCaller = _createNewCallersByTargetType.GetOrAdd(resultType, rt =>
            {
                var typedCreateNewCall = Expression.Call(
                    _selectorParameter,
                    _typedToANewMethod.MakeGenericMethod(rt));

                var createNewCaller = Expression.Lambda<Func<ITargetSelector<TSource>, object>>(
                    typedCreateNewCall,
                    _selectorParameter);

                return createNewCaller.Compile();
            });

            return typedCaller.Invoke(selector);
        }
    }
}