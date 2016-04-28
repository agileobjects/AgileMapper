namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class QualifiedMember
    {
        private readonly Member[] _memberChain;

        private QualifiedMember(Member member, QualifiedMember parent)
            : this(parent?._memberChain.Concat(member).ToArray() ?? new[] { member })
        {
        }

        private QualifiedMember(Member[] memberChain)
        {
            _memberChain = memberChain;
            LeafMember = memberChain.Last();
        }

        #region Factory Method

        public static QualifiedMember From(Member member)
        {
            return new QualifiedMember(member, null);
        }

        public static QualifiedMember From(Member[] memberChain)
        {
            return new QualifiedMember(memberChain);
        }

        #endregion

        public Member LeafMember { get; }

        public Type Type => LeafMember.Type;

        public bool IsEnumerable => LeafMember.IsEnumerable;

        public Type ElementType => LeafMember.ElementType;

        public QualifiedMember Append(Member childMember)
        {
            return new QualifiedMember(childMember, this);
        }

        public QualifiedMember RelativeTo(int depth)
        {
            if (depth == 0)
            {
                return this;
            }

            var relativeMemberChain = new Member[_memberChain.Length - depth];

            Array.Copy(
                _memberChain,
                _memberChain.Length - relativeMemberChain.Length,
                relativeMemberChain,
                0,
                relativeMemberChain.Length);

            return new QualifiedMember(relativeMemberChain);
        }

        public bool Matches(QualifiedMember otherMember)
        {
            if (_memberChain.Length != otherMember._memberChain.Length)
            {
                return false;
            }

            for (var i = 1; i < _memberChain.Length; i++)
            {
                if (_memberChain[i].Name != otherMember._memberChain[i].Name)
                {
                    return false;
                }
            }

            return true;
        }

        public Expression GetAccess(Expression instance)
        {
            return _memberChain
                .Skip(1)
                .Aggregate(
                    instance,
                    (accessSoFar, member) => member.GetAccess(accessSoFar));
        }
    }
}