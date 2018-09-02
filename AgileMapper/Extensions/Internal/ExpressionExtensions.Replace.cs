namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Caching;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal static partial class ExpressionExtensions
    {
        public static Expression ReplaceParametersWith(this LambdaExpression lambda, params Expression[] replacements)
        {
            if (lambda.Parameters.HasOne())
            {
                return lambda.ReplaceParameterWith(replacements.First());
            }

            var replacementsByParameter = lambda
                .Parameters
                .Project((p, i) => new { Parameter = p, Replacement = replacements[i] })
                .ToDictionary(d => (Expression)d.Parameter, d => d.Replacement);

            return lambda.Body.Replace(replacementsByParameter);
        }

        public static Expression ReplaceParameterWith(this LambdaExpression lambda, Expression replacement)
            => ReplaceParameter(lambda.Body, lambda.Parameters[0], replacement);

        private static TExpression ReplaceParameter<TExpression>(
            TExpression expression,
            Expression parameter,
            Expression replacement)
            where TExpression : Expression
        {
            return new ParameterReplacer(parameter, replacement).ReplaceIn(expression);
        }

        public static TExpression Replace<TExpression>(
            this TExpression expression,
            Expression target,
            Expression replacement,
            IEqualityComparer<Expression> comparer = null)
            where TExpression : Expression
        {
            if (expression == null)
            {
                return null;
            }

            if (target.NodeType == Parameter)
            {
                return ReplaceParameter(expression, target, replacement);
            }

            return new ExpressionReplacer(
                    target,
                    replacement,
                    comparer ?? default(ReferenceEqualsComparer<Expression>))
                .Replace<TExpression>(expression);
        }

        public static TExpression Replace<TExpression>(
            this TExpression expression,
            Dictionary<Expression, Expression> replacementsByTarget)
            where TExpression : Expression
        {
            if ((expression == null) || replacementsByTarget.None())
            {
                return expression;
            }

            var replacer = replacementsByTarget.HasOne()
                ? new ExpressionReplacer(
                    replacementsByTarget.Keys.First(),
                    replacementsByTarget.Values.First(),
                    replacementsByTarget.Comparer)
                : new ExpressionReplacer(replacementsByTarget);

            return replacer.Replace<TExpression>(expression);
        }

        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly Expression _parameterToReplace;
            private readonly Expression _replacement;

            public ParameterReplacer(Expression parameterToReplace, Expression replacement)
            {
                _parameterToReplace = parameterToReplace;
                _replacement = replacement;
            }

            public TExpression ReplaceIn<TExpression>(TExpression expression)
                where TExpression : Expression
            {
                return VisitAndConvert(expression, nameof(ParameterReplacer));
            }

            protected override Expression VisitParameter(ParameterExpression parameter)
                => parameter == _parameterToReplace ? _replacement : parameter;
        }

        private class ExpressionReplacer
        {
            private readonly Dictionary<Expression, Expression> _replacementsByTarget;
            private readonly bool _hasDictionary;
            private readonly Expression _target;
            private readonly Expression _replacement;
            private readonly IEqualityComparer<Expression> _comparer;

            public ExpressionReplacer(Dictionary<Expression, Expression> replacementsByTarget)
            {
                _replacementsByTarget = replacementsByTarget;
                _hasDictionary = true;
            }

            public ExpressionReplacer(
                Expression target,
                Expression replacement,
                IEqualityComparer<Expression> comparer)
            {
                _target = target;
                _replacement = replacement;
                _comparer = comparer;
            }

            public TExpression Replace<TExpression>(Expression expression)
                where TExpression : Expression
            {
                return (TExpression)ReplaceIn(expression);
            }

            private Expression ReplaceIn(Expression expression)
            {
                switch (expression.NodeType)
                {
                    case Add:
                    case And:
                    case AndAlso:
                    case Assign:
                    case Coalesce:
                    case Divide:
                    case Equal:
                    case GreaterThan:
                    case GreaterThanOrEqual:
                    case LessThan:
                    case LessThanOrEqual:
                    case Modulo:
                    case Multiply:
                    case NotEqual:
                    case Or:
                    case OrElse:
                    case Subtract:
                        return ReplaceIn((BinaryExpression)expression);

                    case Block:
                        return ReplaceIn((BlockExpression)expression);

                    case Call:
                        return ReplaceIn((MethodCallExpression)expression);

                    case Conditional:
                        return ReplaceIn((ConditionalExpression)expression);

                    case ExpressionType.Convert:
                    case IsFalse:
                    case IsTrue:
                    case Not:
                    case Throw:
                    case TypeAs:
                        return ReplaceIn((UnaryExpression)expression);

                    case Goto:
                        return ReplaceIn((GotoExpression)expression);

                    case Index:
                        return ReplaceIn((IndexExpression)expression);

                    case Invoke:
                        return ReplaceIn((InvocationExpression)expression);

                    case Label:
                        return ReplaceIn((LabelExpression)expression);

                    case Lambda:
                        return ReplaceIn((LambdaExpression)expression);

                    case ListInit:
                        return ReplaceIn((ListInitExpression)expression);

                    case Loop:
                        return ReplaceIn((LoopExpression)expression);

                    case MemberAccess:
                        return ReplaceIn((MemberExpression)expression);

                    case MemberInit:
                        return ReplaceIn((MemberInitExpression)expression);

                    case New:
                        return ReplaceIn((NewExpression)expression);

                    case NewArrayInit:
                        return ReplaceIn((NewArrayExpression)expression);

                    case Parameter:
                        return ReplaceIn((ParameterExpression)expression);

                    case TypeIs:
                        return ReplaceIn((TypeBinaryExpression)expression);

                    case Try:
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
                    b => b.Update(b.Variables.Project(ReplaceIn), b.Expressions.Project(Replace)));
            }

            private Expression ReplaceIn(MethodCallExpression call)
                => ReplaceIn(call, cl => ReplaceInCall(cl.Object, cl.Arguments, cl.Update));

            private Expression ReplaceIn(UnaryExpression unary) => ReplaceIn(unary, un => un.Update(Replace(un.Operand)));

            private Expression ReplaceIn(GotoExpression @goto) => ReplaceIn(@goto, gt => gt.Update(gt.Target, Replace(gt.Value)));

            private Expression ReplaceIn(IndexExpression indexAccess)
                => ReplaceIn(indexAccess, idx => idx.Update(Replace(idx.Object), idx.Arguments.Project(Replace)));

            private Expression ReplaceIn(InvocationExpression invocation)
                => ReplaceIn(invocation, inv => ReplaceInCall(inv.Expression, inv.Arguments, inv.Update));

            private Expression ReplaceInCall(
                Expression subject,
                IEnumerable<Expression> arguments,
                Func<Expression, IEnumerable<Expression>, Expression> replacer)
            {
                return replacer.Invoke(Replace(subject), arguments.Project(Replace).ToArray());
            }

            private Expression ReplaceIn(LabelExpression label)
                => ReplaceIn(label, l => l.Update(l.Target, Replace(l.DefaultValue)));

            private Expression ReplaceIn(LambdaExpression lambda)
                => ReplaceIn(lambda, l => Expression.Lambda(l.Type, Replace(l.Body), l.Parameters.Project(ReplaceIn)));

            private Expression ReplaceIn(MemberExpression memberAccess) => ReplaceIn(memberAccess, ma => ma.Update(Replace(ma.Expression)));

            private Expression ReplaceIn(MemberInitExpression memberInit)
                => ReplaceIn(memberInit, mi => mi.Update(ReplaceInNew(mi.NewExpression), mi.Bindings.Project(ReplaceIn)));

            private Expression ReplaceIn(ListInitExpression listInit)
                => ReplaceIn(listInit, li => li.Update(ReplaceInNew(li.NewExpression), ReplaceIn(li.Initializers)));

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
                        return memberBinding.Update(memberBinding.Bindings.Project(ReplaceIn));
                }

                throw new ArgumentOutOfRangeException();
            }

            private IEnumerable<ElementInit> ReplaceIn(IEnumerable<ElementInit> initializers)
                => initializers.Project(init => init.Update(init.Arguments.Project(Replace)));

            private Expression ReplaceIn(NewExpression newing) => ReplaceIn(newing, ReplaceInNew);

            private NewExpression ReplaceInNew(NewExpression newing)
            {
                return newing.Arguments.None()
                    ? newing
                    : newing.Update(newing.Arguments.Project(Replace));
            }

            private Expression ReplaceIn(NewArrayExpression newArray) => ReplaceIn(newArray, na => na.Update(na.Expressions.Project(Replace)));

            private ParameterExpression ReplaceIn(ParameterExpression parameter) => (ParameterExpression)ReplaceIn(parameter, p => p);

            private Expression Replace(Expression expression) => ReplaceIn(expression, ReplaceIn);

            private Expression ReplaceIn(TypeBinaryExpression typeBinary) => ReplaceIn(typeBinary, tb => tb.Update(Replace(typeBinary.Expression)));

            private Expression ReplaceIn(TryExpression @try)
            {
                return ReplaceIn(
                    @try,
                    t => t.Update(Replace(t.Body), t.Handlers.Project(ReplaceIn), Replace(t.Finally), Replace(t.Fault)));
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

                if (expression.NodeType == Default)
                {
                    return expression;
                }

                if (_hasDictionary)
                {
                    if (_replacementsByTarget.TryGetValue(expression, out var replacement))
                    {
                        return replacement;
                    }
                }
                else if (_comparer.Equals(_target, expression))
                {
                    return _replacement;
                }

                return replacer.Invoke(expression);
            }
        }
    }
}
