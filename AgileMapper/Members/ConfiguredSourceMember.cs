namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Caching;
    using Extensions.Internal;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

    internal class ConfiguredSourceMember : IQualifiedMember
    {
        private readonly ICollection<string> _matchedTargetMemberJoinedNames;
        private readonly MapperContext _mapperContext;
        private readonly Member[] _childMembers;
        private readonly ICache<Member, ConfiguredSourceMember> _childMemberCache;

        public ConfiguredSourceMember(Expression value, IMemberMapperData mapperData)
            : this(
                  value.Type,
                  value.Type.IsEnumerable(),
                  value.Type.IsSimple(),
                  value.ToReadableString(),
                  mapperData.TargetMember.JoinedNames,
                  mapperData.MapperContext)
        {
        }

        private ConfiguredSourceMember(ConfiguredSourceMember parent, Member childMember)
            : this(
                  childMember.Type,
                  childMember.IsEnumerable,
                  childMember.IsSimple,
                  parent.Name + childMember.JoiningName,
                  parent._matchedTargetMemberJoinedNames.ExtendWith(
                      parent._mapperContext.Naming.GetMatchingNamesFor(childMember),
                      parent._mapperContext),
                  parent._mapperContext,
                  parent._childMembers.Append(childMember))
        {
        }

        private ConfiguredSourceMember(
            Type type,
            bool isEnumerable,
            bool isSimple,
            string name,
            ICollection<string> matchedTargetMemberJoinedNames,
            MapperContext mapperContext,
            Member[] childMembers = null)
        {
            Type = type;
            IsEnumerable = isEnumerable;
            IsSimple = isSimple;
            Name = name;
            _matchedTargetMemberJoinedNames = matchedTargetMemberJoinedNames;
            _mapperContext = mapperContext;
            _childMembers = childMembers ?? new[] { Member.RootSource(name, type) };

            if (!isSimple)
            {
                _childMemberCache = mapperContext.Cache.CreateNew<Member, ConfiguredSourceMember>();
            }
        }

        public Type Type { get; }

        public string GetFriendlyTypeName() => Type.GetFriendlyName();

        public bool IsEnumerable { get; }

        public bool IsSimple { get; }

        public string Name { get; }

        public string GetPath() => _childMembers.GetFullName();

        IQualifiedMember IQualifiedMember.GetElementMember() => this.GetElementMember();

        public IQualifiedMember Append(Member childMember)
            => _childMemberCache.GetOrAdd(childMember, cm => new ConfiguredSourceMember(this, cm));

        public IQualifiedMember RelativeTo(IQualifiedMember otherMember)
        {
            var otherConfiguredMember = (ConfiguredSourceMember)otherMember;
            var relativeMemberChain = _childMembers.RelativeTo(otherConfiguredMember._childMembers);

            return new ConfiguredSourceMember(
                Type,
                IsEnumerable,
                IsSimple,
                Name,
                _matchedTargetMemberJoinedNames,
                _mapperContext,
                relativeMemberChain);
        }

        public bool HasCompatibleType(Type type) => false;

        public bool CouldMatch(QualifiedMember otherMember)
            => _matchedTargetMemberJoinedNames.CouldMatch(otherMember.JoinedNames);

        public bool Matches(IQualifiedMember otherMember)
        {
            if (otherMember is QualifiedMember otherQualifiedMember)
            {
                return _matchedTargetMemberJoinedNames.Match(otherQualifiedMember.JoinedNames);
            }

            if (otherMember is ConfiguredSourceMember otherConfiguredMember)
            {
                return _matchedTargetMemberJoinedNames.Match(otherConfiguredMember._matchedTargetMemberJoinedNames);
            }

            return false;
        }

        public Expression GetQualifiedAccess(IMemberMapperData mapperData) => _childMembers.GetQualifiedAccess(mapperData);

        public IQualifiedMember WithType(Type runtimeType)
        {
            if (runtimeType == Type)
            {
                return this;
            }

            var isEnumerable = IsEnumerable || runtimeType.IsEnumerable();
            var isSimple = !isEnumerable && (IsSimple || runtimeType.IsSimple());

            return new ConfiguredSourceMember(
                runtimeType,
                isEnumerable,
                isSimple,
                Name,
                _matchedTargetMemberJoinedNames,
                _mapperContext,
                _childMembers);
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString() => GetPath() + ": " + Type.GetFriendlyName();
    }
}