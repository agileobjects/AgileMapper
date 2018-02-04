namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions.Internal;
    using NetStandardPolyfills;

    internal static class NullConstantExpressionFactory
    {
        private static readonly MethodInfo _getDefaultValueMethod = typeof(NullConstantExpressionFactory)
            .GetNonPublicStaticMethod("GetDefaultValue");

        public static Expression CreateFor(Expression expression) => CreateFor(expression.Type);

        public static Expression CreateFor(Type type)
            => GetDefaultValueFor(type).ToConstantExpression(type);

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