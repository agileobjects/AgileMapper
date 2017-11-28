namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System;
    using System.Linq.Expressions;
    using Extensions;
    using NetStandardPolyfills;

    internal static class DefaultExpressionConverter
    {
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
                    .Call(typeof(DefaultExpressionConverter)
                        .GetNonPublicStaticMethod("GetDefaultValue")
                        .MakeGenericMethod(t))
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