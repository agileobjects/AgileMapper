namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Collections.Generic;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
    using LinqExp = System.Linq.Expressions;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal static class ExpressionEvaluation
    {
        private static readonly IEqualityComparer<Expression> _equator = new ExpressionEquator(MemberAccessesAreEqual);

        public static readonly IEqualityComparer<Expression> Equivalator = new ExpressionEquator((e, x, y) => MemberAccessesAreEquivalent(x, y));

        public static bool AreEqual(Expression x, Expression y) => _equator.Equals(x, y);

        public static bool AreEquivalent(Expression x, Expression y) => Equivalator.Equals(x, y);

        #region Member Access Evaluation

        private static bool MemberAccessesAreEqual(ExpressionEquator equator, MemberExpression x, MemberExpression y)
            => MemberAccessesAreEquivalent(x, y) && equator.Equals(x.Expression, y.Expression);

        public static bool MemberAccessesAreEquivalent(MemberExpression x, MemberExpression y)
        {
            if (ReferenceEquals(x.Member, y.Member))
            {
                return true;
            }

            // ReSharper disable once PossibleNullReferenceException
            return (x.Member.Name == y.Member.Name) &&
                    x.Member.DeclaringType.IsAssignableTo(y.Member.DeclaringType);
        }

        #endregion

        private class ExpressionEquator : IEqualityComparer<Expression>
        {
            private readonly Func<ExpressionEquator, MemberExpression, MemberExpression, bool> _memberAccessComparer;

            public ExpressionEquator(
                Func<ExpressionEquator, MemberExpression, MemberExpression, bool> memberAccessComparer)
            {
                _memberAccessComparer = memberAccessComparer;
            }

            public bool Equals(Expression x, Expression y)
            {
                if (x == y)
                {
                    return true;
                }

                while (true)
                {
                    // ReSharper disable PossibleNullReferenceException
                    if (x.NodeType != y.NodeType)
                    {
                        return false;
                    }
                    // ReSharper restore PossibleNullReferenceException

                    switch (x.NodeType)
                    {
                        case Block:
                            return AllEqual(((BlockExpression)x).Expressions, ((BlockExpression)y).Expressions);

                        case Call:
                            return AreEqual((MethodCallExpression)x, (MethodCallExpression)y);

                        case Conditional:
                            return AreEqual((ConditionalExpression)x, (ConditionalExpression)y);

                        case Constant:
#if NET35
                            return AreEqualNet35((ConstantExpression)x, (ConstantExpression)y);
#else
                            return AreEqual((ConstantExpression)x, (ConstantExpression)y);
#endif

                        case ArrayLength:
                        case ExpressionType.Convert:
                        case Negate:
                        case NegateChecked:
                        case Not:
                        case TypeAs:
                            return AreEqual((UnaryExpression)x, (UnaryExpression)y);

                        case Default:
                            return ((DefaultExpression)x).Type == ((DefaultExpression)y).Type;

                        case Index:
                            return AreEqual((IndexExpression)x, (IndexExpression)y);

                        case Lambda:
                            x = ((LambdaExpression)x).Body;
                            y = ((LambdaExpression)y).Body;
                            continue;

                        case ListInit:
                            return AreEqual((ListInitExpression)x, (ListInitExpression)y);

                        case MemberAccess:
                            return _memberAccessComparer.Invoke(this, (MemberExpression)x, (MemberExpression)y);

                        case Add:
                        case AddChecked:
                        case And:
                        case AndAlso:
                        case ArrayIndex:
                        case Coalesce:
                        case Divide:
                        case Equal:
                        case ExclusiveOr:
                        case GreaterThan:
                        case GreaterThanOrEqual:
                        case LeftShift:
                        case LessThan:
                        case LessThanOrEqual:
                        case Modulo:
                        case Multiply:
                        case MultiplyChecked:
                        case NotEqual:
                        case Or:
                        case OrElse:
                        case RightShift:
                        case Subtract:
                        case SubtractChecked:
                            return AreEqual((BinaryExpression)x, (BinaryExpression)y);

                        case MemberInit:
                            return AreEqual((MemberInitExpression)x, (MemberInitExpression)y);

                        case New:
                            return AreEqual((NewExpression)x, (NewExpression)y);

                        case NewArrayBounds:
                        case NewArrayInit:
                            return AreEqual((NewArrayExpression)x, (NewArrayExpression)y);

                        case Parameter:
                            return AreEqual((ParameterExpression)x, (ParameterExpression)y);

                        case Quote:
                            x = ((UnaryExpression)x).Operand;
                            y = ((UnaryExpression)y).Operand;
                            continue;

                        case TypeIs:
                            return AreEqual((TypeBinaryExpression)x, (TypeBinaryExpression)y);
                    }

                    throw new NotImplementedException("Unable to equate Expressions of type " + x.NodeType);
                }
            }

            private bool AllEqual(IList<Expression> xItems, IList<Expression> yItems)
            {
                if (xItems.Count != yItems.Count)
                {
                    return false;
                }

                for (var i = 0; i < xItems.Count; i++)
                {
                    if (!Equals(xItems[i], yItems[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool AreEqual(MethodCallExpression x, MethodCallExpression y)
            {
                return ReferenceEquals(x.Method, y.Method) &&
                       (((x.Object == null) && (y.Object == null)) || ((x.Object != null) && Equals(x.Object, y.Object))) &&
                       AllEqual(x.Arguments, y.Arguments);
            }

            private bool AreEqual(ConditionalExpression x, ConditionalExpression y)
            {
                return (x.Type == y.Type) && Equals(x.Test, y.Test) &&
                        Equals(x.IfTrue, y.IfTrue) && Equals(x.IfFalse, y.IfFalse);
            }
#if NET35
            private bool AreEqualNet35(ConstantExpression x, ConstantExpression y)
            {
                if (AreEqual(x, y))
                {
                    return true;
                }

                return (x.Type == y.Type) &&
                       (x.Value is LinqExp.Expression xExpression) &&
                        Equals(xExpression.ToDlrExpression(), ((LinqExp.Expression)y.Value).ToDlrExpression());
            }
#endif
            private static bool AreEqual(ConstantExpression x, ConstantExpression y)
                => ReferenceEquals(x.Value, y.Value) || x.Value.Equals(y.Value);

            private bool AreEqual(UnaryExpression x, UnaryExpression y)
            {
                return (x.Type == y.Type) && Equals(x.Operand, y.Operand);
            }

            private bool AreEqual(IndexExpression x, IndexExpression y)
            {
                return ReferenceEquals(x.Indexer, y.Indexer) &&
                       AllEqual(x.Arguments, y.Arguments);
            }

            private bool AreEqual(ListInitExpression x, ListInitExpression y)
            {
                return (x.Type == y.Type) && Equals(x.NewExpression, y.NewExpression) &&
                        AllEqual(x.Initializers, y.Initializers);
            }

            private bool AllEqual(IList<ElementInit> xInitializers, IList<ElementInit> yInitializers)
            {
                if (xInitializers.Count != yInitializers.Count)
                {
                    return false;
                }

                for (var i = 0; i < xInitializers.Count; i++)
                {
                    var x = xInitializers[i];
                    var y = yInitializers[i];

                    if (!ReferenceEquals(x.AddMethod, y.AddMethod) ||
                        !AllEqual(x.Arguments, y.Arguments))
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool AreEqual(BinaryExpression x, BinaryExpression y)
            {
                return ReferenceEquals(x.Method, y.Method) &&
                       Equals(x.Left, y.Left) && Equals(x.Right, y.Right);
            }

            private bool AreEqual(MemberInitExpression x, MemberInitExpression y)
            {
                return Equals(x.NewExpression, y.NewExpression) &&
                       AllEqual(x.Bindings, y.Bindings);
            }

            private bool AllEqual(IList<MemberBinding> xBindings, IList<MemberBinding> yBindings)
            {
                if (xBindings.Count != yBindings.Count)
                {
                    return false;
                }

                for (var i = 0; i < xBindings.Count; i++)
                {
                    var x = xBindings[i];
                    var y = yBindings[i];

                    if ((x.BindingType != y.BindingType) || !ReferenceEquals(x.Member, y.Member))
                    {
                        return false;
                    }

                    switch (x.BindingType)
                    {
                        case MemberBindingType.Assignment:
                            if (Equals(((MemberAssignment)x).Expression, ((MemberAssignment)y).Expression))
                            {
                                break;
                            }
                            return false;

                        case MemberBindingType.MemberBinding:
                            if (AllEqual(((MemberMemberBinding)x).Bindings, ((MemberMemberBinding)y).Bindings))
                            {
                                break;
                            }
                            return false;

                        case MemberBindingType.ListBinding:
                            if (AreEqual((MemberListBinding)x, (MemberListBinding)y))
                            {
                                break;
                            }
                            return false;
                    }
                }

                return true;
            }

            private bool AreEqual(MemberListBinding x, MemberListBinding y)
            {
                if (x.Initializers.Count != y.Initializers.Count)
                {
                    return false;
                }

                for (var i = 0; i < x.Initializers.Count; i++)
                {
                    var xInitialiser = x.Initializers[i];
                    var yInitialiser = y.Initializers[i];

                    if (!ReferenceEquals(xInitialiser.AddMethod, yInitialiser.AddMethod) ||
                        !AllEqual(xInitialiser.Arguments, yInitialiser.Arguments))
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool AreEqual(NewExpression x, NewExpression y)
            {
                return (x.Type == y.Type) && ReferenceEquals(x.Constructor, y.Constructor) &&
                        AllEqual(x.Arguments, y.Arguments);
            }

            private bool AreEqual(NewArrayExpression x, NewArrayExpression y)
            {
                return (x.Type == y.Type) && AllEqual(x.Expressions, y.Expressions);
            }

            private static bool AreEqual(ParameterExpression x, ParameterExpression y)
            {
                return (x.Type == y.Type) && (x.Name == y.Name);
            }

            private bool AreEqual(TypeBinaryExpression x, TypeBinaryExpression y)
                => (x.TypeOperand == y.TypeOperand) && Equals(x.Expression, y.Expression);

            public int GetHashCode(Expression obj) => 0;
        }
    }
}