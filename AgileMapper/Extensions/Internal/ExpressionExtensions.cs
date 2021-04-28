namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
    using ReadableExpressions.Translations;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;
#if NET35
    using LinqExp = System.Linq.Expressions;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal static partial class ExpressionExtensions
    {
        private static readonly MethodInfo _stringEqualsMethod;

        static ExpressionExtensions()
        {
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

        public static bool IsInvocation(this LambdaExpression lambda) => lambda.Body.NodeType == Invoke;

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
        public static ConditionalExpression ToIfFalseDefaultCondition(
            this Expression value,
            Expression condition,
            Expression defaultValue = null)
        {
            return Expression.Condition(
                condition,
                value,
                defaultValue ?? value.Type.ToDefaultExpression());
        }

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

            var loopVariables = loopBody.Variables.Contains(variable)
                ? (IList<ParameterExpression>)loopBody.Variables
                : loopBody.Variables.Append(variable);

            loopBody = loopBody.Update(loopVariables, loopBodyExpressions);

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
                return Expression.ArrayAccess(indexedExpression, indexValue);
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

        [DebuggerStepThrough]
        public static MethodCallExpression WithToStringCall(this Expression value)
        {
            var toStringMethodType = value.Type.IsInterface()
                ? typeof(object)
                : value.Type;

            return Expression.Call(value, toStringMethodType.GetPublicInstanceMethod("ToString", parameterCount: 0));
        }

        public static Expression GetToEnumerableCall(this Expression enumerable, MethodInfo method, Type elementType)
        {
            if (!method.IsGenericMethod)
            {
                return Expression.Call(enumerable, method);
            }

            var typedToEnumerableMethod = method.MakeGenericMethod(elementType);

            return Expression.Call(typedToEnumerableMethod, enumerable);
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

        public static Expression GetRootExpression(this Expression expression)
        {
            while (true)
            {
                var parent = expression.GetParentOrNull();

                if (parent == null)
                {
                    return expression;
                }

                expression = parent;
            }
        }

        public static Expression ToExpression(this IList<Expression> expressions)
            => expressions.HasOne() ? expressions.First() : Expression.Block(expressions);

        public static IList<Expression> GetMemberMappingExpressions(this IList<Expression> mappingExpressions)
            => mappingExpressions.Filter(IsMemberMapping).ToList();

        private static bool IsMemberMapping(Expression expression)
        {
            switch (expression.NodeType)
            {
                case Constant:
                    return false;

                case Call when (
                    IsCallTo(nameof(IObjectMappingDataUntyped.Register), expression) ||
                    IsCallTo(nameof(IObjectMappingDataUntyped.TryGet), expression)):

                    return false;

                case Assign when IsMapRepeatedCall(((BinaryExpression)expression).Right):
                    return false;
            }

            return true;
        }

        private static bool IsMapRepeatedCall(Expression expression)
        {
            return (expression.NodeType == Call) &&
                   IsCallTo(nameof(IObjectMappingDataUntyped.MapRepeated), expression);
        }

        private static bool IsCallTo(string methodName, Expression call)
            => ((MethodCallExpression)call).Method.Name == methodName;

        public static bool TryGetVariableAssignment(this IList<Expression> mappingExpressions, out BinaryExpression assignment)
        {
            if (mappingExpressions.TryFindMatch(exp => exp.NodeType == Assign, out var assignmentExpression))
            {
                assignment = (BinaryExpression)assignmentExpression;
                return true;
            }

            assignment = null;
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
    }
}
