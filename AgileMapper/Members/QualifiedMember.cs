namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    [DebuggerDisplay("{Signature}")]
    internal class QualifiedMember : IQualifiedMember
    {
        public static readonly QualifiedMember All = new QualifiedMember(new Member[0], new string[0], NamingSettings.Default);
        public static readonly QualifiedMember None = new QualifiedMember(new Member[0], new string[0], NamingSettings.Default);

        private readonly Member[] _memberChain;
        private readonly string[] _memberMatchingNames;
        private readonly IEnumerable<string> _joinedNames;
        private readonly NamingSettings _namingSettings;
        private readonly Func<string> _pathFactory;

        private QualifiedMember(Member[] memberChain, string[] memberMatchingNames, NamingSettings namingSettings)
            : this(memberChain.LastOrDefault(), namingSettings)
        {
            _memberChain = memberChain;
            _memberMatchingNames = memberMatchingNames;
            _joinedNames = namingSettings.GetJoinedNamesFor(memberMatchingNames);
            Signature = memberChain.GetSignature();
            _pathFactory = () => _memberChain.GetFullName();
        }

        private QualifiedMember(Member member, QualifiedMember parent, NamingSettings namingSettings)
            : this(member, namingSettings)
        {
            var matchingName = namingSettings.GetMatchingNameFor(member);

            if (parent == null)
            {
                _memberChain = new[] { member };
                _memberMatchingNames = new[] { matchingName };
                _joinedNames = _memberMatchingNames;
                Signature = member.Signature;
                _pathFactory = () => _memberChain[0].JoiningName;
                return;
            }

            _memberChain = parent._memberChain.Append(member);
            _memberMatchingNames = parent._memberMatchingNames.Append(matchingName);
            _joinedNames = namingSettings.GetJoinedNamesFor(_memberMatchingNames);

            Signature = parent.Signature + "." + member.Signature;
            _pathFactory = () => parent.GetPath() + member.JoiningName;
        }

        private QualifiedMember(Member leafMember, NamingSettings namingSettings)
        {
            LeafMember = leafMember;
            _namingSettings = namingSettings;
        }

        #region Factory Method

        public static QualifiedMember From(Member member, NamingSettings namingSettings)
            => new QualifiedMember(member, null, namingSettings);

        public static QualifiedMember From(IEnumerable<Member> memberChain, NamingSettings namingSettings)
        {
            var memberChainArray = memberChain.ToArray();
            var matchingNames = memberChainArray.Select(namingSettings.GetMatchingNameFor).ToArray();

            return new QualifiedMember(memberChainArray, matchingNames, namingSettings);
        }

        #endregion

        public IEnumerable<Member> MemberChain => _memberChain;

        public Member LeafMember { get; }

        public Type Type => LeafMember?.Type;

        public string Name => LeafMember.Name;

        public string GetPath() => _pathFactory.Invoke();

        public bool IsComplex => LeafMember.IsComplex;

        public bool IsEnumerable => LeafMember.IsEnumerable;

        public bool IsSimple => LeafMember.IsSimple;

        public bool IsReadable => LeafMember.IsReadable;

        public string Signature { get; }

        IQualifiedMember IQualifiedMember.Append(Member childMember) => Append(childMember);

        public QualifiedMember Append(Member childMember) => new QualifiedMember(childMember, this, _namingSettings);

        public IQualifiedMember RelativeTo(IQualifiedMember otherMember)
        {
            var otherQualifiedMember = (QualifiedMember)otherMember;

            if (otherQualifiedMember.LeafMember == _memberChain[0])
            {
                return this;
            }

            var relativeMemberChain = _memberChain.RelativeTo(otherQualifiedMember._memberChain);

            return new QualifiedMember(relativeMemberChain, _memberMatchingNames, _namingSettings);
        }

        IQualifiedMember IQualifiedMember.WithType(Type runtimeType) => WithType(runtimeType);

        public QualifiedMember WithType(Type runtimeType)
        {
            if (runtimeType == Type)
            {
                return this;
            }

            _memberChain[_memberChain.Length - 1] = LeafMember.WithType(runtimeType);

            return new QualifiedMember(_memberChain, _memberMatchingNames, _namingSettings);
        }

        public bool IsSameAs(QualifiedMember otherMember)
        {
            if (this == otherMember)
            {
                return true;
            }

            if ((this == All) || (otherMember == All))
            {
                return true;
            }

            if ((this == None) || (otherMember == None))
            {
                return false;
            }

            return (otherMember.Type == Type) &&
                   (otherMember.Name == Name) &&
                   otherMember.LeafMember.DeclaringType.IsAssignableFrom(LeafMember.DeclaringType);
        }

        public bool CouldMatch(QualifiedMember otherMember)
        {
            return otherMember._joinedNames
                .Any(otherJoinedName => _joinedNames
                    .Any(joinedName => otherJoinedName.StartsWith(joinedName, StringComparison.OrdinalIgnoreCase)));
        }

        public bool Matches(IQualifiedMember otherMember)
        {
            var otherQualifiedMember = otherMember as QualifiedMember;

            if (otherQualifiedMember == null)
            {
                return otherMember.Matches(this);
            }

            return _joinedNames
                .Intersect(otherQualifiedMember._joinedNames, CaseInsensitiveStringComparer.Instance)
                .Any();
        }

        public Expression GetAccess(Expression instance) => LeafMember.GetAccess(instance);

        public Expression GetQualifiedAccess(Expression instance)
            => _memberChain.GetQualifiedAccess(instance);
    }
}