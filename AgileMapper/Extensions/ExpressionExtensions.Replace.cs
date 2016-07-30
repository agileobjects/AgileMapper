namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal static partial class ExpressionExtensions
    {
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

        public static Expression Replace(this Expression expression,
            Dictionary<Expression, Expression> replacementsByTarget)
        {
            if (replacementsByTarget.None())
            {
                return expression;
            }

            var replacer = new ExpressionReplacer(replacementsByTarget);
            var replaced = replacer.ReplaceIn(expression);

            return replaced;
        }

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

                    case ExpressionType.Conditional:
                        return ReplaceIn((ConditionalExpression)expression);

                    case ExpressionType.Convert:
                    case ExpressionType.Not:
                    case ExpressionType.TypeAs:
                        return ReplaceIn((UnaryExpression)expression);

                    case ExpressionType.Invoke:
                        return ReplaceIn((InvocationExpression)expression);

                    case ExpressionType.Lambda:
                        return ReplaceIn((LambdaExpression)expression);

                    case ExpressionType.ListInit:
                        return ReplaceIn((ListInitExpression)expression);

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

            private Expression ReplaceIn(ConditionalExpression conditional)
            {
                return ReplaceIn(
                    conditional,
                    () => conditional.Update(
                        Replace(conditional.Test),
                        Replace(conditional.IfTrue),
                        Replace(conditional.IfFalse)));
            }

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

            private Expression ReplaceIn(ListInitExpression listInit)
            {
                return ReplaceIn(
                    listInit,
                    () => listInit.Update(
                        ReplaceIn(listInit.NewExpression),
                        ReplaceIn(listInit.Initializers)));
            }

            private MemberBinding ReplaceIn(MemberBinding binding)
            {
                switch (binding.BindingType)
                {
                    case MemberBindingType.Assignment:
                        var assignment = (MemberAssignment)binding;
                        return assignment.Update(Replace(assignment.Expression));

                    case MemberBindingType.ListBinding:
                        var listBinding = (MemberListBinding)binding;
                        return listBinding.Update(ReplaceIn(listBinding.Initializers));

                    case MemberBindingType.MemberBinding:
                        var memberBinding = (MemberMemberBinding)binding;
                        return memberBinding.Update(memberBinding.Bindings.Select(ReplaceIn));
                }

                throw new ArgumentOutOfRangeException();
            }

            private IEnumerable<ElementInit> ReplaceIn(IEnumerable<ElementInit> initializers)
                => initializers.Select(init => init.Update(init.Arguments.Select(Replace)));

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
    }
}
