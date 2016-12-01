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
            .GetPublicStaticMethods().First(m => m.Name == "ToArray");

        private static readonly MethodInfo _collectionToArrayMethod = typeof(EnumerableExtensions)
            .GetPublicStaticMethods().Where(m => m.Name == "ToArray").ElementAt(1);

        private static readonly MethodInfo _linqToArrayMethod = typeof(Enumerable)
            .GetPublicStaticMethod("ToArray");

        private static readonly MethodInfo _toListMethod = typeof(Enumerable)
            .GetPublicStaticMethod("ToList");

        public static Expression AndTogether(this ICollection<Expression> expressions)
        {
            if (expressions.None())
            {
                return null;
            }

            if (expressions.HasOne())
            {
                return expressions.First();
            }

            var allClauses = expressions.Chain(firstClause => firstClause, Expression.AndAlso);

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

            var allNotNullCheck = notNullChecks.Chain(firstCheck => firstCheck, Expression.AndAlso);

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

        public static Expression GetValueOrDefaultCall(this Expression nullableExpression)
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
            var conversionMethod = GetToArrayConversionMethod(enumerable, elementType);

            return GetToEnumerableCall(enumerable, conversionMethod, elementType);
        }

        private static MethodInfo GetToArrayConversionMethod(Expression enumerable, Type elementType)
        {
            var wrapperType = typeof(ReadOnlyCollectionWrapper<>).MakeGenericType(elementType);

            if (enumerable.Type == wrapperType)
            {
                return wrapperType.GetMethod("ToArray");
            }

            var listType = typeof(IList<>).MakeGenericType(elementType);

            if (listType.IsAssignableFrom(enumerable.Type))
            {
                return _listToArrayMethod;
            }

            var collectionType = typeof(ICollection<>).MakeGenericType(elementType);

            return collectionType.IsAssignableFrom(enumerable.Type)
                ? _collectionToArrayMethod
                : _linqToArrayMethod;
        }

        public static Expression WithToListCall(this Expression enumerable, Type elementType)
            => GetToEnumerableCall(enumerable, _toListMethod, elementType);

        private static Expression GetToEnumerableCall(Expression enumerable, MethodInfo method, Type elementType)
        {
            if (!method.IsGenericMethod)
            {
                return Expression.Call(enumerable, method);
            }

            var typedToEnumerableMethod = method.MakeGenericMethod(elementType);

            return Expression.Call(typedToEnumerableMethod, enumerable);
        }

        private static readonly Type _typedEnumerable = typeof(Enumerable<>);

        public static Expression GetEmptyInstanceCreation(this Type enumerableType, Type elementType = null)
        {
            if (elementType == null)
            {
                elementType = enumerableType.GetEnumerableElementType();
            }

            if (enumerableType.IsArray)
            {
                return Expression.Field(null, _typedEnumerable.MakeGenericType(elementType), "EmptyArray");
            }

            var typeHelper = new EnumerableTypeHelper(enumerableType, elementType);

            if (typeHelper.IsEnumerableInterface)
            {
                return Expression.Field(null, _typedEnumerable.MakeGenericType(elementType), "Empty");
            }

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
