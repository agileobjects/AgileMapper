namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;

    internal static partial class ExpressionExtensions
    {
        private static readonly MethodInfo _listToArrayMethod = typeof(EnumerableExtensions)
            .GetPublicStaticMethod("ToArray");

        private static readonly MethodInfo _linqToArrayMethod = typeof(Enumerable)
            .GetPublicStaticMethod("ToArray");

        private static readonly MethodInfo _toListMethod = typeof(Enumerable)
            .GetPublicStaticMethod("ToList");

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

        public static Expression AndTogether(this ICollection<Expression> expressions)
        {
            if (expressions.Count == 0)
            {
                return null;
            }

            if (expressions.Count == 1)
            {
                return expressions.First();
            }

            var allClauses = expressions.Skip(1).Aggregate(expressions.First(), Expression.AndAlso);

            return allClauses;
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

        public static Expression GetIsDefaultComparison(this Expression expression)
            => Expression.Equal(expression, Expression.Default(expression.Type));

        public static Expression GetIsNotDefaultComparison(this Expression expression)
        {
            if (expression.Type.IsNullableType())
            {
                return Expression.Property(expression, "HasValue");
            }

            return Expression.NotEqual(expression, Expression.Default(expression.Type));
        }

        public static Expression GetToValueOrDefaultCall(this Expression nullableExpression)
        {
            var parameterlessGetValueOrDefault = nullableExpression.Type
                .GetPublicInstanceMethods()
                .First(m => (m.Name == "GetValueOrDefault") && !m.GetParameters().Any());

            return Expression.Call(nullableExpression, parameterlessGetValueOrDefault);
        }

        public static Expression GetConversionTo(this Expression expression, Type targetType)
            => (expression.Type != targetType) ? Expression.Convert(expression, targetType) : expression;

        public static Expression WithToArrayCall(this Expression enumerable, Type elementType)
        {
            var conversionMethod = typeof(IList<>).MakeGenericType(elementType).IsAssignableFrom(enumerable.Type)
                ? _listToArrayMethod
                : _linqToArrayMethod;

            return GetToEnumerableCall(enumerable, conversionMethod, elementType);
        }

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
    }
}
