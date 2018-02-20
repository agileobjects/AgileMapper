﻿namespace AgileObjects.AgileMapper.Extensions.Internal
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
        private static readonly MethodInfo _listToArrayMethod;
        private static readonly MethodInfo _collectionToArrayMethod;
        private static readonly MethodInfo _linqToArrayMethod;
        private static readonly MethodInfo _linqToListMethod;
        private static readonly MethodInfo _stringEqualsMethod;

        static ExpressionExtensions()
        {
            var toArrayExtensionMethods = typeof(EnumerableExtensions).GetPublicStaticMethods("ToArray").ToArray();
            _listToArrayMethod = toArrayExtensionMethods.First();
            _collectionToArrayMethod = toArrayExtensionMethods.ElementAt(1);

            var linqEnumerableMethods = typeof(Enumerable).GetPublicStaticMethods().ToArray();
            _linqToArrayMethod = linqEnumerableMethods.First(m => m.Name == "ToArray");
            _linqToListMethod = linqEnumerableMethods.First(m => m.Name == "ToList");

            _stringEqualsMethod = typeof(string)
                .GetPublicStaticMethod("Equals", parameterCount: 3);
        }

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

        public static Expression GetIsDefaultComparison(this Expression expression)
            => Expression.Equal(expression, ToDefaultExpression(expression.Type));

        public static Expression GetIsNotDefaultComparison(this Expression expression)
        {
            if (expression.Type.IsNullableType())
            {
                return Expression.Property(expression, "HasValue");
            }

            var typeDefault = expression.Type.ToDefaultExpression();

            if (!expression.Type.IsValueType() || !expression.Type.IsComplex())
            {
                return Expression.NotEqual(expression, typeDefault);
            }

            var objectEquals = typeof(object).GetPublicStaticMethod("Equals");

            var objectEqualsCall = Expression.Call(
                null,
                objectEquals,
                expression.GetConversionToObject(),
                typeDefault.GetConversionToObject());

            return Expression.IsFalse(objectEqualsCall);
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

        public static Expression GetCount(
            this Expression collectionAccess,
            Type countType = null,
            Func<Expression, Type> collectionInterfaceTypeFactory = null)
        {
            if (collectionAccess.Type.IsArray)
            {
                return countType == typeof(long)
                    ? Expression.Property(collectionAccess, "LongLength")
                    : (Expression)Expression.ArrayLength(collectionAccess);
            }

            var countProperty = collectionAccess.Type.GetPublicInstanceProperty("Count");

            if (countProperty != null)
            {
                return Expression.Property(collectionAccess, countProperty);
            }

            if (collectionInterfaceTypeFactory == null)
            {
                collectionInterfaceTypeFactory = exp => typeof(ICollection<>)
                    .MakeGenericType(exp.Type.GetEnumerableElementType());
            }

            countProperty = collectionInterfaceTypeFactory
                .Invoke(collectionAccess)
                .GetPublicInstanceProperty("Count");

            return Expression.Property(collectionAccess, countProperty);
        }

        public static Expression GetValueOrDefaultCall(this Expression nullableExpression)
        {
            var parameterlessGetValueOrDefault = nullableExpression.Type
                .GetPublicInstanceMethods()
                .First(m => (m.Name == "GetValueOrDefault") && !m.GetParameters().Any());

            return Expression.Call(nullableExpression, parameterlessGetValueOrDefault);
        }

        [DebuggerStepThrough]
        public static Expression GetConversionToObject(this Expression expression)
            => GetConversionTo<object>(expression);

        [DebuggerStepThrough]
        public static Expression GetConversionTo<T>(this Expression expression)
            => GetConversionTo(expression, typeof(T));

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

        public static bool IsLinqToArrayOrToListCall(this MethodCallExpression call)
        {
            return call.Method.IsStatic && call.Method.IsGenericMethod &&
                  (ReferenceEquals(call.Method, _linqToListMethod) ||
                   ReferenceEquals(call.Method, _linqToArrayMethod));
        }

        public static Expression WithToArrayLinqCall(this Expression enumerable, Type elementType)
            => GetToEnumerableCall(enumerable, _linqToArrayMethod, elementType);

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

        [DebuggerStepThrough]
        public static MethodCallExpression WithToStringCall(this Expression value)
            => Expression.Call(value, value.Type.GetPublicInstanceMethod("ToString", parameterCount: 0));

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

        public static Expression WithToListLinqCall(this Expression enumerable, Type elementType)
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

        public static Expression GetEmptyInstanceCreation(
            this Type enumerableType,
            Type elementType,
            EnumerableTypeHelper typeHelper = null)
        {
            if (enumerableType.IsArray)
            {
                return GetEmptyArray(elementType);
            }

            if (enumerableType.IsValueType())
            {
                return Expression.New(enumerableType);
            }

            if (typeHelper == null)
            {
                typeHelper = new EnumerableTypeHelper(enumerableType, elementType);
            }

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
