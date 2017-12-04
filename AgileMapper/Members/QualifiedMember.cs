namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using Caching;
    using Extensions;
    using ReadableExpressions.Extensions;

    internal class QualifiedMember : IQualifiedMember
    {
        public static readonly QualifiedMember All = new QualifiedMember(Enumerable<Member>.EmptyArray, null, null);
        public static readonly QualifiedMember None = new QualifiedMember(Enumerable<Member>.EmptyArray, null, null);

        private readonly MapperContext _mapperContext;
        private readonly Func<string> _pathFactory;
        private readonly ICache<Type, QualifiedMember> _runtimeTypedMemberCache;
        private readonly ICache<Member, QualifiedMember> _childMemberCache;

        protected QualifiedMember(Member[] memberChain, QualifiedMember adaptedMember)
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

            MemberChain = memberChain;
            JoinedNames = joinedNames;
            _pathFactory = () => MemberChain.GetFullName();
            IsRecursion = DetermineRecursion();
        }

        private QualifiedMember(Member member, QualifiedMember parent, MapperContext mapperContext)
            : this(member, mapperContext)
        {
            var memberMatchingNames = mapperContext.Naming.GetMatchingNamesFor(member);

            if (parent == null)
            {
                MemberChain = new[] { member };
                JoinedNames = memberMatchingNames;
                _pathFactory = () => MemberChain[0].JoiningName;
                return;
            }

            MemberChain = parent.MemberChain.Append(member);
            JoinedNames = parent.JoinedNames.ExtendWith(memberMatchingNames, mapperContext);

            _pathFactory = () => parent.GetPath() + member.JoiningName;
            IsRecursion = DetermineRecursion();
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
            if (IsSimple || (Type == typeof(object)))
            {
                return false;
            }

            if (MemberChain.Length < 3)
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

            for (var i = MemberChain.Length - 2; i >= 0; --i)
            {
                if (LeafMember.Type == MemberChain[i].Type)
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
                .Select(mapperContext.Naming.GetMatchingNamesFor)
                .ToArray();

            var joinedNames = mapperContext.Naming.GetJoinedNamesFor(matchingNameSets);

            return new QualifiedMember(memberChain, joinedNames, mapperContext);
        }

        #endregion

        public Member[] MemberChain { get; }

        public Member LeafMember { get; }

        public Type Type => LeafMember?.Type;

        public Type ElementType => LeafMember?.ElementType;

        public virtual Type GetElementType(Type sourceElementType) => ElementType;

        public string Name => LeafMember.Name;

        public virtual string RegistrationName { get; }

        public ICollection<string> JoinedNames { get; }

        public string GetPath() => _pathFactory.Invoke();

        public bool IsComplex => LeafMember.IsComplex;

        public bool IsEnumerable => LeafMember.IsEnumerable;

        public bool IsDictionary => LeafMember.IsDictionary;

        public bool IsSimple => LeafMember.IsSimple;

        public bool IsReadable => LeafMember.IsReadable;

        public bool IsReadOnly { get; set; }

        public bool IsRecursion { get; }

        /// <summary>
        /// Determines whether the QualifiedMember represents the first time a Member
        /// recurses within a recursive relationship. For example, the member representing
        /// Foo.SubFoo.SubFoo is a recursion root; Foo.SubFoo is not.
        /// </summary>
        /// <returns>True if the QualifiedMember represents a recursion, otherwise false.</returns>
        public bool IsRecursionRoot()
        {
            if (!IsRecursion || IsSimple)
            {
                return false;
            }

            var recursedMember = default(Member);

            for (var i = MemberChain.Length - 2; i > 0; --i)
            {
                var member = MemberChain[i];

                if (!member.Equals(LeafMember))
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

        public bool IsCustom { get; set; }

        public virtual bool GuardObjectValuePopulations => false;

        IQualifiedMember IQualifiedMember.GetElementMember() => this.GetElementMember();

        IQualifiedMember IQualifiedMember.Append(Member childMember) => Append(childMember);

        [DebuggerStepThrough]
        public QualifiedMember Append(Member childMember)
            => _childMemberCache.GetOrAdd(childMember, CreateChildMember);

        protected virtual QualifiedMember CreateChildMember(Member childMember)
        {
            var qualifiedChildMember = new QualifiedMember(childMember, this, _mapperContext);
            qualifiedChildMember = _mapperContext.QualifiedMemberFactory.GetFinalTargetMember(qualifiedChildMember);

            return qualifiedChildMember;
        }

        public QualifiedMember GetChildMember(string registrationName)
            => _childMemberCache.Values.First(childMember => childMember.RegistrationName == registrationName);

        public IQualifiedMember RelativeTo(IQualifiedMember otherMember)
        {
            var otherQualifiedMember = (QualifiedMember)otherMember;

            if ((otherQualifiedMember.LeafMember == MemberChain[0]) &&
                otherQualifiedMember.MemberChain[0].IsRoot &&
                ((otherQualifiedMember.MemberChain.Length + 1) == MemberChain.Length))
            {
                return this;
            }

            var relativeMemberChain = MemberChain.RelativeTo(otherQualifiedMember.MemberChain);

            return new QualifiedMember(relativeMemberChain, this);
        }

        IQualifiedMember IQualifiedMember.WithType(Type runtimeType) => WithType(runtimeType);

        public QualifiedMember WithType(Type runtimeType)
        {
            if (runtimeType == Type)
            {
                return this;
            }

            var runtimeTypedMember = _runtimeTypedMemberCache
                .GetOrAdd(runtimeType, CreateRuntimeTypedMember);

            return runtimeTypedMember;
        }

        protected virtual QualifiedMember CreateRuntimeTypedMember(Type runtimeType)
        {
            var newMemberChain = new Member[MemberChain.Length];

            for (var i = 0; i < MemberChain.Length - 1; i++)
            {
                newMemberChain[i] = MemberChain[i];
            }

            newMemberChain[MemberChain.Length - 1] = LeafMember.WithType(runtimeType);

            return new QualifiedMember(newMemberChain, this);
        }

        public bool CouldMatch(QualifiedMember otherMember) => JoinedNames.CouldMatch(otherMember.JoinedNames);

        public virtual bool Matches(IQualifiedMember otherMember)
        {
            if (otherMember == this)
            {
                return true;
            }

            if (otherMember is QualifiedMember otherQualifiedMember)
            {
                return JoinedNames.Match(otherQualifiedMember.JoinedNames);
            }

            return otherMember.Matches(this);
        }

        public virtual Expression GetAccess(Expression instance, IMemberMapperData mapperData)
            => LeafMember.GetAccess(instance);

        public Expression GetQualifiedAccess(IMemberMapperData mapperData)
            => MemberChain.GetQualifiedAccess(mapperData);

        public virtual bool CheckExistingElementValue => false;

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public virtual BlockExpression GetAccessChecked(IMemberMapperData mapperData) => null;

        public virtual Expression GetHasDefaultValueCheck(IMemberMapperData mapperData)
            => this.GetAccess(mapperData).GetIsDefaultComparison();

        public virtual Expression GetPopulation(Expression value, IMemberMapperData mapperData)
            => LeafMember.GetPopulation(mapperData.TargetInstance, value);

        public virtual void MapCreating(IQualifiedMember sourceMember)
        {
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString() => GetPath() + ": " + Type.GetFriendlyName();
    }
}