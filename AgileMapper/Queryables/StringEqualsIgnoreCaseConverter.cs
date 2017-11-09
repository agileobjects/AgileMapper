namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using NetStandardPolyfills;

    internal static class StringEqualsIgnoreCaseConverter
    {
        public static bool TryConvert(MethodCallExpression methodCall, out Expression converted)
        {
            if (IsEqualsIgnoreCaseCall(methodCall))
            {
                converted = Convert(methodCall);
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

        private static Expression Convert(MethodCallExpression methodCall)
        {
            var subjectToLower = Expression.Call(
                methodCall.Arguments[0],
                typeof(string)
                    .GetPublicInstanceMethods()
                    .First(m => (m.Name == "ToLower") && (m.GetParameters().None())));

            var comparison = Expression.Equal(subjectToLower, methodCall.Arguments[1]);

            return comparison;
        }
    }
}