namespace AgileObjects.AgileMapper.Extensions
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class ExpressionEquator : IEqualityComparer<Expression>
    {
        public static IEqualityComparer<Expression> Instance = new ExpressionEquator();

        public bool Equals(Expression x, Expression y)
        {
            if (x.NodeType != y.NodeType)
            {
                return false;
            }

            switch (x.NodeType)
            {
                case ExpressionType.Index:
                    return AreEqual((IndexExpression)x, (IndexExpression)y);


                case ExpressionType.MemberAccess:
                    return AreEqual((MemberExpression)x, (MemberExpression)y);
            }

            return false;
        }

        private bool AreEqual(IndexExpression x, IndexExpression y)
        {
            if (!ReferenceEquals(x.Indexer, y.Indexer))
            {
                return false;
            }

            var xIndex = (ConstantExpression)x.Arguments[0];
            var yIndex = (ConstantExpression)y.Arguments[0];

            return xIndex.Value.Equals(yIndex.Value);
        }

        private bool AreEqual(MemberExpression x, MemberExpression y)
        {
            if (ReferenceEquals(x.Member, y.Member))
            {
                return true;
            }

            if (x.Member.Name != y.Member.Name)
            {
                return false;
            }

            // ReSharper disable once PossibleNullReferenceException
            return y.Member.DeclaringType.IsAssignableFrom(x.Member.DeclaringType);
        }

        public int GetHashCode(Expression obj) => 0;
    }
}