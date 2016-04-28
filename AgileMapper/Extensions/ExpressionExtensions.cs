namespace AgileObjects.AgileMapper.Extensions
{
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using ReadableExpressions.Extensions;

    internal static class ExpressionExtensions
    {
        private static readonly MethodInfo _toArrayMethod = typeof(Enumerable)
            .GetMethod("ToArray", Constants.PublicStatic);

        public static Expression GetParentOrNull(this Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Call:
                    return ((MethodCallExpression)expression).GetSubject();

                case ExpressionType.MemberAccess:
                    return ((MemberExpression)expression).Expression;
            }

            return null;
        }

        public static BinaryExpression GetIsNotDefaultComparison(this Expression expression)
        {
            return Expression.NotEqual(expression, Expression.Default(expression.Type));
        }

        public static Expression WithToArrayCall(this Expression enumerable)
        {
            var elementType = enumerable.Type.GetEnumerableElementType();
            var typedToArrayMethod = _toArrayMethod.MakeGenericMethod(elementType);

            return Expression.Call(typedToArrayMethod, enumerable);
        }

        public static bool IsRootedIn(this Expression expression, Expression possibleParent)
        {
            var parent = expression.GetParentOrNull();

            while (parent != null)
            {
                if (parent == possibleParent)
                {
                    return true;
                }

                parent = parent.GetParentOrNull();
            }

            return false;
        }
    }
}
