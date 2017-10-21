namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

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

                    case ExpressionType.Constant:
                        return AreEqual((ConstantExpression)x, (ConstantExpression)y);

                    case ExpressionType.Index:
                        return AreEqual((IndexExpression)x, (IndexExpression)y);

                    case ExpressionType.Lambda:
                        x = ((LambdaExpression)x).Body;
                        y = ((LambdaExpression)y).Body;
                        continue;

                    case ExpressionType.MemberAccess:
                        return AreEqual((MemberExpression)x, (MemberExpression)y);

                    case ExpressionType.Add:
                    case ExpressionType.Equal:
                    case ExpressionType.Multiply:
                    case ExpressionType.NotEqual:
                        return AreEqual((BinaryExpression)x, (BinaryExpression)y);

                    case ExpressionType.NewArrayInit:
                        return AreEqual((NewArrayExpression)x, (NewArrayExpression)y);

                    case ExpressionType.Parameter:
                        return AreEqual((ParameterExpression)x, (ParameterExpression)y);

                    case ExpressionType.Quote:
                        x = ((UnaryExpression)x).Operand;
                        y = ((UnaryExpression)y).Operand;
                        continue;
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

        private static bool AreEqual(ConstantExpression x, ConstantExpression y)
        {
            return x.Value.Equals(y.Value);
        }

        private bool AreEqual(IndexExpression x, IndexExpression y)
        {
            return ReferenceEquals(x.Indexer, y.Indexer) &&
                   AllEqual(x.Arguments, y.Arguments);
        }

        private static bool AreEqual(MemberExpression x, MemberExpression y)
        {
            if (ReferenceEquals(x.Member, y.Member))
            {
                return true;
            }

            return (x.Member.Name == y.Member.Name) &&
                    y.Member.DeclaringType.IsAssignableFrom(x.Member.DeclaringType);
        }

        private bool AreEqual(BinaryExpression x, BinaryExpression y)
        {
            return ReferenceEquals(x.Method, y.Method) &&
                   Equals(x.Left, y.Left) && Equals(x.Right, y.Right);
        }

        private static bool AreEqual(ParameterExpression x, ParameterExpression y)
        {
            return (x.Type == y.Type) && (x.Name == y.Name);
        }

        private bool AreEqual(NewArrayExpression x, NewArrayExpression y)
        {
            return (x.Type == y.Type) && AllEqual(x.Expressions, y.Expressions);
        }

        public int GetHashCode(Expression obj) => 0;
    }
}