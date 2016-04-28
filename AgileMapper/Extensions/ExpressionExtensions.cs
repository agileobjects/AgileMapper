namespace AgileObjects.AgileMapper.Extensions
{
    using System;
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

        public static string GetMemberName(this Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Call:
                    return ((MethodCallExpression)expression).Method.Name;

                case ExpressionType.MemberAccess:
                    return ((MemberExpression)expression).Member.Name;
            }

            throw new NotSupportedException("Unable to get member name of " + expression.NodeType + " Expression");
        }

        public static Expression GetMemberAccess(this Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Convert:
                    return GetMemberAccess(((UnaryExpression)expression).Operand);

                case ExpressionType.Call:
                    return GetMethodCallMemberAccess((MethodCallExpression)expression);

                case ExpressionType.MemberAccess:
                    return expression;
            }

            throw new NotSupportedException("Unable to get member access from " + expression.NodeType + " Expression");
        }

        private static Expression GetMethodCallMemberAccess(MethodCallExpression methodCall)
        {
            if ((methodCall.Type != typeof(Delegate)) || (methodCall.Method.Name != "CreateDelegate"))
            {
                return methodCall;
            }

            // ReSharper disable once PossibleNullReferenceException
            var methodInfo = (MethodInfo)((ConstantExpression)methodCall.Object).Value;
            var instance = methodCall.Arguments.Last();
            var valueParameter = Expression.Parameter(methodInfo.GetParameters().First().ParameterType, "value");

            return Expression.Call(instance, methodInfo, valueParameter);
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

        public static Expression Replace(this Expression subject, Expression target, Expression replacement)
        {
            return new ExpressionReplacer(target, replacement).ReplaceIn(subject);
        }

        #region Replace Helper

        private class ExpressionReplacer
        {
            private readonly Expression _target;
            private readonly Expression _replacement;

            public ExpressionReplacer(Expression target, Expression replacement)
            {
                _target = target;
                _replacement = replacement;
            }

            public Expression ReplaceIn(Expression expression)
            {
                switch (expression?.NodeType)
                {
                    case ExpressionType.Add:
                    case ExpressionType.Divide:
                    case ExpressionType.Multiply:
                    case ExpressionType.Subtract:
                        return ReplaceIn((BinaryExpression)expression);

                    case ExpressionType.Call:
                        return ReplaceIn((MethodCallExpression)expression);

                    case ExpressionType.Convert:
                        return ReplaceIn((UnaryExpression)expression);

                    case ExpressionType.MemberAccess:
                        return ReplaceIn((MemberExpression)expression);
                }

                return expression;
            }

            private Expression ReplaceIn(BinaryExpression binary)
            {
                return binary.Update(Replace(binary.Left), binary.Conversion, Replace(binary.Right));
            }

            private Expression ReplaceIn(MethodCallExpression call)
            {
                return call.Update(Replace(call.Object), call.Arguments.Select(Replace).ToArray());
            }

            private Expression ReplaceIn(UnaryExpression unary)
            {
                return unary.Update(Replace(unary.Operand));
            }

            private Expression ReplaceIn(MemberExpression memberAccess)
            {
                return memberAccess.Update(Replace(memberAccess.Expression));
            }

            private Expression Replace(Expression expression)
            {
                return expression == _target ? _replacement : ReplaceIn(expression);
            }
        }

        #endregion
    }
}
