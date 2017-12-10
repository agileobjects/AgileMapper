namespace AgileObjects.AgileMapper.Members.Dictionaries
{
    using System;
    using System.Linq.Expressions;
    using Extensions;
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
        }

        public DictionaryEntrySourceMember(DictionaryEntrySourceMember parent, Member childMember)
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
            Member[] childMembers = null)
        {
            Type = type;
            IsEnumerable = type.IsEnumerable();
            _pathFactory = pathFactory;
            _matchedTargetMember = matchedTargetMember;
            Parent = parent;
            _childMembers = childMembers ?? new[] { Member.RootSource(type) };
        }

        public DictionarySourceMember Parent { get; }

        public Type Type { get; }

        public string GetFriendlyTypeName() => Type.GetFriendlyName();

        public Type ElementType => _childMembers.First().ElementType;

        public bool IsEnumerable { get; }

        public string Name => _matchedTargetMember.Name;

        public string GetPath() => _pathFactory.Invoke();

        IQualifiedMember IQualifiedMember.GetElementMember() => this.GetElementMember();

        public IQualifiedMember GetInstanceElementMember() => Append(Member.EnumerableElement(Type, Type));

        public IQualifiedMember Append(Member childMember) => new DictionaryEntrySourceMember(this, childMember);

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

        public bool HasCompatibleType(Type type) => false;

        public bool CouldMatch(QualifiedMember otherMember) => _matchedTargetMember.CouldMatch(otherMember);

        public bool Matches(IQualifiedMember otherMember)
        {
            return (otherMember == Parent)
                ? Type.IsDictionary()
                : _matchedTargetMember.Matches(otherMember);
        }

        public Expression GetQualifiedAccess(IMemberMapperData mapperData) => _childMembers.GetQualifiedAccess(mapperData);

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