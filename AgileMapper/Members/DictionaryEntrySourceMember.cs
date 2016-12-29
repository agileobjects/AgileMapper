namespace AgileObjects.AgileMapper.Members
{
    using System;
#if !NET_STANDARD
    using System.Diagnostics.CodeAnalysis;
#endif
    using System.Linq;
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
            _childMembers = childMembers ?? new[] { Member.RootSource("Source", type) };
        }

        public DictionarySourceMember Parent { get; }

        public Type Type { get; }

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

            return new DictionaryEntrySourceMember(
                runtimeType,
                _pathFactory,
                _matchedTargetMember,
                Parent,
                childMembers);
        }

        public bool CouldMatch(QualifiedMember otherMember) => _matchedTargetMember.CouldMatch(otherMember);

        public bool Matches(IQualifiedMember otherMember)
        {
            return (otherMember == Parent)
                ? Type.IsDictionary()
                : _matchedTargetMember.Matches(otherMember);
        }

        public Expression GetQualifiedAccess(Expression instance) => _childMembers.GetQualifiedAccess(instance);

        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
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