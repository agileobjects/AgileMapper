namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class EquivalentMemberAccessComparer : IEqualityComparer<Expression>
    {
        public static readonly IEqualityComparer<Expression> Instance = new EquivalentMemberAccessComparer();

        public bool Equals(Expression x, Expression y)
        {
            if (x.NodeType != y.NodeType)
            {
                return false;
            }

            var memberAccessX = (MemberExpression)x;
            var memberAccessY = (MemberExpression)y;

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