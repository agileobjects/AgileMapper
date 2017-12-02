namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using NetStandardPolyfills;

    internal class ExpressionEquator : IEqualityComparer<Expression>
    {
        public static IEqualityComparer<Expression> Instance = new ExpressionEquator();

        public bool Equals(Expression x, Expression y)
        {
            while (true)
            {
                if (x.NodeType != y.NodeType)
                {
                    return false;
                }

                switch (x.NodeType)
                {
                    case ExpressionType.Call:
                        return AreEqual((MethodCallExpression)x, (MethodCallExpression)y);

                    case ExpressionType.Conditional:
                        return AreEqual((ConditionalExpression)x, (ConditionalExpression)y);

                    case ExpressionType.Constant:
                        return AreEqual((ConstantExpression)x, (ConstantExpression)y);

                    case ExpressionType.ArrayLength:
                    case ExpressionType.Convert:
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.Not:
                    case ExpressionType.TypeAs:
                        return AreEqual((UnaryExpression)x, (UnaryExpression)y);

                    case ExpressionType.Index:
                        return AreEqual((IndexExpression)x, (IndexExpression)y);

                    case ExpressionType.Lambda:
                        x = ((LambdaExpression)x).Body;
                        y = ((LambdaExpression)y).Body;
                        continue;

                    case ExpressionType.ListInit:
                        return AreEqual((ListInitExpression)x, (ListInitExpression)y);

                    case ExpressionType.MemberAccess:
                        return AreEqual((MemberExpression)x, (MemberExpression)y);

                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.ArrayIndex:
                    case ExpressionType.Coalesce:
                    case ExpressionType.Divide:
                    case ExpressionType.Equal:
                    case ExpressionType.ExclusiveOr:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LeftShift:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.Modulo:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.RightShift:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                        return AreEqual((BinaryExpression)x, (BinaryExpression)y);

                    case ExpressionType.MemberInit:
                        return AreEqual((MemberInitExpression)x, (MemberInitExpression)y);

                    case ExpressionType.New:
                        return AreEqual((NewExpression)x, (NewExpression)y);

                    case ExpressionType.NewArrayBounds:
                    case ExpressionType.NewArrayInit:
                        return AreEqual((NewArrayExpression)x, (NewArrayExpression)y);

                    case ExpressionType.Parameter:
                        return AreEqual((ParameterExpression)x, (ParameterExpression)y);

                    case ExpressionType.Quote:
                        x = ((UnaryExpression)x).Operand;
                        y = ((UnaryExpression)y).Operand;
                        continue;

                    case ExpressionType.TypeIs:
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

        private static bool AreEqual(ConstantExpression x, ConstantExpression y)
        {
            return ReferenceEquals(x.Value, y.Value) || x.Value.Equals(y.Value);
        }

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

        private static bool AreEqual(MemberExpression x, MemberExpression y)
        {
            if (ReferenceEquals(x.Member, y.Member))
            {
                return true;
            }

            // ReSharper disable once PossibleNullReferenceException
            return (x.Member.Name == y.Member.Name) &&
                    y.Member.DeclaringType.IsAssignableFrom(x.Member.DeclaringType);
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