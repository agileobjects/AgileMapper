namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Caching;
    using Extensions;
    using ReadableExpressions;

    internal class ConfiguredSourceMember : IQualifiedMember
    {
        private readonly string[] _matchedTargetMemberNames;
        private readonly IEnumerable<string> _matchedTargetMemberJoinedNames;
        private readonly MapperContext _mapperContext;
        private readonly Member[] _childMembers;
        private readonly ICache<Member, ConfiguredSourceMember> _childMemberCache;

        public ConfiguredSourceMember(Expression value, IMemberMapperData mapperData)
            : this(
                  value.Type,
                  value.Type.IsEnumerable(),
                  value.ToReadableString(),
                  mapperData.TargetMember.MemberChain.Select(mapperData.MapperContext.NamingSettings.GetMatchingNameFor).ToArray(),
                  mapperData.MapperContext)
        {
        }

        private ConfiguredSourceMember(ConfiguredSourceMember parent, Member childMember)
            : this(
                  childMember.Type,
                  childMember.IsEnumerable,
                  parent.Name + childMember.JoiningName,
                  parent._matchedTargetMemberNames.Append(
                      parent._mapperContext.NamingSettings.GetMatchingNameFor(childMember)),
                  parent._mapperContext,
                  parent._childMembers.Append(childMember))
        {
        }

        private ConfiguredSourceMember(
            Type type,
            bool isEnumerable,
            string name,
            string[] matchedTargetMemberNames,
            MapperContext mapperContext,
            Member[] childMembers = null)
            : this(
                  type,
                  isEnumerable,
                  name,
                  matchedTargetMemberNames,
                  mapperContext.NamingSettings.GetJoinedNamesFor(matchedTargetMemberNames),
                  mapperContext,
                  childMembers)
        {
        }

        private ConfiguredSourceMember(
            Type type,
            bool isEnumerable,
            string name,
            string[] matchedTargetMemberNames,
            IEnumerable<string> matchedTargetMemberJoinedNames,
            MapperContext mapperContext,
            Member[] childMembers = null)
        {
            Type = type;
            IsEnumerable = isEnumerable;
            Name = name;
            _matchedTargetMemberNames = matchedTargetMemberNames;
            _matchedTargetMemberJoinedNames = matchedTargetMemberJoinedNames;
            _mapperContext = mapperContext;
            _childMembers = childMembers ?? new[] { Member.RootSource(name, type) };
            _childMemberCache = mapperContext.Cache.CreateNew<Member, ConfiguredSourceMember>();
        }

        public Type Type { get; }

        public bool IsEnumerable { get; }

        public string Name { get; }

        public string GetPath() => _childMembers.GetFullName();

        public IQualifiedMember Append(Member childMember)
            => _childMemberCache.GetOrAdd(childMember, cm => new ConfiguredSourceMember(this, cm));

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
                _mapperContext,
                relativeMemberChain);
        }

        public bool CouldMatch(QualifiedMember otherMember)
            => _matchedTargetMemberJoinedNames.CouldMatch(otherMember.JoinedNames);

        public bool Matches(IQualifiedMember otherMember)
        {
            if (otherMember == this)
            {
                return true;
            }

            var otherQualifiedMember = otherMember as QualifiedMember;
            if (otherQualifiedMember != null)
            {
                return _matchedTargetMemberJoinedNames.Match(otherQualifiedMember.JoinedNames);
            }

            var otherConfiguredMember = otherMember as ConfiguredSourceMember;
            if (otherConfiguredMember != null)
            {
                return _matchedTargetMemberJoinedNames.Match(otherConfiguredMember._matchedTargetMemberJoinedNames);
            }

            return false;
        }

        public Expression GetQualifiedAccess(Expression instance) => _childMembers.GetQualifiedAccess(instance);

        public IQualifiedMember WithType(Type runtimeType)
        {
            if (runtimeType == Type)
            {
                return this;
            }

            return new ConfiguredSourceMember(
                runtimeType,
                IsEnumerable || runtimeType.IsEnumerable(),
                Name,
                _matchedTargetMemberNames,
                _matchedTargetMemberJoinedNames,
                _mapperContext,
                _childMembers);
        }
    }
}