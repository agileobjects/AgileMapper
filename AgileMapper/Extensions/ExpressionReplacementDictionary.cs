namespace AgileObjects.AgileMapper.Extensions
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Xml;

#if NET_STANDARD
    using System.Reflection;
#endif

    internal class ExpressionReplacementDictionary : Dictionary<Expression, Expression>
    {
        public ExpressionReplacementDictionary(int capacity)
            : base(capacity, EquivalentMemberAccessComparer.Instance)
        {
        }

        private class EquivalentMemberAccessComparer : IEqualityComparer<Expression>
        {
            public static readonly IEqualityComparer<Expression> Instance = new EquivalentMemberAccessComparer();

            public bool Equals(Expression x, Expression y)
            {
                // ReSharper disable PossibleNullReferenceException
                if (x.NodeType != y.NodeType)
                {
                    return false;
                }
                // ReSharper restore PossibleNullReferenceException

                if (x.NodeType == ExpressionType.Index)
                {
                    return AreIndexAccessesEqual((IndexExpression)x, (IndexExpression)y);
                }

                var memberAccessX = (MemberExpression)x;
                var memberAccessY = (MemberExpression)y;

                if (ReferenceEquals(memberAccessX.Member, memberAccessY.Member))
                {
                    return true;
                }

                if (memberAccessX.Member.Name != memberAccessY.Member.Name)
                {
                    return false;
                }

                // ReSharper disable once PossibleNullReferenceException
                return memberAccessY.Member.DeclaringType.IsAssignableFrom(memberAccessX.Member.DeclaringType);
            }

            private static bool AreIndexAccessesEqual(IndexExpression x, IndexExpression y)
            {
                if (x.Indexer != y.Indexer)
                {
                    return false;
                }

                var xIndex = (ConstantExpression)x.Arguments[0];
                var yIndex = (ConstantExpression)y.Arguments[0];

                return xIndex.Value.Equals(yIndex.Value);
            }

            public int GetHashCode(Expression obj) => 0;
        }
    }
}