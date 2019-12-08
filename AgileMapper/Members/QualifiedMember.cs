namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching;
    using Dictionaries;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    internal class QualifiedMember : IQualifiedMember
    {
        public static readonly QualifiedMember All = new QualifiedMember(default(Member), null);
        public static readonly QualifiedMember None = new QualifiedMember(default(Member), null);

        private readonly MapperContext _mapperContext;
        private readonly QualifiedMember _parent;
        private readonly Func<QualifiedMember, string> _pathFactory;
        private readonly ICache<Member, QualifiedMember> _childMemberCache;
        private ICache<Type, QualifiedMember> _runtimeTypedMemberCache;

        protected QualifiedMember(Member[] memberChain, QualifiedMember adaptedMember)
            : this(memberChain, adaptedMember.JoinedNames, adaptedMember._mapperContext)
        {
            if (IsSimple || adaptedMember.IsSimple)
            {
                return;
            }

            Context = adaptedMember.Context;

            foreach (var childMember in adaptedMember._childMemberCache.Values)
            {
                _childMemberCache.GetOrAdd(childMember.LeafMember, m => childMember);
            }
        }

        private QualifiedMember(Member[] memberChain, IList<string> joinedNames, MapperContext mapperContext)
            : this(memberChain.Last(), mapperContext)
        {
            MemberChain = memberChain;
            JoinedNames = joinedNames;
            _pathFactory = m => m.MemberChain.GetFullName();
            IsRecursion = DetermineRecursion();
        }

        private QualifiedMember(Member member, QualifiedMember parent, MapperContext mapperContext)
            : this(member, mapperContext)
        {
            if (parent == null)
            {
                MemberChain = new[] { member };
                JoinedNames = NamingSettings.RootMatchingNames;
                _pathFactory = m => m.LeafMember.JoiningName;
                return;
            }

            _parent = parent;
            MemberChain = parent.MemberChain.Append(member);
            var memberMatchingNames = mapperContext.Naming.GetMatchingNamesFor(member, parent.Context);
            JoinedNames = parent.JoinedNames.ExtendWith(memberMatchingNames, mapperContext);

            _pathFactory = m => m._parent.GetPath() + m.LeafMember.JoiningName;
            IsRecursion = DetermineRecursion();
        }

        private QualifiedMember(Member leafMember, MapperContext mapperContext)
        {
            if (leafMember == null)
            {
                IsRoot = true;
                return;
            }

            IsRoot = leafMember.IsRoot;
            LeafMember = leafMember;
            _mapperContext = mapperContext;

            if (!IsSimple)
            {
                _childMemberCache = mapperContext.Cache.CreateNew<Member, QualifiedMember>(default(HashCodeComparer<Member>));
            }

            RegistrationName = this.IsConstructorParameter() ? "ctor:" + Name : Name;
            IsReadOnly = IsReadable && !leafMember.IsWriteable;
        }

        #region Setup

        private bool DetermineRecursion()
        {
            if (IsSimple || (Type == typeof(object)))
            {
                return false;
            }

            if (Depth < 3)
            {
                // Need at least 3 members for recursion: 
                // Foo -> Foo.ChildFoo -> Foo.ChildFoo.ChildFoo
                return false;
            }

            for (var i = Depth - 2; i >= 0; --i)
            {
                if ((LeafMember.Type == MemberChain[i].Type) &&
                   ((Depth - i > 2) || LeafMember.Equals(MemberChain[i])))
                {
                    // Recursion if the types match and either:
                    //  1. It's via an intermediate object, e.g. Order.OrderItem.Order, or
                    //  2. It's the same member, e.g. root.Parent.Parent
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Factory Methods

        public static QualifiedMember CreateRoot(Member rootMember, MapperContext mapperContext)
            => new QualifiedMember(rootMember, null, mapperContext);

        public static QualifiedMember Create(Member[] memberChain, MapperContext mapperContext)
        {
            var qualifiedMember = new QualifiedMember(memberChain, Enumerable<string>.EmptyArray, mapperContext);

            QualifiedMemberContext.Set(qualifiedMember, mapperContext);

            return qualifiedMember;
        }

        #endregion

        public IQualifiedMemberContext Context { get; private set; }

        public virtual bool IsRoot { get; }

        public Member[] MemberChain { get; }

        public int Depth => MemberChain.Length;

        public Member LeafMember { get; }

        public Type Type => LeafMember?.Type;

        public Type RootType => MemberChain[0].Type;

        public string GetFriendlyTypeName() => Type.GetFriendlyName();

        public Type ElementType => LeafMember?.ElementType;

        public virtual Type GetElementType(Type sourceElementType) => ElementType;

        public string Name => LeafMember.Name;

        public virtual string RegistrationName { get; }

        public IList<string> JoinedNames { get; private set; }

        public string GetPath() => _pathFactory.Invoke(this);

        public bool IsComplex => LeafMember.IsComplex;

        public bool IsEnumerable => LeafMember.IsEnumerable;

        public bool IsDictionary => LeafMember.IsDictionary;

        public bool IsSimple => LeafMember.IsSimple;

        public bool IsReadable => LeafMember.IsReadable;

        public bool IsReadOnly { get; set; }

        public bool IsRecursion { get; }

        public bool IsCustom { get; set; }

        public virtual bool GuardObjectValuePopulations => false;

        public virtual bool HasCompatibleType(Type type) => Type?.IsAssignableTo(type) == true;

        IQualifiedMember IQualifiedMember.GetElementMember() => this.GetElementMember();

        IQualifiedMember IQualifiedMember.Append(Member childMember) => Append(childMember);

        [DebuggerStepThrough]
        public QualifiedMember Append(Member childMember)
            => _childMemberCache.GetOrAdd(childMember, CreateChildMember);

        protected virtual QualifiedMember CreateChildMember(Member childMember)
            => CreateFinalMember(new QualifiedMember(childMember, this, _mapperContext));

        public QualifiedMember GetChildMember(string registrationName)
            => _childMemberCache.Values.First(childMember => childMember.RegistrationName == registrationName);

        public IQualifiedMember RelativeTo(IQualifiedMember otherMember)
        {
            var otherQualifiedMember = (QualifiedMember)otherMember;

            if ((otherQualifiedMember.LeafMember == MemberChain[0]) &&
                otherQualifiedMember.MemberChain[0].IsRoot &&
                ((otherQualifiedMember.Depth + 1) == Depth))
            {
                return this;
            }

            var relativeMemberChain = MemberChain.RelativeTo(otherQualifiedMember.MemberChain);

            return new QualifiedMember(relativeMemberChain, this);
        }

        IQualifiedMember IQualifiedMember.WithType(Type runtimeType)
        {
            if (runtimeType == Type)
            {
                return this;
            }

            var typedMember = WithType(runtimeType);

            if (runtimeType.IsDictionary())
            {
                return new DictionarySourceMember(typedMember, typedMember);
            }

            return typedMember;
        }

        public QualifiedMember WithType(Type runtimeType)
        {
            if (runtimeType == Type)
            {
                return this;
            }

            var runtimeTypedMember = RuntimeTypedMemberCache
                .GetOrAdd(runtimeType, CreateRuntimeTypedMember);

            return runtimeTypedMember;
        }

        private ICache<Type, QualifiedMember> RuntimeTypedMemberCache
            => _runtimeTypedMemberCache ??
              (_runtimeTypedMemberCache = _mapperContext.Cache.CreateNew<Type, QualifiedMember>(default(HashCodeComparer<Type>)));

        protected virtual QualifiedMember CreateRuntimeTypedMember(Type runtimeType)
        {
            var newMemberChain = new Member[Depth];

            for (var i = 0; i < Depth - 1; i++)
            {
                newMemberChain[i] = MemberChain[i];
            }

            newMemberChain[Depth - 1] = LeafMember.WithType(runtimeType);

            return CreateFinalMember(new QualifiedMember(newMemberChain, this));
        }

        private QualifiedMember CreateFinalMember(QualifiedMember member)
        {
            member.SetContext(Context);

            return member.IsTargetMember
                ? _mapperContext.QualifiedMemberFactory.GetFinalTargetMember(member)
                : member;
        }

        private bool IsTargetMember => MemberChain[0].Name == Member.RootTargetMemberName;

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

        public Expression GetQualifiedAccess(Expression parentInstance)
            => MemberChain.GetQualifiedAccess(parentInstance);

        IQualifiedMember IQualifiedMember.SetContext(IQualifiedMemberContext context)
            => SetContext(context);

        public QualifiedMember SetContext(IQualifiedMemberContext context)
        {
            Context = context;

            if (IsRoot || JoinedNames.Any())
            {
                return this;
            }

            var matchingNameSets = MemberChain.ProjectToArray(
                context,
               (ctx, m) => ctx.MapperContext.Naming.GetMatchingNamesFor(m, ctx));

            JoinedNames = _mapperContext.Naming.GetJoinedNamesFor(matchingNameSets);
            return this;
        }

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

        public virtual void MapCreating(Type sourceType)
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