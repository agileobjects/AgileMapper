namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions.Internal;
    using NetStandardPolyfills;

    internal static class DefaultExpressionConverter
    {
        private static readonly MethodInfo _getDefaultValueMethod = typeof(DefaultExpressionConverter)
            .GetNonPublicStaticMethod("GetDefaultValue");

        public static Expression Convert(Expression defaultExpression)
            => Convert((DefaultExpression)defaultExpression);

        public static Expression Convert(DefaultExpression defaultExpression)
            => GetDefaultValueFor(defaultExpression.Type).ToConstantExpression(defaultExpression.Type);

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