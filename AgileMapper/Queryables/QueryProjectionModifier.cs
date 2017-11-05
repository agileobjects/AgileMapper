namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using NetStandardPolyfills;

    internal class QueryProjectionModifier : ExpressionVisitor
    {
        public Expression Modify(Expression queryProjection)
        {
            return VisitAndConvert(queryProjection, "Modify");
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            if (IsStringEqualsIgnoreCase(methodCall))
            {
                return GetStringToLowerEqualsComparison(methodCall);
            }

            return base.VisitMethodCall(methodCall);
        }

        private static bool IsStringEqualsIgnoreCase(MethodCallExpression methodCall)
        {
            return methodCall.Method.IsStatic &&
                   (methodCall.Arguments.Count == 3) &&
                   (methodCall.Method.DeclaringType == typeof(string)) &&
                   (methodCall.Method.Name == "Equals");
        }

        private static Expression GetStringToLowerEqualsComparison(MethodCallExpression methodCall)
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