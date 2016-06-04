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

        public static Expression ReplaceParameterWith(this LambdaExpression lambda, Expression replacement)
            => ReplaceParametersWith(lambda, replacement);

        public static Expression ReplaceParametersWith(this LambdaExpression lambda, params Expression[] replacements)
        {
            var replacementsByParameter = lambda
                .Parameters
                .Cast<Expression>()
                .Select((p, i) => new { Parameter = p, Replacement = replacements[i] })
                .ToDictionary(d => d.Parameter, d => d.Replacement);

            return lambda.Body.Replace(replacementsByParameter);
        }

        public static Expression Replace(this Expression expression, Expression target, Expression replacement)
            => expression.Replace(new Dictionary<Expression, Expression> { [target] = replacement });

        public static Expression Replace(this Expression expression, Dictionary<Expression, Expression> replacementsByTarget)
            => new ExpressionReplacer(replacementsByTarget).ReplaceIn(expression);

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
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Divide:
                    case ExpressionType.Equal:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.Modulo:
                    case ExpressionType.Multiply:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.Subtract:
                        return ReplaceIn((BinaryExpression)expression);

                    case ExpressionType.Call:
                        return ReplaceIn((MethodCallExpression)expression);

                    case ExpressionType.Convert:
                    case ExpressionType.Not:
                    case ExpressionType.TypeAs:
                        return ReplaceIn((UnaryExpression)expression);

                    case ExpressionType.Invoke:
                        return ReplaceIn((InvocationExpression)expression);

                    case ExpressionType.Lambda:
                        return ReplaceIn((LambdaExpression)expression);

                    case ExpressionType.MemberAccess:
                        return ReplaceIn((MemberExpression)expression);

                    case ExpressionType.MemberInit:
                        return ReplaceIn((MemberInitExpression)expression);

                    case ExpressionType.New:
                        return ReplaceIn((NewExpression)expression);

                    case ExpressionType.NewArrayInit:
                        return ReplaceIn((NewArrayExpression)expression);

                    case ExpressionType.TypeIs:
                        return ReplaceIn((TypeBinaryExpression)expression);
                }

                return expression;
            }

            private Expression ReplaceIn(BinaryExpression binary)
                => ReplaceIn(binary, () => binary.Update(Replace(binary.Left), binary.Conversion, Replace(binary.Right)));

            private Expression ReplaceIn(LambdaExpression lambda)
            {
                return ReplaceIn(
                    lambda,
                    () => Expression.Lambda(
                        lambda.Type,
                        Replace(lambda.Body),
                        lambda.Parameters.Select(Replace).Cast<ParameterExpression>()));
            }

            private Expression ReplaceIn(MethodCallExpression call)
                => ReplaceIn(call, () => ReplaceInCall(call.Object, call.Arguments, call.Update));

            private Expression ReplaceIn(UnaryExpression unary)
                => ReplaceIn(unary, () => unary.Update(Replace(unary.Operand)));

            private Expression ReplaceIn(InvocationExpression invocation)
                => ReplaceIn(invocation, () => ReplaceInCall(invocation.Expression, invocation.Arguments, invocation.Update));

            private Expression ReplaceInCall(
                Expression subject,
                IEnumerable<Expression> arguments,
                Func<Expression, IEnumerable<Expression>, Expression> replacer)
            {
                return replacer.Invoke(Replace(subject), arguments.Select(Replace).ToArray());
            }

            private Expression ReplaceIn(MemberExpression memberAccess)
                => ReplaceIn(memberAccess, () => memberAccess.Update(Replace(memberAccess.Expression)));

            private Expression ReplaceIn(MemberInitExpression memberInit)
            {
                return ReplaceIn(
                    memberInit,
                    () => memberInit.Update(
                        ReplaceIn(memberInit.NewExpression),
                        memberInit.Bindings.Select(ReplaceIn)));
            }

            private MemberBinding ReplaceIn(MemberBinding binding)
            {
                switch (binding.BindingType)
                {
                    case MemberBindingType.Assignment:
                        var assignment = (MemberAssignment)binding;
                        return assignment.Update(ReplaceIn(assignment.Expression));

                    //case MemberBindingType.ListBinding:
                    //    var memberBinding = (MemberMemberBinding)binding;
                    //    break;
                    //case MemberBindingType.MemberBinding:
                    //    var listBinding = (MemberListBinding)binding;
                    //    break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private NewExpression ReplaceIn(NewExpression newing) => (NewExpression)ReplaceIn(newing, () => newing.Update(newing.Arguments.Select(Replace)));

            private Expression ReplaceIn(NewArrayExpression newArray) => ReplaceIn(newArray, () => newArray.Update(newArray.Expressions.Select(Replace)));

            private Expression ReplaceIn(TypeBinaryExpression typeBinary) => ReplaceIn(typeBinary, () => typeBinary.Update(Replace(typeBinary.Expression)));

            private Expression Replace(Expression expression) => ReplaceIn(expression, () => ReplaceIn(expression));

            private Expression ReplaceIn(Expression expression, Func<Expression> replacer)
            {
                if (expression == null)
                {
                    return null;
                }

                Expression replacement;

                return _replacementsByTarget.TryGetValue(expression, out replacement) ? replacement : replacer.Invoke();
            }
        }

        #endregion
    }
}
