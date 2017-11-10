namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using NetStandardPolyfills;

    internal static class StringEqualsIgnoreCaseConverter
    {
        public static bool TryConvert(MethodCallExpression methodCall, IQueryable queryable, out Expression converted)
        {
            if (IsEqualsIgnoreCaseCall(methodCall))
            {
                converted = Convert(methodCall, queryable);
                return true;
            }

            converted = null;
            return false;
        }

        private static bool IsEqualsIgnoreCaseCall(MethodCallExpression methodCall)
        {
            return methodCall.Method.IsStatic &&
                  (methodCall.Arguments.Count == 3) &&
                  (methodCall.Method.DeclaringType == typeof(string)) &&
                  (methodCall.Method.Name == "Equals");
        }

        private static Expression Convert(MethodCallExpression methodCall, IQueryable queryable)
        {
            var subject = methodCall.Arguments[0];

            var subjectToLower = Expression.Call(
                subject,
                typeof(string).GetPublicInstanceMethod("ToLower", parameterCount: 0));

            var comparison = Expression.Equal(subjectToLower, methodCall.Arguments[1]);

            if (NullCheckNotRequired(queryable))
            {
                return comparison;
            }

            var subjectNotNull = subject.GetIsNotDefaultComparison();

            return Expression.AndAlso(subjectNotNull, comparison);
        }

        private static bool NullCheckNotRequired(IQueryable queryable)
        {
            var providerName = queryable.Provider.GetType().FullName;

            return !providerName.Contains("EntityFrameworkCore");
        }
    }
}