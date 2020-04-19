namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching;
    using Caching.Dictionaries;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal static partial class ExpressionExtensions
    {
        public static Expression ReplaceParameterWith(this LambdaExpression lambda, Expression replacement)
            => ReplaceParameter(lambda.Body, lambda.Parameters[0], replacement);

        public static TExpression ReplaceParameter<TExpression>(
            this TExpression expression,
            Expression parameter,
            Expression replacement,
            int? replacementCount = null)
            where TExpression : Expression
        {
            return new ParameterReplacer(parameter, replacement, replacementCount).ReplaceIn(expression);
        }

        public static TExpression Replace<TExpression>(
            this TExpression expression,
            Expression target,
            Expression replacement,
            IEqualityComparer<Expression> comparer = null,
            int? replacementCount = null)
            where TExpression : Expression
        {
            if (target == replacement)
            {
                return expression;
            }

            if (expression == null)
            {
                return null;
            }

            if (target.NodeType == Parameter)
            {
                return ReplaceParameter(expression, target, replacement, replacementCount);
            }

            return new ExpressionReplacer(
                    target,
                    replacement,
                    comparer ?? ReferenceEqualsComparer<Expression>.Default,
                    replacementCount)
                .Replace<TExpression>(expression);
        }

        public static TExpression Replace<TExpression>(
            this TExpression expression,
            ISimpleDictionary<Expression, Expression> replacementsByTarget)
            where TExpression : Expression
        {
            if ((expression == null) || replacementsByTarget.None)
            {
                return expression;
            }

            var replacer = replacementsByTarget.HasOne
                ? new ExpressionReplacer(
                    replacementsByTarget.Keys.First(),
                    replacementsByTarget.Values.First(),
                    replacementsByTarget.Comparer)
                : new ExpressionReplacer(replacementsByTarget);

            return replacer.Replace<TExpression>(expression);
        }

        private class ParameterReplacer : QuickUnwindExpressionVisitor
        {
            private readonly Expression _parameterToReplace;
            private readonly Expression _replacement;
            private int _replacementCount;

            public ParameterReplacer(
                Expression parameterToReplace,
                Expression replacement,
                int? replacementCount)
            {
                _parameterToReplace = parameterToReplace;
                _replacement = replacement;
                _replacementCount = replacementCount ?? int.MaxValue;
            }

            protected override bool QuickUnwind => _replacementCount == 0;

            public TExpression ReplaceIn<TExpression>(TExpression expression)
                where TExpression : Expression
            {
                return VisitAndConvert(expression, nameof(ParameterReplacer));
            }

            protected override Expression VisitParameter(ParameterExpression parameter)
            {
                if (parameter != _parameterToReplace)
                {
                    return parameter;
                }

                --_replacementCount;
                return _replacement;
            }
        }

        private class ExpressionReplacer
        {
            private readonly ISimpleDictionary<Expression, Expression> _replacementsByTarget;
            private readonly bool _hasDictionary;
            private readonly Expression _target;
            private readonly Expression _replacement;
            private readonly IEqualityComparer<Expression> _comparer;
            private int _replacementCount;

            public ExpressionReplacer(ISimpleDictionary<Expression, Expression> replacementsByTarget)
                : this(replacementCount: null)
            {
                _replacementsByTarget = replacementsByTarget;
                _hasDictionary = true;
            }

            public ExpressionReplacer(
                Expression target,
                Expression replacement,
                IEqualityComparer<Expression> comparer,
                int? replacementCount = null)
                : this(replacementCount)
            {
                _target = target;
                _replacement = replacement;
                _comparer = comparer;
            }

            private ExpressionReplacer(int? replacementCount)
                => _replacementCount = replacementCount ?? int.MaxValue;

            public TExpression Replace<TExpression>(Expression expression)
                where TExpression : Expression
            {
                return (TExpression)ReplaceIn(expression);
            }

            private Expression ReplaceIn(Expression expression)
            {
                if (_replacementCount == 0)
                {
                    return expression;
                }

                switch (expression.NodeType)
                {
                    case Add:
                    case And:
                    case AndAlso:
                    case Assign:
                    case Coalesce:
                    case Divide:
                    case Equal:
                    case ExclusiveOr:
                    case GreaterThan:
                    case GreaterThanOrEqual:
                    case LessThan:
                    case LessThanOrEqual:
                    case Modulo:
                    case Multiply:
                    case NotEqual:
                    case Or:
                    case OrElse:
                    case Power:
                    case Subtract:
                        return ReplaceIn((BinaryExpression)expression);

                    case Block:
                        return ReplaceIn((BlockExpression)expression);

                    case Call:
                        return ReplaceIn((MethodCallExpression)expression);

                    case Conditional:
                        return ReplaceIn((ConditionalExpression)expression);

                    case ArrayLength:
                    case ExpressionType.Convert:
                    case Decrement:
                    case Increment:
                    case IsFalse:
                    case IsTrue:
                    case ExpressionType.Negate:
                    case Not:
                    case Throw:
                    case TypeAs:
                    case Unbox:
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

                    case ArrayIndex:
                        break;

                    case NewArrayBounds:
                        break;
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
                    b => b.Update(ReplaceIn(b.Variables), ReplaceIn(b.Expressions)));
            }

            private Expression ReplaceIn(MethodCallExpression call)
                => ReplaceIn(call, cl => ReplaceInCall(cl.Object, cl.Arguments, cl.Update));

            private Expression ReplaceIn(UnaryExpression unary) => ReplaceIn(unary, un => un.Update(Replace(un.Operand)));

            private Expression ReplaceIn(GotoExpression @goto) => ReplaceIn(@goto, gt => gt.Update(gt.Target, Replace(gt.Value)));

            private Expression ReplaceIn(IndexExpression indexAccess)
                => ReplaceIn(indexAccess, idx => idx.Update(Replace(idx.Object), ReplaceIn(idx.Arguments)));

            private Expression ReplaceIn(InvocationExpression invocation)
                => ReplaceIn(invocation, inv => ReplaceInCall(inv.Expression, inv.Arguments, inv.Update));

            private Expression ReplaceInCall(
                Expression subject,
                IList<Expression> arguments,
                Func<Expression, IEnumerable<Expression>, Expression> replacer)
            {
                return replacer.Invoke(Replace(subject), ReplaceIn(arguments));
            }

            private Expression ReplaceIn(LabelExpression label)
                => ReplaceIn(label, l => l.Update(l.Target, Replace(l.DefaultValue)));

            private Expression ReplaceIn(LambdaExpression lambda)
                => ReplaceIn(lambda, l => Expression.Lambda(l.Type, Replace(l.Body), ReplaceIn(l.Parameters)));

            private Expression ReplaceIn(MemberExpression memberAccess) => ReplaceIn(memberAccess, ma => ma.Update(Replace(ma.Expression)));

            private Expression ReplaceIn(MemberInitExpression memberInit)
                => ReplaceIn(memberInit, mi => mi.Update(ReplaceInNew(mi.NewExpression), ReplaceIn(mi.Bindings)));

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
                        return memberBinding.Update(ReplaceIn(memberBinding.Bindings));
                }

                throw new ArgumentOutOfRangeException();
            }

            private IEnumerable<ElementInit> ReplaceIn(IList<ElementInit> initializers)
                => ReplaceIn(initializers, init => init.Update(ReplaceIn(init.Arguments)));

            private Expression ReplaceIn(NewExpression newing) => ReplaceIn(newing, ReplaceInNew);

            private NewExpression ReplaceInNew(NewExpression newing)
            {
                return newing.Arguments.None()
                    ? newing
                    : newing.Update(ReplaceIn(newing.Arguments));
            }

            private Expression ReplaceIn(NewArrayExpression newArray) => ReplaceIn(newArray, na => na.Update(ReplaceIn(na.Expressions)));

            private ParameterExpression ReplaceIn(ParameterExpression parameter) => (ParameterExpression)ReplaceIn(parameter, p => p);

            private Expression Replace(Expression expression) => ReplaceIn(expression, ReplaceIn);

            private Expression ReplaceIn(TypeBinaryExpression typeBinary) => ReplaceIn(typeBinary, tb => tb.Update(Replace(typeBinary.Expression)));

            private Expression ReplaceIn(TryExpression @try)
            {
                return ReplaceIn(
                    @try,
                    t => t.Update(Replace(t.Body), ReplaceIn(t.Handlers, ReplaceIn), Replace(t.Finally), Replace(t.Fault)));
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

                if ((_replacementCount == 0) || (expression.NodeType == Default))
                {
                    return expression;
                }

                if (_hasDictionary)
                {
                    if (_replacementsByTarget.TryGetValue(expression, out var replacement))
                    {
                        --_replacementCount;
                        return replacement;
                    }
                }
                else if (_comparer.Equals(_target, expression))
                {
                    --_replacementCount;
                    return _replacement;
                }

                return replacer.Invoke(expression);
            }

            private IEnumerable<ParameterExpression> ReplaceIn(IList<ParameterExpression> parameters)
                => ReplaceIn(parameters, ReplaceIn);

            private IEnumerable<Expression> ReplaceIn(IList<Expression> expressions)
                => ReplaceIn(expressions, Replace);

            private IEnumerable<MemberBinding> ReplaceIn(IList<MemberBinding> bindings)
                => ReplaceIn(bindings, ReplaceIn);

            private IEnumerable<TItem> ReplaceIn<TItem>(IList<TItem> items, Func<TItem, TItem> replacer)
                where TItem : class
            {
                var itemCount = items.Count;

                IList<TItem> replacedItems;
                TItem item, replacedItem;

                switch (itemCount)
                {
                    case 0:
                        return items;

                    case 1:
                        item = items[0];
                        replacedItem = replacer.Invoke(item);
                        return (replacedItem != item) ? new[] { replacedItem } : items;

                    default:
                        replacedItems = null;

                        for (var i = 0; i < itemCount; ++i)
                        {
                            item = items[i];
                            replacedItem = replacer.Invoke(item);

                            if (replacedItem == item)
                            {
                                if (replacedItems != null)
                                {
                                    replacedItems[i] = item;
                                }

                                continue;
                            }

                            if (replacedItems == null)
                            {
                                replacedItems = new TItem[itemCount];

                                for (var j = 0; j < i; ++j)
                                {
                                    replacedItems[j] = items[j];
                                }
                            }

                            replacedItems[i] = replacedItem;
                        }

                        return replacedItems ?? items;
                }
            }
        }
    }
}
