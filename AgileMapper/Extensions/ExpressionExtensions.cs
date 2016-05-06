namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;
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

                case ExpressionType.Lambda:
                    return GetMemberAccess(((LambdaExpression)expression).Body);

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
            var valueParameter = Parameters.Create(methodInfo.GetParameters().First().ParameterType, "value");

            return Expression.Call(instance, methodInfo, valueParameter);
        }

        public static Expression GetIsNotDefaultComparisons(this IEnumerable<Expression> expressions)
        {
            var notNullChecks = expressions
                .Select(exp => new
                {
                    Expression = exp,
                    Depth = GetDepth(exp)
                })
                .OrderBy(d => d.Depth)
                .ThenBy(d => d.Expression.ToString())
                .Select(d => d.Expression.GetIsNotDefaultComparison())
                .ToArray();

            var allNotNullCheck = notNullChecks
                .Skip(1)
                .Aggregate(notNullChecks.First(), Expression.AndAlso);

            return allNotNullCheck;
        }

        private static int GetDepth(Expression expression)
        {
            var depth = -1;
            var parent = expression;

            while (parent != null)
            {
                ++depth;
                parent = parent.GetParentOrNull();
            }

            return depth;
        }

        public static BinaryExpression GetIsDefaultComparison(this Expression expression)
        {
            return Expression.Equal(expression, Expression.Default(expression.Type));
        }

        public static BinaryExpression GetIsNotDefaultComparison(this Expression expression)
        {
            return Expression.NotEqual(expression, Expression.Default(expression.Type));
        }

        public static Expression GetToValueOrDefaultCall(this Expression nullableExpression)
        {
            return Expression.Call(
                nullableExpression,
                nullableExpression.Type.GetMethod("GetValueOrDefault", Constants.NoTypeArguments));
        }

        public static Expression GetConversionTo(this Expression expression, Type targetType)
        {
            return (expression.Type != targetType) ? Expression.Convert(expression, targetType) : expression;
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

        public static Expression ReplaceParameter(this LambdaExpression lambda, Expression replacement)
        {
            return ReplaceParameters(lambda, replacement);
        }

        public static Expression ReplaceParameters(this LambdaExpression lambda, params Expression[] replacements)
        {
            var replacementsByParameter = lambda
                .Parameters
                .Cast<Expression>()
                .Select((p, i) => new { Parameter = p, Replacement = replacements[i] })
                .ToDictionary(d => d.Parameter, d => d.Replacement);

            return new ExpressionReplacer(replacementsByParameter).ReplaceIn(lambda.Body);
        }

        #region Replace Helper

        private class ExpressionReplacer
        {
            private readonly Dictionary<Expression, Expression> _replacementsByTarget;

            public ExpressionReplacer(Dictionary<Expression, Expression> replacementsByTarget)
            {
                _replacementsByTarget = replacementsByTarget;
            }

            public Expression ReplaceIn(Expression expression)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Add:
                    case ExpressionType.Divide:
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.Modulo:
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
                if (expression == null)
                {
                    return null;
                }

                Expression replacement;

                return _replacementsByTarget.TryGetValue(expression, out replacement)
                    ? replacement
                    : ReplaceIn(expression);
            }
        }

        #endregion
    }
}
