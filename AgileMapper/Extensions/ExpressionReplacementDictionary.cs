namespace AgileObjects.AgileMapper.Extensions
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif

    internal class ExpressionReplacementDictionary : Dictionary<Expression, Expression>
    {
        public ExpressionReplacementDictionary()
            : base(EquivalentMemberAccessComparer.Instance)
        {
        }

        private class EquivalentMemberAccessComparer : IEqualityComparer<Expression>
        {
            public static readonly IEqualityComparer<Expression> Instance = new EquivalentMemberAccessComparer();

            public bool Equals(Expression x, Expression y)
            {
                if (x == y)
                {
                    return true;
                }

                if (x.NodeType != y.NodeType)
                {
                    return false;
                }

                if (x.NodeType != ExpressionType.MemberAccess)
                {
                    return false;
                }

                var memberAccessX = (MemberExpression)x;
                var memberAccessY = (MemberExpression)y;

                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (memberAccessX.Member == memberAccessY.Member)
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

            public int GetHashCode(Expression obj) => 0;
        }
    }
}