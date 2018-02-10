namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Linq.Expressions;
    using Extensions.Internal;
    using NetStandardPolyfills;

    internal static class StringEqualsIgnoreCaseConverter
    {
        public static bool TryConvert(
            MethodCallExpression methodCall,
            IQueryProjectionModifier context,
            out Expression converted)
        {
            if (context.Settings.SupportsStringEqualsIgnoreCase || IsNotEqualsIgnoreCaseCall(methodCall))
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
                   (methodCall.Method.Name != nameof(string.Equals));
        }

        private static Expression Convert(MethodCallExpression methodCall)
        {
            var subjectToLower = Expression.Call(
                methodCall.Arguments[0],
                typeof(string).GetPublicInstanceMethod("ToLower", parameterCount: 0));

            var comparisonValue = GetComparisonValue(methodCall.Arguments[1]);
            var comparison = Expression.Equal(subjectToLower, comparisonValue);

            return comparison;
        }

        private static Expression GetComparisonValue(Expression value)
        {
            if (value.NodeType != ExpressionType.Constant)
            {
                return value;
            }

            var stringValue = ((ConstantExpression)value).Value?.ToString().ToLowerInvariant();

            return stringValue.ToConstantExpression();
        }
    }
}