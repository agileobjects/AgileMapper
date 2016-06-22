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

        private readonly Member[] _memberChainChain;
        private readonly string[] _memberMatchingNames;
        private readonly IEnumerable<string> _joinedNames;
        private readonly NamingSettings _namingSettings;

        private QualifiedMember(Member[] memberChainChain, string[] memberMatchingNames, NamingSettings namingSettings)
            : this(memberChainChain.LastOrDefault(), namingSettings)
        {
            _memberChainChain = memberChainChain;
            _memberMatchingNames = memberMatchingNames;
            _joinedNames = namingSettings.GetJoinedNamesFor(memberMatchingNames);
            Signature = string.Join(">", memberChainChain.Select(m => m.Signature));
            Path = GetFullName(memberChainChain);
        }

        private QualifiedMember(Member member, QualifiedMember parent, NamingSettings namingSettings)
            : this(member, namingSettings)
        {
            var matchingName = namingSettings.GetMatchingNameFor(member);

            if (parent == null)
            {
                _memberChainChain = new[] { member };
                _memberMatchingNames = new[] { matchingName };
                _joinedNames = _memberMatchingNames;
                Signature = member.Signature;
                Path = member.JoiningName;
                return;
            }

            _memberChainChain = parent._memberChainChain.Append(member);
            _memberMatchingNames = parent._memberMatchingNames.Append(matchingName);
            _joinedNames = namingSettings.GetJoinedNamesFor(_memberMatchingNames);

            Signature = parent.Signature + ">" + member.Signature;
            Path = parent.Path + member.JoiningName;
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

        public static string GetFullName(IEnumerable<Member> members)
            => string.Join(string.Empty, members.Select(m => m.JoiningName));

        public IEnumerable<Member> MemberChain => _memberChainChain;

        public Member LeafMember { get; }

        public Type DeclaringType => LeafMember.DeclaringType;

        public Type Type => LeafMember?.Type;

        public string Name => LeafMember.Name;

        public string Path { get; }

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

            if (otherQualifiedMember.LeafMember == _memberChainChain[0])
            {
                return this;
            }

            var relativeMemberChain = _memberChainChain.RelativeTo(otherQualifiedMember._memberChainChain);

            return new QualifiedMember(relativeMemberChain, _memberMatchingNames, _namingSettings);
        }

        IQualifiedMember IQualifiedMember.WithType(Type runtimeType) => WithType(runtimeType);

        public QualifiedMember WithType(Type runtimeType)
        {
            if (runtimeType == Type)
            {
                return this;
            }

            _memberChainChain[_memberChainChain.Length - 1] = LeafMember.WithType(runtimeType);

            return new QualifiedMember(_memberChainChain, _memberMatchingNames, _namingSettings);
        }

        public bool IsSameAs(IQualifiedMember otherMember)
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
                   otherMember.DeclaringType.IsAssignableFrom(DeclaringType);
        }

        public bool CouldMatch(IQualifiedMember otherMember)
        {
            if (_memberChainChain.Length == 0)
            {
                return true;
            }

            var otherQualifiedMember = otherMember as QualifiedMember;

            if (otherQualifiedMember == null)
            {
                return otherMember.CouldMatch(this);
            }

            return otherQualifiedMember._joinedNames
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
            => _memberChainChain.GetQualifiedAccess(instance);

        public Expression GetPopulation(Expression instance, Expression value)
            => LeafMember.GetPopulation(instance, value);
    }
}