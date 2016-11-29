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

        public static TExpression Replace<TExpression>(
            this TExpression expression,
            Expression target,
            Expression replacement)
            where TExpression : Expression
        {
            return expression.Replace(new Dictionary<Expression, Expression> { [target] = replacement });
        }

        public static TExpression Replace<TExpression>(
            this TExpression expression,
            Dictionary<Expression, Expression> replacementsByTarget)
            where TExpression : Expression
        {
            if (replacementsByTarget.None())
            {
                return expression;
            }

            var replacer = new ExpressionReplacer(replacementsByTarget);
            var replaced = replacer.ReplaceIn(expression);

            return (TExpression)replaced;
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
                    case ExpressionType.Assign:
                    case ExpressionType.Coalesce:
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

                    case ExpressionType.Block:
                        return ReplaceIn((BlockExpression)expression);

                    case ExpressionType.Call:
                        return ReplaceIn((MethodCallExpression)expression);

                    case ExpressionType.Conditional:
                        return ReplaceIn((ConditionalExpression)expression);

                    case ExpressionType.Convert:
                    case ExpressionType.Not:
                    case ExpressionType.Throw:
                    case ExpressionType.TypeAs:
                        return ReplaceIn((UnaryExpression)expression);

                    case ExpressionType.Goto:
                        return ReplaceIn((GotoExpression)expression);

                    case ExpressionType.Invoke:
                        return ReplaceIn((InvocationExpression)expression);

                    case ExpressionType.Lambda:
                        return ReplaceIn((LambdaExpression)expression);

                    case ExpressionType.ListInit:
                        return ReplaceIn((ListInitExpression)expression);

                    case ExpressionType.Loop:
                        return ReplaceIn((LoopExpression)expression);

                    case ExpressionType.MemberAccess:
                        return ReplaceIn((MemberExpression)expression);

                    case ExpressionType.MemberInit:
                        return ReplaceIn((MemberInitExpression)expression);

                    case ExpressionType.New:
                        return ReplaceIn((NewExpression)expression);

                    case ExpressionType.NewArrayInit:
                        return ReplaceIn((NewArrayExpression)expression);

                    case ExpressionType.Parameter:
                        return ReplaceIn((ParameterExpression)expression);

                    case ExpressionType.TypeIs:
                        return ReplaceIn((TypeBinaryExpression)expression);

                    case ExpressionType.Try:
                        return ReplaceIn((TryExpression)expression);
                }

                return expression;
            }

            private Expression ReplaceIn(BinaryExpression binary)
                => ReplaceIn(binary, b => b.Update(Replace(b.Left), b.Conversion, Replace(b.Right)));

            private Expression ReplaceIn(ConditionalExpression conditional)
            {
                return ReplaceIn(
                    conditional,
                    cnd => cnd.Update(Replace(cnd.Test), Replace(cnd.IfTrue), Replace(cnd.IfFalse)));
            }

            private Expression ReplaceIn(BlockExpression block)
            {
                return ReplaceIn(
                    block,
                    b => b.Update(b.Variables.Select(ReplaceIn), b.Expressions.Select(Replace)));
            }

            private Expression ReplaceIn(MethodCallExpression call)
                => ReplaceIn(call, cl => ReplaceInCall(cl.Object, cl.Arguments, cl.Update));

            private Expression ReplaceIn(UnaryExpression unary) => ReplaceIn(unary, un => un.Update(Replace(un.Operand)));

            private Expression ReplaceIn(GotoExpression @goto) => ReplaceIn(@goto, gt => gt.Update(gt.Target, Replace(gt.Value)));

            private Expression ReplaceIn(InvocationExpression invocation)
                => ReplaceIn(invocation, inv => ReplaceInCall(inv.Expression, inv.Arguments, inv.Update));

            private Expression ReplaceInCall(
                Expression subject,
                IEnumerable<Expression> arguments,
                Func<Expression, IEnumerable<Expression>, Expression> replacer)
            {
                return replacer.Invoke(Replace(subject), arguments.Select(Replace).ToArray());
            }

            private Expression ReplaceIn(LambdaExpression lambda)
                => ReplaceIn(lambda, l => Expression.Lambda(l.Type, Replace(l.Body), l.Parameters.Select(ReplaceIn)));

            private Expression ReplaceIn(MemberExpression memberAccess) => ReplaceIn(memberAccess, ma => ma.Update(Replace(ma.Expression)));

            private Expression ReplaceIn(MemberInitExpression memberInit)
                => ReplaceIn(memberInit, mi => mi.Update(ReplaceIn(mi.NewExpression), mi.Bindings.Select(ReplaceIn)));

            private Expression ReplaceIn(ListInitExpression listInit)
                => ReplaceIn(listInit, li => li.Update(ReplaceIn(li.NewExpression), ReplaceIn(li.Initializers)));

            private Expression ReplaceIn(LoopExpression loop) => ReplaceIn(loop, l => l.Update(l.BreakLabel, l.ContinueLabel, Replace(l.Body)));

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

            private NewExpression ReplaceIn(NewExpression newing) => (NewExpression)ReplaceIn(newing, nw => nw.Update(nw.Arguments.Select(Replace)));

            private Expression ReplaceIn(NewArrayExpression newArray) => ReplaceIn(newArray, na => na.Update(na.Expressions.Select(Replace)));

            private ParameterExpression ReplaceIn(ParameterExpression parameter) => (ParameterExpression)ReplaceIn(parameter, p => p);

            private Expression Replace(Expression expression) => ReplaceIn(expression, ReplaceIn);

            private Expression ReplaceIn(TypeBinaryExpression typeBinary) => ReplaceIn(typeBinary, tb => tb.Update(Replace(typeBinary.Expression)));

            private Expression ReplaceIn(TryExpression @try)
            {
                return ReplaceIn(
                    @try,
                    t => t.Update(Replace(t.Body), t.Handlers.Select(ReplaceIn), Replace(t.Finally), Replace(t.Fault)));
            }

            private CatchBlock ReplaceIn(CatchBlock @catch)
                => @catch.Update(ReplaceIn(@catch.Variable), Replace(@catch.Filter), Replace(@catch.Body));

            private Expression ReplaceIn<TExpression>(TExpression expression, Func<TExpression, Expression> replacer)
                where TExpression : Expression
            {
                if (expression == null)
                {
                    return null;
                }

                if (expression.NodeType == ExpressionType.Default)
                {
                    return expression;
                }

                Expression replacement;

                if (_replacementsByTarget.TryGetValue(expression, out replacement))
                {
                    return replacement;
                }

                return replacer.Invoke(expression);
            }
        }
    }
}
