namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using ReadableExpressions;

    internal class ConfiguredSourceMember : IQualifiedMember
    {
        private readonly string[] _matchedTargetMemberNames;
        private readonly IEnumerable<string> _matchedTargetMemberJoinedNames;
        private readonly NamingSettings _namingSettings;
        private readonly Member[] _childMembers;

        public ConfiguredSourceMember(Expression value, MemberMapperData data)
            : this(
                  value.Type,
                  value.Type.IsEnumerable(),
                  value.ToReadableString(),
                  data.TargetMember.MemberChain.Select(data.MapperContext.NamingSettings.GetMatchingNameFor).ToArray(),
                  data.MapperContext.NamingSettings)
        {
        }

        private ConfiguredSourceMember(ConfiguredSourceMember parent, Member childMember, bool isEnumerable)
            : this(
                  childMember.Type,
                  isEnumerable,
                  parent.Name + childMember.JoiningName,
                  parent._matchedTargetMemberNames.Append(parent._namingSettings.GetMatchingNameFor(childMember)),
                  parent._namingSettings,
                  parent._childMembers.Append(childMember))
        {
        }

        private ConfiguredSourceMember(
            Type type,
            bool isEnumerable,
            string name,
            string[] matchedTargetMemberNames,
            NamingSettings namingSettings,
            Member[] childMembers = null)
            : this(
                  type, 
                  isEnumerable, 
                  name, 
                  matchedTargetMemberNames, 
                  namingSettings.GetJoinedNamesFor(matchedTargetMemberNames), 
                  namingSettings, 
                  childMembers)
        {
        }

        private ConfiguredSourceMember(
            Type type,
            bool isEnumerable,
            string name,
            string[] matchedTargetMemberNames,
            IEnumerable<string> matchedTargetMemberJoinedNames,
            NamingSettings namingSettings,
            Member[] childMembers = null)
        {
            Type = type;
            IsEnumerable = isEnumerable;
            Name = name;
            _matchedTargetMemberNames = matchedTargetMemberNames;
            _matchedTargetMemberJoinedNames = matchedTargetMemberJoinedNames;
            _namingSettings = namingSettings;
            _childMembers = childMembers ?? new[] { Member.RootSource(name, type) };
            Signature = _childMembers.GetSignature();
        }

        public Type Type { get; }

        public bool IsEnumerable { get; }

        public string Name { get; }

        public string Signature { get; }

        public string GetPath() => _childMembers.GetFullName();

        public IQualifiedMember Append(Member childMember) => new ConfiguredSourceMember(this, childMember, IsEnumerable);

        public IQualifiedMember RelativeTo(IQualifiedMember otherMember)
        {
            var otherConfiguredMember = (ConfiguredSourceMember)otherMember;
            var relativeMemberChain = _childMembers.RelativeTo(otherConfiguredMember._childMembers);

            return new ConfiguredSourceMember(
                Type,
                IsEnumerable,
                Name,
                _matchedTargetMemberNames,
                _matchedTargetMemberJoinedNames,
                _namingSettings,
                relativeMemberChain);
        }

        public bool CouldMatch(QualifiedMember otherMember)
            => _matchedTargetMemberJoinedNames.CouldMatch(otherMember.JoinedNames);

        public bool Matches(QualifiedMember otherMember)
            => _matchedTargetMemberJoinedNames.Match(otherMember.JoinedNames);

        public Expression GetQualifiedAccess(Expression instance) => _childMembers.GetQualifiedAccess(instance);

        public IQualifiedMember WithType(Type runtimeType) => this;
    }
}