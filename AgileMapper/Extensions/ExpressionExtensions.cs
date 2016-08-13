namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;

    internal static partial class ExpressionExtensions
    {
        private static readonly MethodInfo _toArrayMethod = typeof(Enumerable)
            .GetMethod("ToArray", Constants.PublicStatic);

        private static readonly MethodInfo _toListMethod = typeof(Enumerable)
            .GetMethod("ToList", Constants.PublicStatic);

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
            while (true)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Convert:
                        expression = ((UnaryExpression)expression).Operand;
                        continue;

                    case ExpressionType.Call:
                        return GetMethodCallMemberAccess((MethodCallExpression)expression);

                    case ExpressionType.Lambda:
                        expression = ((LambdaExpression)expression).Body;
                        continue;

                    case ExpressionType.MemberAccess:
                        return expression;
                }

                throw new NotSupportedException("Unable to get member access from " + expression.NodeType + " Expression");
            }
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

        public static Expression GetIsNotDefaultComparisonsOrNull(this IEnumerable<Expression> expressions)
        {
            var notNullChecks = expressions
                .Select(exp => exp.GetIsNotDefaultComparison())
                .ToArray();

            if (notNullChecks.Length == 0)
            {
                return null;
            }

            var allNotNullCheck = notNullChecks
                .Skip(1)
                .Aggregate(notNullChecks.First(), Expression.AndAlso);

            return allNotNullCheck;
        }

        public static BinaryExpression GetIsDefaultComparison(this Expression expression)
            => Expression.Equal(expression, Expression.Default(expression.Type));

        public static BinaryExpression GetIsNotDefaultComparison(this Expression expression)
            => Expression.NotEqual(expression, Expression.Default(expression.Type));

        public static Expression GetToValueOrDefaultCall(this Expression nullableExpression)
        {
            return Expression.Call(
                nullableExpression,
                nullableExpression.Type.GetMethod("GetValueOrDefault", Constants.NoTypeArguments));
        }

        public static Expression GetConversionTo(this Expression expression, Type targetType)
            => (expression.Type != targetType) ? Expression.Convert(expression, targetType) : expression;

        public static Expression WithToArrayCall(this Expression enumerable, Type elementType)
            => GetToEnumerableCall(enumerable, _toArrayMethod, elementType);

        public static Expression WithToListCall(this Expression enumerable, Type elementType)
            => GetToEnumerableCall(enumerable, _toListMethod, elementType);

        private static Expression GetToEnumerableCall(Expression enumerable, MethodInfo method, Type elementType)
        {
            var typedToEnumerableMethod = method.MakeGenericMethod(elementType);

            return Expression.Call(typedToEnumerableMethod, enumerable);
        }

        public static Expression GetEmptyInstanceCreation(this Type enumerableType, Type elementType = null)
        {
            if (elementType == null)
            {
                elementType = enumerableType.GetEnumerableElementType();
            }

            if (enumerableType.IsArray)
            {
                return Expression.NewArrayBounds(elementType, Expression.Constant(0));
            }

            var typeHelper = new EnumerableTypeHelper(enumerableType, elementType);

            return Expression.New(typeHelper.IsCollection ? typeHelper.CollectionType : typeHelper.ListType);
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

        public static Expression WithNullChecks(this Expression expression, Expression rootParameter)
        {
            var originalExpression = expression;

            // Skip the outermost check:
            expression = expression.GetParentOrNull();

            if (expression == rootParameter)
            {
                return originalExpression;
            }

            var defaultValue = Expression.Default(originalExpression.Type);
            var valueOrDefaultExpression = originalExpression;

            while (expression != null)
            {
                var expressionIsNotDefault = expression.GetIsNotDefaultComparison();

                valueOrDefaultExpression = Expression.Condition(
                    expressionIsNotDefault,
                    valueOrDefaultExpression,
                    defaultValue);

                expression = expression.GetParentOrNull();

                if (expression == rootParameter)
                {
                    break;
                }
            }

            return valueOrDefaultExpression;
        }
    }
}
