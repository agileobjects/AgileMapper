namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using NetStandardPolyfills;
    using ObjectPopulation.Enumerables;
    using ReadableExpressions.Extensions;

    internal static partial class ExpressionExtensions
    {
        private static readonly MethodInfo _listToArrayMethod = typeof(EnumerableExtensions)
            .GetPublicStaticMethods().First(m => m.Name == "ToArray");

        private static readonly MethodInfo _collectionToArrayMethod = typeof(EnumerableExtensions)
            .GetPublicStaticMethods().Where(m => m.Name == "ToArray").ElementAt(1);

        private static readonly MethodInfo _linqToArrayMethod = typeof(Enumerable)
            .GetPublicStaticMethod("ToArray");

        private static readonly MethodInfo _linqToListMethod = typeof(Enumerable)
            .GetPublicStaticMethod("ToList");

        private static readonly MethodInfo _stringEqualsMethod = typeof(string)
            .GetPublicInstanceMethods()
            .First(m => (m.Name == "Equals") && (m.GetParameters().Length == 2));

        [DebuggerStepThrough]
        public static BinaryExpression AssignTo(this Expression subject, Expression value)
            => Expression.Assign(subject, value);

        [DebuggerStepThrough]
        public static ConstantExpression ToConstantExpression<T>(this T item)
            => ToConstantExpression(item, typeof(T));

        [DebuggerStepThrough]
        public static ConstantExpression ToConstantExpression<TItem>(this TItem item, Type type)
            => Expression.Constant(item, type);

        [DebuggerStepThrough]
        public static DefaultExpression ToDefaultExpression(this Type type) => Expression.Default(type);

        public static Expression AndTogether(this IList<Expression> expressions)
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

        public static LoopExpression InsertAssignment(
            this LoopExpression loop,
            int insertIndex,
            ParameterExpression variable,
            Expression value)
        {
            var loopBody = (BlockExpression)loop.Body;
            var loopBodyExpressions = new List<Expression>(loopBody.Expressions);

            var variableAssignment = variable.AssignTo(value);
            loopBodyExpressions.Insert(insertIndex, variableAssignment);

            loopBody = loopBody.Update(loopBody.Variables.Concat(variable), loopBodyExpressions);

            return loop.Update(loop.BreakLabel, loop.ContinueLabel, loopBody);
        }

        public static Expression GetCaseInsensitiveEquals(this Expression stringValue, Expression comparisonValue)
        {
            return Expression.Call(
                stringValue,
                _stringEqualsMethod,
                comparisonValue,
                StringComparison.OrdinalIgnoreCase.ToConstantExpression());
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
            => Expression.Equal(expression, ToDefaultExpression(expression.Type));

        public static Expression GetIsNotDefaultComparison(this Expression expression)
        {
            if (expression.Type.IsNullableType())
            {
                return Expression.Property(expression, "HasValue");
            }

            return Expression.NotEqual(expression, ToDefaultExpression(expression.Type));
        }

        public static Expression GetIndexAccess(this Expression indexedExpression, Expression indexValue)
        {
            if (indexedExpression.Type.IsArray)
            {
                return Expression.ArrayIndex(indexedExpression, indexValue);
            }

            var indexer = indexedExpression.Type
                .GetPublicInstanceProperties()
                .First(p =>
                    p.GetIndexParameters().HasOne() &&
                   (p.GetIndexParameters()[0].ParameterType == indexValue.Type));

            return Expression.MakeIndex(indexedExpression, indexer, new[] { indexValue });
        }

        public static Expression GetValueOrDefaultCall(this Expression nullableExpression)
        {
            var parameterlessGetValueOrDefault = nullableExpression.Type
                .GetPublicInstanceMethods()
                .First(m => (m.Name == "GetValueOrDefault") && !m.GetParameters().Any());

            return Expression.Call(nullableExpression, parameterlessGetValueOrDefault);
        }

        [DebuggerStepThrough]
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

        public static Expression WithToReadOnlyCollectionCall(this Expression enumerable, Type elementType)
        {
            var wrapperType = typeof(ReadOnlyCollectionWrapper<>).MakeGenericType(elementType);
            var toReadOnlyMethod = wrapperType.GetMethod("ToReadOnlyCollection");

            return GetToEnumerableCall(enumerable, toReadOnlyMethod, elementType);
        }

        public static Expression WithToListCall(this Expression enumerable, Type elementType)
            => GetToEnumerableCall(enumerable, _linqToListMethod, elementType);

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
                return GetEmptyArray(elementType);
            }

            var typeHelper = new EnumerableTypeHelper(enumerableType, elementType);

            if (typeHelper.IsEnumerableInterface)
            {
                return Expression.Field(null, _typedEnumerable.MakeGenericType(elementType), "Empty");
            }

            if (typeHelper.IsReadOnlyCollection)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                return Expression.New(
                    typeHelper.ReadOnlyCollectionType.GetConstructor(new[] { typeHelper.ListInterfaceType }),
                    GetEmptyArray(elementType));
            }

            var fallbackType = typeHelper.IsCollection
                ? typeHelper.CollectionType
                : typeHelper.IsDictionary
                    ? GetDictionaryType(typeHelper.EnumerableType)
                    : typeHelper.ListType;

            return Expression.New(fallbackType);
        }

        private static Expression GetEmptyArray(Type elementType)
             => Expression.Field(null, _typedEnumerable.MakeGenericType(elementType), "EmptyArray");

        private static Type GetDictionaryType(Type dictionaryType)
        {
            return dictionaryType.IsInterface()
                ? typeof(Dictionary<,>).MakeGenericType(dictionaryType.GetGenericArguments())
                : dictionaryType;
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
