namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq.Expressions;
    using NetStandardPolyfills;

    internal static class StringEqualsIgnoreCaseConverter
    {
        public static bool TryConvert(MethodCallExpression methodCall, QueryProviderSettings settings, out Expression converted)
        {
            if (settings.SupportsStringEqualsIgnoreCase || IsNotEqualsIgnoreCaseCall(methodCall))
            {
                converted = null;
                return false;
            }

            converted = Convert(methodCall);
            return true;
        }

        private static bool IsNotEqualsIgnoreCaseCall(MethodCallExpression methodCall)
        {
            return !methodCall.Method.IsStatic ||
                   (methodCall.Arguments.Count != 3) ||
                   (methodCall.Method.DeclaringType != typeof(string)) ||
                   (methodCall.Method.Name != "Equals");
        }

        private static Expression Convert(MethodCallExpression methodCall)
        {
            var subjectToLower = Expression.Call(
                methodCall.Arguments[0],
                typeof(string).GetPublicInstanceMethod("ToLower", parameterCount: 0));

            var comparison = Expression.Equal(subjectToLower, methodCall.Arguments[1]);

            return comparison;
        }
    }
}