namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    [DebuggerDisplay("{GetPath()}")]
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
                () => parent.GetPath() + "." + matchedTargetMember.Name,
                matchedTargetMember)
        {
        }

        private DictionaryEntrySourceMember(DictionaryEntrySourceMember parent, Member childMember)
            : this(
                childMember.Type,
                () => parent.GetPath() + "." + childMember.Name,
                parent._matchedTargetMember.Append(childMember),
                parent._childMembers.Append(childMember))
        {
        }

        private DictionaryEntrySourceMember(
            Type type,
            Func<string> pathFactory,
            QualifiedMember matchedTargetMember,
            Member[] childMembers = null)
        {
            Type = type;
            IsEnumerable = type.IsEnumerable();
            _pathFactory = pathFactory;
            _matchedTargetMember = matchedTargetMember;
            _childMembers = childMembers ?? new[] { Member.RootSource("Source", type) };
        }

        public Type Type { get; }

        public bool IsEnumerable { get; }

        public string Name => _matchedTargetMember.Name;

        public string GetPath() => _pathFactory.Invoke();

        public IQualifiedMember Append(Member childMember) => new DictionaryEntrySourceMember(this, childMember);

        public IQualifiedMember RelativeTo(IQualifiedMember otherMember)
        {
            var otherEntryMember = (DictionaryEntrySourceMember)otherMember;
            var relativeMemberChain = _childMembers.RelativeTo(otherEntryMember._childMembers);

            return new DictionaryEntrySourceMember(
                Type,
                _pathFactory,
                _matchedTargetMember,
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
                childMembers);
        }

        public bool CouldMatch(QualifiedMember otherMember) => _matchedTargetMember.CouldMatch(otherMember);

        public bool Matches(QualifiedMember otherMember) => _matchedTargetMember.Matches(otherMember);

        public Expression GetQualifiedAccess(Expression instance) => _childMembers.GetQualifiedAccess(instance);
    }
}