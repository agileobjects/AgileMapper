namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System;
    using System.Reflection;
    using Extensions.Internal;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class DefaultValueConstantExpressionFactory
    {
        private static readonly MethodInfo _getDefaultValueMethod = typeof(DefaultValueConstantExpressionFactory)
            .GetNonPublicStaticMethod("GetDefaultValue");

        public static Expression CreateFor(Expression expression)
            => GetDefaultValueFor(expression.Type).ToConstantExpression(expression.Type);

        private static object GetDefaultValueFor(Type type)
        {
            if (!type.IsValueType())
            {
                return null;
            }

            var getDefaultValueCaller = GlobalContext.Instance.Cache.GetOrAdd(type, t =>
            {
                var getDefaultValueCall = Expression
                    .Call(_getDefaultValueMethod.MakeGenericMethod(t))
                    .GetConversionToObject();

                var getDefaultValueLambda = Expression.Lambda<Func<object>>(getDefaultValueCall);

                return getDefaultValueLambda.Compile();
            });

            return getDefaultValueCaller.Invoke();
        }

        // ReSharper disable once UnusedMember.Local
        private static T GetDefaultValue<T>() => default(T);
    }
}