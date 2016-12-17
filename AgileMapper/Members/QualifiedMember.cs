namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
#if !NET_STANDARD
    using System.Diagnostics.CodeAnalysis;
#endif
    using System.Linq;
    using System.Linq.Expressions;
    using Caching;
    using Extensions;

    internal class QualifiedMember : IQualifiedMember
    {
        public static readonly QualifiedMember All = new QualifiedMember(Enumerable<Member>.EmptyArray, null, null);
        public static readonly QualifiedMember None = new QualifiedMember(Enumerable<Member>.EmptyArray, null, null);

        private readonly Member[] _memberChain;
        private readonly MapperContext _mapperContext;
        private readonly Func<string> _pathFactory;
        private readonly ICache<Type, QualifiedMember> _runtimeTypedMemberCache;
        private readonly ICache<Member, QualifiedMember> _childMemberCache;

        private QualifiedMember(Member[] memberChain, QualifiedMember adaptedMember)
            : this(memberChain, adaptedMember.JoinedNames, adaptedMember._mapperContext)
        {
            foreach (var childMember in adaptedMember._childMemberCache.Values)
            {
                _childMemberCache.GetOrAdd(childMember.LeafMember, m => childMember);
            }
        }

        private QualifiedMember(Member[] memberChain, ICollection<string> joinedNames, MapperContext mapperContext)
            : this(memberChain.LastOrDefault(), mapperContext)
        {
            if (LeafMember == null)
            {
                return;
            }

            _memberChain = memberChain;
            JoinedNames = joinedNames;
            _pathFactory = () => _memberChain.GetFullName();
            IsRecursive = DetermineRecursion();
        }

        private QualifiedMember(Member member, QualifiedMember parent, MapperContext mapperContext)
            : this(member, mapperContext)
        {
            var memberMatchingNames = mapperContext.NamingSettings.GetMatchingNamesFor(member);

            if (parent == null)
            {
                _memberChain = new[] { member };
                JoinedNames = memberMatchingNames;
                _pathFactory = () => _memberChain[0].JoiningName;
                return;
            }

            _memberChain = parent._memberChain.Append(member);
            JoinedNames = parent.JoinedNames.ExtendWith(memberMatchingNames, mapperContext);

            _pathFactory = () => parent.GetPath() + member.JoiningName;
            IsRecursive = DetermineRecursion();
        }

        private QualifiedMember(Member leafMember, MapperContext mapperContext)
        {
            if (leafMember == null)
            {
                return;
            }

            LeafMember = leafMember;
            _mapperContext = mapperContext;
            _runtimeTypedMemberCache = mapperContext.Cache.CreateNew<Type, QualifiedMember>();
            _childMemberCache = mapperContext.Cache.CreateNew<Member, QualifiedMember>();

            RegistrationName = (LeafMember.MemberType != MemberType.ConstructorParameter)
                ? Name
                : "ctor:" + Name;

            IsReadOnly = IsReadable && !leafMember.IsWriteable;
        }

        #region Setup

        private bool DetermineRecursion()
        {
            if (IsSimple)
            {
                return false;
            }

            if (_memberChain.Length < 3)
            {
                // Need at least 3 members for recursion: 
                // Foo -> Foo.ChildFoo -> Foo.ChildFoo.ChildFoo
                return false;
            }

            if (LeafMember.IsEnumerableElement())
            {
                // Recurse on enumerable and complex type members, 
                // not enumerable elements:
                return false;
            }

            for (var i = _memberChain.Length - 2; i > 0; --i)
            {
                if (LeafMember == _memberChain[i])
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Factory Method

        public static QualifiedMember From(Member member, MapperContext mapperContext)
            => new QualifiedMember(member, null, mapperContext);

        public static QualifiedMember From(Member[] memberChain, MapperContext mapperContext)
        {
            var matchingNameSets = memberChain
                .Select(mapperContext.NamingSettings.GetMatchingNamesFor)
                .ToArray();

            var joinedNames = mapperContext.NamingSettings.GetJoinedNamesFor(matchingNameSets);

            return new QualifiedMember(memberChain, joinedNames, mapperContext);
        }

        #endregion

        public IEnumerable<Member> MemberChain => _memberChain;

        public Member LeafMember { get; }

        public Type Type => LeafMember?.Type;

        public Type ElementType => LeafMember?.ElementType;

        public string Name => LeafMember.Name;

        public string RegistrationName { get; }

        public ICollection<string> JoinedNames { get; }

        public string GetPath() => _pathFactory.Invoke();

        public bool IsComplex => LeafMember.IsComplex;

        public bool IsEnumerable => LeafMember.IsEnumerable;

        public bool IsSimple => LeafMember.IsSimple;

        public bool IsReadable => LeafMember.IsReadable;

        public bool IsReadOnly { get; set; }

        public bool IsRecursive { get; }

        public bool IsRecursionRoot()
        {
            if (!IsRecursive)
            {
                return false;
            }

            var recursedMember = default(Member);

            for (var i = _memberChain.Length - 2; i > 0; --i)
            {
                var member = _memberChain[i];

                if (member != LeafMember)
                {
                    continue;
                }

                if (recursedMember != null)
                {
                    return false;
                }

                recursedMember = member;
            }

            return true;
        }

        IQualifiedMember IQualifiedMember.GetElementMember() => GetElementMember();

        public QualifiedMember GetElementMember() => Append(Type.GetElementMember());

        IQualifiedMember IQualifiedMember.Append(Member childMember) => Append(childMember);

        public QualifiedMember Append(Member childMember)
            => _childMemberCache.GetOrAdd(childMember, cm => new QualifiedMember(cm, this, _mapperContext));

        public QualifiedMember GetChildMember(string registrationName)
            => _childMemberCache.Values.First(childMember => childMember.RegistrationName == registrationName);

        public IQualifiedMember RelativeTo(IQualifiedMember otherMember)
        {
            var otherQualifiedMember = (QualifiedMember)otherMember;

            if ((otherQualifiedMember.LeafMember == _memberChain[0]) &&
                otherQualifiedMember._memberChain[0].IsRoot &&
                ((otherQualifiedMember._memberChain.Length + 1) == _memberChain.Length))
            {
                return this;
            }

            var relativeMemberChain = _memberChain.RelativeTo(otherQualifiedMember._memberChain);

            return new QualifiedMember(relativeMemberChain, this);
        }

        IQualifiedMember IQualifiedMember.WithType(Type runtimeType) => WithType(runtimeType);

        public QualifiedMember WithType(Type runtimeType)
        {
            if (runtimeType == Type)
            {
                return this;
            }

            var runtimeTypedMember = _runtimeTypedMemberCache.GetOrAdd(runtimeType, rt =>
            {
                var newMemberChain = new Member[_memberChain.Length];

                for (var i = 0; i < _memberChain.Length - 1; i++)
                {
                    newMemberChain[i] = _memberChain[i];
                }

                newMemberChain[_memberChain.Length - 1] = LeafMember.WithType(rt);

                return new QualifiedMember(newMemberChain, this);
            });

            return runtimeTypedMember;
        }

        public bool CouldMatch(QualifiedMember otherMember) => JoinedNames.CouldMatch(otherMember.JoinedNames);

        public bool Matches(IQualifiedMember otherMember)
        {
            if (otherMember == this)
            {
                return true;
            }

            var otherQualifiedMember = otherMember as QualifiedMember;
            if (otherQualifiedMember != null)
            {
                return JoinedNames.Match(otherQualifiedMember.JoinedNames);
            }

            return otherMember.Matches(this);
        }

        public Expression GetAccess(Expression instance) => LeafMember.GetAccess(instance);

        public Expression GetQualifiedAccess(Expression instance)
            => _memberChain.GetQualifiedAccess(instance);

        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString() => GetPath();
    }
}