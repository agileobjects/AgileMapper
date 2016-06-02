namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class QualifiedMember : IQualifiedMember
    {
        public static readonly IQualifiedMember All = new QualifiedMember(new Member[0], null);

        private readonly Member[] _memberChain;
        private readonly IQualifiedMemberName _qualifiedName;

        private QualifiedMember(Member member, QualifiedMember parent)
            : this(parent?._memberChain.Concat(member).ToArray() ?? new[] { member })
        {
        }

        private QualifiedMember(Member[] memberChain)
            : this(memberChain, new QualifiedMemberName(memberChain.Select(m => m.MemberName).ToArray()))
        {
        }

        private QualifiedMember(Member[] memberChain, IQualifiedMemberName qualifiedName)
        {
            _memberChain = memberChain;
            LeafMember = memberChain.LastOrDefault();
            _qualifiedName = qualifiedName;
            Signature = string.Join(">", memberChain.Select(m => m.Signature));
        }

        #region Factory Method

        public static QualifiedMember From(Member member) => new QualifiedMember(member, null);

        public static QualifiedMember From(Member[] memberChain) => new QualifiedMember(memberChain);

        #endregion

        public IEnumerable<Member> Members => _memberChain;

        public Member LeafMember { get; }

        public Type DeclaringType => LeafMember.DeclaringType;

        public Type Type => LeafMember.Type;

        public string Name => LeafMember.Name;

        public bool IsComplex => LeafMember.IsComplex;

        public bool IsEnumerable => LeafMember.IsEnumerable;

        public bool IsSimple => LeafMember.IsSimple;

        public bool IsReadable => LeafMember.IsReadable;

        public string Signature { get; }

        public IQualifiedMember Append(Member childMember)
        {
            return new QualifiedMember(childMember, this);
        }

        public IQualifiedMember RelativeTo(int depth)
        {
            if (depth == 0)
            {
                return this;
            }

            var relativeMemberChain = new Member[_memberChain.Length - depth];

            Array.Copy(
                _memberChain,
                depth,
                relativeMemberChain,
                0,
                relativeMemberChain.Length);

            return new QualifiedMember(relativeMemberChain);
        }

        public IQualifiedMember WithType(Type runtimeType)
        {
            if (runtimeType == Type)
            {
                return this;
            }

            var newMemberChain = new Member[_memberChain.Length];
            Array.Copy(_memberChain, 0, newMemberChain, 0, newMemberChain.Length - 1);

            newMemberChain[newMemberChain.Length - 1] = LeafMember.WithType(runtimeType);

            return From(newMemberChain);
        }

        public bool IsSameAs(IQualifiedMember otherMember)
        {
            if ((this == All) || (otherMember == All))
            {
                return true;
            }

            return (otherMember.Type == Type) &&
                   (otherMember.Name == Name) &&
                   otherMember.DeclaringType.IsAssignableFrom(DeclaringType);
        }

        public bool Matches(IQualifiedMember otherMember)
        {
            var otherQualifiedMember = otherMember as QualifiedMember;

            return (otherQualifiedMember != null)
                ? _qualifiedName.Matches(otherQualifiedMember._qualifiedName)
                : otherMember.Matches(this);
        }

        public Expression GetAccess(Expression instance) => LeafMember.GetAccess(instance);

        public Expression GetQualifiedAccess(Expression instance)
            => _memberChain.Skip(1).GetQualifiedAccess(instance);

        public Expression GetPopulation(Expression instance, Expression value)
            => LeafMember.GetPopulation(instance, value);
    }
}