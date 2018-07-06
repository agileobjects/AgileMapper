namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Caching;
    using Extensions.Internal;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ConfiguredSourceMember : IQualifiedMember
    {
        private readonly Expression _rootValue;
        private readonly IList<string> _matchedTargetMemberJoinedNames;
        private readonly MapperContext _mapperContext;
        private readonly Member[] _childMembers;
        private readonly ICache<Member, ConfiguredSourceMember> _childMemberCache;
        private readonly bool _isMatchedToRootTarget;

        public ConfiguredSourceMember(Expression value, IMemberMapperData mapperData)
            : this(
                  value,
                  value.Type,
                  value.Type.IsEnumerable(),
                  value.Type.IsSimple(),
                  value.ToReadableString(),
                  mapperData.TargetMember.JoinedNames,
                  mapperData.MapperContext)
        {
            _isMatchedToRootTarget = mapperData.TargetMember.IsRoot;
        }

        private ConfiguredSourceMember(ConfiguredSourceMember parent, Member childMember)
            : this(
                  parent._rootValue,
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
            Expression rootValue,
            Type type,
            bool isEnumerable,
            bool isSimple,
            string name,
            IList<string> matchedTargetMemberJoinedNames,
            MapperContext mapperContext,
            Member[] childMembers = null)
        {
            _rootValue = rootValue;
            Type = type;
            IsEnumerable = isEnumerable;
            IsSimple = isSimple;
            Name = name;
            _matchedTargetMemberJoinedNames = matchedTargetMemberJoinedNames;
            _mapperContext = mapperContext;
            _childMembers = childMembers ?? new[] { Member.RootSource(name, type) };

            if (isSimple)
            {
                return;
            }

            if (isEnumerable)
            {
                ElementType = (childMembers != null)
                    ? childMembers.Last().ElementType
                    : type.GetEnumerableElementType();
            }

            _childMemberCache = mapperContext.Cache.CreateNew<Member, ConfiguredSourceMember>();
        }

        public bool IsRoot => false;

        public Type Type { get; }

        public Type ElementType { get; }

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
            if (!(otherMember is ConfiguredSourceMember otherConfiguredMember))
            {
                return this;
            }

            var relativeMemberChain = _childMembers.RelativeTo(otherConfiguredMember._childMembers);

            if ((relativeMemberChain == _childMembers) ||
                 relativeMemberChain.SequenceEqual(_childMembers))
            {
                return this;
            }

            return new ConfiguredSourceMember(
                _rootValue,
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

        public Expression GetQualifiedAccess(Expression parentInstance)
        {
            if (_isMatchedToRootTarget && _childMembers.HasOne())
            {
                return _rootValue;
            }

            return _childMembers.GetQualifiedAccess(parentInstance);
        }

        public IQualifiedMember WithType(Type runtimeType)
        {
            if (runtimeType == Type)
            {
                return this;
            }

            var isEnumerable = IsEnumerable || runtimeType.IsEnumerable();
            var isSimple = !isEnumerable && (IsSimple || runtimeType.IsSimple());

            return new ConfiguredSourceMember(
                _rootValue,
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