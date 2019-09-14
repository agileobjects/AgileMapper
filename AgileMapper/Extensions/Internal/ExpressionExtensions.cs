﻿namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
    using ReadableExpressions.Translations;
    using LinqExp = System.Linq.Expressions;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif
    using System.Reflection;
    using Members;
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
            var toArrayExtensionMethods = typeof(PublicEnumerableExtensions).GetPublicStaticMethods("ToArray").ToArray();
            _listToArrayMethod = toArrayExtensionMethods.First();
            _collectionToArrayMethod = toArrayExtensionMethods.Last();

            var linqEnumerableMethods = typeof(Enumerable).GetPublicStaticMethods().ToArray();
            _linqToArrayMethod = linqEnumerableMethods.First(m => m.Name == "ToArray");
            _linqToListMethod = linqEnumerableMethods.First(m => m.Name == "ToList");

            _stringEqualsMethod = typeof(string)
                .GetPublicStaticMethod("Equals", parameterCount: 3);
        }

        [DebuggerStepThrough]
        public static BinaryExpression AssignTo(this Expression subject, Expression value)
            => Expression.Assign(subject, value);

        public static bool IsNullableHasValueAccess(this Expression expression)
            => (expression.NodeType == MemberAccess) && IsNullableHasValueAccess((MemberExpression)expression);

        public static bool IsNullableHasValueAccess(this MemberExpression memberAccess)
        {
            return (memberAccess.Expression != null) &&
                   (memberAccess.Member.Name == "HasValue") &&
                   (memberAccess.Expression.Type.IsNullableType());
        }

        public static Expression Negate(this Expression expression)
            => (expression.NodeType != Not) ? Expression.Not(expression) : ((UnaryExpression)expression).Operand;

        [DebuggerStepThrough]
        public static ConstantExpression ToConstantExpression<T>(this T item)
            => ToConstantExpression(item, typeof(T));

        [DebuggerStepThrough]
        public static ConstantExpression ToConstantExpression<TItem>(this TItem item, Type type)
            => Expression.Constant(item, type);

        [DebuggerStepThrough]
        public static DefaultExpression ToDefaultExpression(this Type type) => Expression.Default(type);

        [DebuggerStepThrough]
        public static ConditionalExpression ToIfFalseDefaultCondition(this Expression value, Expression condition)
            => Expression.Condition(condition, value, value.Type.ToDefaultExpression());

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
            var loopBodyExpressions = new Expression[loopBody.Expressions.Count + 1];
            var expressionOffset = 0;

            for (var i = 0; i < loopBodyExpressions.Length; i++)
            {
                if (i != insertIndex)
                {
                    loopBodyExpressions[i] = loopBody.Expressions[i - expressionOffset];
                    continue;
                }

                loopBodyExpressions[i] = variable.AssignTo(value);
                expressionOffset = 1;
            }

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
                return GetNullableHasValueAccess(expression);
            }

            var typeDefault = expression.Type.ToDefaultExpression();

            return Expression.NotEqual(expression, typeDefault);
        }

        public static Expression GetNullableHasValueAccess(this Expression expression)
            => Expression.Property(expression, "HasValue");

        public static Expression GetNullableValueAccess(this Expression nullableExpression)
            => Expression.Property(nullableExpression, "Value");

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
                if (countType == typeof(long))
                {
                    var longLength = collectionAccess.Type.GetPublicInstanceProperty("LongLength");

                    if (longLength != null)
                    {
                        return Expression.Property(collectionAccess, longLength);
                    }
                }

                return Expression.ArrayLength(collectionAccess);
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

            var collectionType = collectionInterfaceTypeFactory.Invoke(collectionAccess);

            if (collectionAccess.Type.IsAssignableTo(collectionType))
            {
                return Expression.Property(
                    collectionAccess,
                    collectionType.GetPublicInstanceProperty("Count"));
            }

            var linqCountMethodName = (countType == typeof(long))
                ? nameof(Enumerable.LongCount)
                : nameof(Enumerable.Count);

            var linqCountMethod = typeof(Enumerable)
                .GetPublicStaticMethod(linqCountMethodName, parameterCount: 1)
                .MakeGenericMethod(collectionAccess.Type.GetEnumerableElementType());

            if (collectionAccess.Type.IsAssignableTo(linqCountMethod.GetParameters().First().ParameterType))
            {
                return Expression.Call(linqCountMethod, collectionAccess);
            }

            return null;
        }

        public static Expression GetValueOrDefaultCall(this Expression nullableExpression)
        {
            var parameterlessGetValueOrDefault = nullableExpression.Type
                .GetPublicInstanceMethod("GetValueOrDefault", parameterCount: 0);

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

            if ((targetType == typeof(object)) && expression.Type.IsValueType())
            {
                return Expression.Convert(expression, typeof(object));
            }

            if (expression.Type.GetNonNullableType() == targetType)
            {
                return expression.GetValueOrDefaultCall();
            }

            return Expression.Convert(expression, targetType);
        }

        public static Expression WithOrderingLinqCall(
            this Expression enumerable,
            string orderingMethodName,
            ParameterExpression element,
            Expression orderMemberAccess)
        {
            var funcTypes = new[] { element.Type, orderMemberAccess.Type };

            var orderingMethod = typeof(Enumerable)
                .GetPublicStaticMethod(orderingMethodName, parameterCount: 2)
                .MakeGenericMethod(funcTypes);

            var orderLambda = Expression.Lambda(
                Expression.GetFuncType(funcTypes),
                orderMemberAccess,
                element);

            return Expression.Call(orderingMethod, enumerable, orderLambda);
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
        {
            var toStringMethodType = value.Type.IsInterface()
                ? typeof(object)
                : value.Type;

            return Expression.Call(value, toStringMethodType.GetPublicInstanceMethod("ToString", parameterCount: 0));
        }

        public static Expression GetReadOnlyCollectionCreation(this Expression enumerable, Type elementType)
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

        public static Expression GetCollectionTypeCreation(this Expression enumerable, Type elementType)
        {
            var typeHelper = new EnumerableTypeHelper(enumerable.Type, elementType);

            if (typeHelper.HasListInterface)
            {
                return GetCollectionTypeCreation(typeHelper, enumerable.GetConversionTo(typeHelper.ListInterfaceType));
            }

            var nonListToArrayMethod = GetNonListToArrayConversionMethod(typeHelper);
            var toArrayCall = GetToEnumerableCall(enumerable, nonListToArrayMethod, typeHelper.ElementType);

            return GetCollectionTypeCreation(typeHelper, toArrayCall);
        }

        private static Expression GetCollectionTypeCreation(EnumerableTypeHelper typeHelper, Expression list)
            => GetCopyListCollectionCreation(typeHelper, typeHelper.CollectionType, list);

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

            if (typeHelper.IsEnumerableOrQueryable)
            {
                return Expression.Field(null, typeof(Enumerable<>).MakeGenericType(elementType), "Empty");
            }

            if (typeHelper.IsReadOnlyCollection)
            {
                return GetReadOnlyCollectionCreation(typeHelper, GetEmptyArray(elementType));
            }

            return Expression.New(typeHelper.GetEmptyInstanceCreationFallbackType());
        }

        private static Expression GetEmptyArray(Type elementType)
             => Expression.Field(null, typeof(Enumerable<>).MakeGenericType(elementType), "EmptyArray");

        private static Expression GetReadOnlyCollectionCreation(EnumerableTypeHelper typeHelper, Expression list)
            => GetCopyListCollectionCreation(typeHelper, typeHelper.ReadOnlyCollectionType, list);

        private static Expression GetCopyListCollectionCreation(
            EnumerableTypeHelper typeHelper,
            Type collectonType,
            Expression list)
        {
            var copyListConstructor = collectonType.GetPublicInstanceConstructor(typeHelper.ListInterfaceType);

            return (copyListConstructor != null)
                ? Expression.New(copyListConstructor, list)
                : Expression.New(collectonType);
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

        public static bool TryGetVariableAssignment(this IList<Expression> mappingExpressions, out BinaryExpression binaryExpression)
        {
            if (mappingExpressions.TryFindMatch(exp => exp.NodeType == Assign, out var assignment))
            {
                binaryExpression = (BinaryExpression)assignment;
                return true;
            }

            binaryExpression = null;
            return false;
        }
#if NET35
        public static LambdaExpression ToDlrExpression(this LinqExp.LambdaExpression linqLambda)
            => LinqExpressionToDlrExpressionConverter.Convert(linqLambda);

        public static Expression<TDelegate> ToDlrExpression<TDelegate>(this LinqExp.Expression<TDelegate> linqLambda)
            => (Expression<TDelegate>)LinqExpressionToDlrExpressionConverter.Convert(linqLambda);

        public static Expression ToDlrExpression(this LinqExp.Expression linqExpression)
            => LinqExpressionToDlrExpressionConverter.Convert(linqExpression);
#endif
        public static TryExpression WrapInTryCatch(this Expression mapping, IMemberMapperData mapperData)
        {
            var configuredCallback = mapperData.MapperContext.UserConfigurations.GetExceptionCallbackOrNull(mapperData);
            var exceptionVariable = Parameters.Create<Exception>("ex");

            if (configuredCallback == null)
            {
                var catchBody = Expression.Throw(
                    MappingException.GetFactoryMethodCall(mapperData, exceptionVariable),
                    mapping.Type);

                return CreateTryCatch(mapping, exceptionVariable, catchBody);
            }

            var configuredCatchBody = configuredCallback
                .ToCatchBody(exceptionVariable, mapping.Type, mapperData);

            return CreateTryCatch(mapping, exceptionVariable, configuredCatchBody);
        }

        private static TryExpression CreateTryCatch(
            Expression mappingBlock,
            ParameterExpression exceptionVariable,
            Expression catchBody)
        {
            var catchBlock = Expression.Catch(exceptionVariable, catchBody);

            return Expression.TryCatch(mappingBlock, catchBlock);
        }
    }
}
