namespace AgileObjects.AgileMapper.Extensions.Internal
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
            .GetPublicStaticMethods("ToArray").First();

        private static readonly MethodInfo _collectionToArrayMethod = typeof(EnumerableExtensions)
            .GetPublicStaticMethods("ToArray").ElementAt(1);

        private static readonly MethodInfo _linqToArrayMethod = typeof(Enumerable)
            .GetPublicStaticMethod("ToArray");

        private static readonly MethodInfo _linqToListMethod = typeof(Enumerable)
            .GetPublicStaticMethod("ToList");

        private static readonly MethodInfo _stringEqualsMethod = typeof(string)
            .GetPublicStaticMethod("Equals", parameterCount: 3);

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

            loopBody = loopBody.Update(loopBody.Variables.Append(variable), loopBodyExpressions);

            return loop.Update(loop.BreakLabel, loop.ContinueLabel, loopBody);
        }

        public static Expression GetCaseInsensitiveEquals(this Expression stringValue, Expression comparisonValue)
        {
            return Expression.Call(
                _stringEqualsMethod,
                stringValue,
                comparisonValue,
                StringComparison.OrdinalIgnoreCase.ToConstantExpression());
        }

        public static Expression GetIsNotDefaultComparisonsOrNull(this IList<Expression> expressions)
        {
            if (expressions.None())
            {
                return null;
            }

            if (expressions.HasOne())
            {
                return expressions[0].GetIsNotDefaultComparison();
            }

            var notNullChecks = expressions
                .Select(exp => exp.GetIsNotDefaultComparison())
                .ToArray();

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

            var typeDefault = expression.Type.ToDefaultExpression();

            return Expression.NotEqual(expression, typeDefault);
        }

        public static Expression GetIndexAccess(this Expression indexedExpression, Expression indexValue)
        {
            if (indexedExpression.Type.IsArray)
            {
                return Expression.ArrayIndex(indexedExpression, indexValue);
            }

            var relevantTypes = new[] { indexedExpression.Type }
                .Concat(indexedExpression.Type.GetAllInterfaces());

            var indexer = relevantTypes
                .SelectMany(t => t.GetPublicInstanceProperties())
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
        {
            if (expression.Type == targetType)
            {
                return expression;
            }

            if (expression.Type.GetNonNullableType() == targetType)
            {
                return expression.GetValueOrDefaultCall();
            }

            return Expression.Convert(expression, targetType);
        }

        public static Expression WithToArrayCall(this Expression enumerable, Type elementType)
        {
            var conversionMethod = GetToArrayConversionMethod(enumerable, elementType);

            return GetToEnumerableCall(enumerable, conversionMethod, elementType);
        }

        private static MethodInfo GetToArrayConversionMethod(Expression enumerable, Type elementType)
        {
            var typeHelper = new EnumerableTypeHelper(enumerable.Type, elementType);

            if (TryGetWrapperMethod(typeHelper, "ToArray", out var method))
            {
                return method;
            }

            if (typeHelper.HasListInterface)
            {
                return _listToArrayMethod;
            }

            return GetNonListToArrayConversionMethod(typeHelper);
        }

        private static bool TryGetWrapperMethod(
            EnumerableTypeHelper typeHelper,
            string methodName,
            out MethodInfo method)
        {
            var wrapperType = typeHelper.WrapperType;

            if (typeHelper.EnumerableType != wrapperType)
            {
                method = null;
                return false;
            }

            method = wrapperType.GetPublicInstanceMethod(methodName);
            return true;
        }

        private static MethodInfo GetNonListToArrayConversionMethod(EnumerableTypeHelper typeHelper)
            => typeHelper.HasCollectionInterface ? _collectionToArrayMethod : _linqToArrayMethod;

        public static Expression WithToReadOnlyCollectionCall(this Expression enumerable, Type elementType)
        {
            var typeHelper = new EnumerableTypeHelper(enumerable.Type, elementType);

            if (TryGetWrapperMethod(typeHelper, "ToReadOnlyCollection", out var method))
            {

                return GetToEnumerableCall(enumerable, method, typeHelper.ElementType);
            }

            if (typeHelper.IsList)
            {
                return Expression.Call(enumerable, typeHelper.ListType.GetPublicInstanceMethod("AsReadOnly"));
            }

            if (typeHelper.HasListInterface)
            {
                return GetReadOnlyCollectionCreation(typeHelper, enumerable);
            }

            var nonListToArrayMethod = GetNonListToArrayConversionMethod(typeHelper);
            var toArrayCall = GetToEnumerableCall(enumerable, nonListToArrayMethod, typeHelper.ElementType);

            return GetReadOnlyCollectionCreation(typeHelper, toArrayCall);
        }

        public static Expression WithToCollectionCall(this Expression enumerable, Type elementType)
        {
            var typeHelper = new EnumerableTypeHelper(enumerable.Type, elementType);

            if (typeHelper.HasListInterface)
            {
                return GetCollectionCreation(typeHelper, enumerable.GetConversionTo(typeHelper.ListInterfaceType));
            }

            var nonListToArrayMethod = GetNonListToArrayConversionMethod(typeHelper);
            var toArrayCall = GetToEnumerableCall(enumerable, nonListToArrayMethod, typeHelper.ElementType);

            return GetCollectionCreation(typeHelper, toArrayCall);
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

        public static Expression GetEmptyInstanceCreation(this Type enumerableType, Type elementType)
        {
            if (enumerableType.IsArray)
            {
                return GetEmptyArray(elementType);
            }

            var typeHelper = new EnumerableTypeHelper(enumerableType, elementType);

            if (typeHelper.IsEnumerableInterface)
            {
                return Expression.Field(null, typeof(Enumerable<>).MakeGenericType(elementType), "Empty");
            }

            if (typeHelper.IsReadOnlyCollection)
            {
                return GetReadOnlyCollectionCreation(typeHelper, GetEmptyArray(elementType));
            }

            var fallbackType = typeHelper.IsCollection
                ? typeHelper.CollectionType
                : typeHelper.IsDictionary
                    ? GetDictionaryType(typeHelper.EnumerableType)
                    : typeHelper.ListType;

            return Expression.New(fallbackType);
        }

        private static Expression GetEmptyArray(Type elementType)
             => Expression.Field(null, typeof(Enumerable<>).MakeGenericType(elementType), "EmptyArray");

        private static Expression GetReadOnlyCollectionCreation(EnumerableTypeHelper typeHelper, Expression list)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return Expression.New(
                typeHelper.ReadOnlyCollectionType.GetPublicInstanceConstructor(typeHelper.ListInterfaceType),
                list);
        }

        private static Expression GetCollectionCreation(EnumerableTypeHelper typeHelper, Expression list)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return Expression.New(
                typeHelper.CollectionType.GetPublicInstanceConstructor(typeHelper.ListInterfaceType),
                list);
        }

        private static Type GetDictionaryType(Type dictionaryType)
        {
            return dictionaryType.IsInterface()
                ? typeof(Dictionary<,>).MakeGenericType(dictionaryType.GetGenericTypeArguments())
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
