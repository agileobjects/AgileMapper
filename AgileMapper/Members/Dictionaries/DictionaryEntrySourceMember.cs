namespace AgileObjects.AgileMapper.Members.Dictionaries
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using ReadableExpressions.Extensions;

    internal class DictionaryEntrySourceMember : IQualifiedMember
    {
        private readonly QualifiedMember _matchedTargetMember;
        private readonly Func<string> _pathFactory;
        private readonly Member[] _childMembers;

        public DictionaryEntrySourceMember(
            Type entryType,
            QualifiedMember matchedTargetMember,
            DictionarySourceMember parent)
            : this(
                entryType,
                () => parent.GetPath() + "['" + matchedTargetMember.Name + "']",
                matchedTargetMember,
                parent)
        {
            var entryMember = Member.RootSource(entryType);
            _childMembers = new[] { entryMember };

            IsEnumerable = entryMember.IsEnumerable;
            IsSimple = !IsEnumerable && entryMember.IsSimple;
        }

        private DictionaryEntrySourceMember(DictionaryEntrySourceMember parent, Member childMember)
            : this(
                childMember.Type,
                () => parent.GetPath() + "." + childMember.Name,
                parent._matchedTargetMember.Append(childMember),
                parent.Parent,
                parent._childMembers.Append(childMember))
        {
        }

        private DictionaryEntrySourceMember(
            Type type,
            Func<string> pathFactory,
            QualifiedMember matchedTargetMember,
            DictionarySourceMember parent,
            Member[] childMembers)
            : this(type, pathFactory, matchedTargetMember, parent)
        {
            _childMembers = childMembers;

            var leafMember = childMembers.Last();
            IsEnumerable = leafMember.IsEnumerable;
            IsSimple = leafMember.IsSimple;
        }

        private DictionaryEntrySourceMember(
            Type type,
            Func<string> pathFactory,
            QualifiedMember matchedTargetMember,
            DictionarySourceMember parent)
        {
            Type = type;
            _pathFactory = pathFactory;
            _matchedTargetMember = matchedTargetMember;
            Parent = parent;
        }

        public DictionarySourceMember Parent { get; }

        public bool IsRoot => false;

        public Type Type { get; }

        public Type RootType => Parent.Type;

        public string GetFriendlyTypeName() => Type.GetFriendlyName();

        public Type ElementType => _childMembers.Last().ElementType;

        public bool IsEnumerable { get; }

        public bool IsSimple { get; }

        public string Name => _matchedTargetMember.Name;

        public string GetPath() => _pathFactory.Invoke();

        IQualifiedMember IQualifiedMember.GetElementMember() => this.GetElementMember();

        public IQualifiedMember GetInstanceElementMember() => Append(Member.EnumerableElement(Type, Type));

        public IQualifiedMember Append(Member childMember)
            => new DictionaryEntrySourceMember(this, childMember);

        public IQualifiedMember RelativeTo(IQualifiedMember otherMember)
        {
            var otherEntryMember = (DictionaryEntrySourceMember)otherMember;
            var relativeMemberChain = _childMembers.RelativeTo(otherEntryMember._childMembers);

            return new DictionaryEntrySourceMember(
                Type,
                _pathFactory,
                _matchedTargetMember,
                Parent,
                relativeMemberChain);
        }

        public IQualifiedMember WithType(Type runtimeType)
        {
            if (runtimeType == Type)
            {
                return this;
            }

            var childMembers = new Member[_childMembers.Length];
            Array.Copy(_childMembers, 0, childMembers, 0, childMembers.Length - 1);
            childMembers[childMembers.Length - 1] = _childMembers.Last().WithType(runtimeType);

            var dictionaryEntry = new DictionaryEntrySourceMember(
                runtimeType,
                _pathFactory,
                _matchedTargetMember,
                Parent,
                childMembers);

            if (runtimeType.IsDictionary())
            {
                return new DictionarySourceMember(dictionaryEntry, _matchedTargetMember);
            }

            return dictionaryEntry;
        }

        public bool HasCompatibleType(Type type) => Parent.HasCompatibleType(type);

        public bool CouldMatch(QualifiedMember otherMember)
            => _matchedTargetMember.CouldMatch(otherMember);

        public bool Matches(IQualifiedMember otherMember)
        {
            if (otherMember == Parent)
            {
                return Type.IsDictionary();
            }

            return _matchedTargetMember.Matches(otherMember);
        }

        public Expression GetQualifiedAccess(Expression parentInstance)
            => _childMembers.GetQualifiedAccess(parentInstance);

        public IQualifiedMember SetContext(IQualifiedMemberContext context)
        {
            _matchedTargetMember.SetContext(context);
            return this;
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString()
        {
            var entryType = Type.GetFriendlyName();

            return GetPath() + ": " + entryType;
        }
    }
}