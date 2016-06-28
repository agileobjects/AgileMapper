namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Linq.Expressions;
    using Extensions;
    using ReadableExpressions;

    internal class ConfiguredSourceMember : IQualifiedMember
    {
        private readonly QualifiedMember _matchedTargetMember;
        private readonly Member[] _childMembers;

        public ConfiguredSourceMember(Expression value, IMappingData data)
            : this(value.Type, value.ToReadableString(), data.TargetMember)
        {
        }

        private ConfiguredSourceMember(ConfiguredSourceMember parent, Member childMember)
            : this(
                  childMember.Type,
                  parent.Name + childMember.JoiningName,
                  parent._matchedTargetMember.Append(childMember),
                  parent._childMembers.Append(childMember))
        {
        }

        private ConfiguredSourceMember(
            Type type,
            string name,
            QualifiedMember matchedTargetMember,
            Member[] childMembers = null)
        {
            Type = type;
            Name = name;
            _matchedTargetMember = matchedTargetMember;
            _childMembers = childMembers ?? new[] { Member.RootSource(name, type) };
            Signature = _childMembers.GetSignature();
        }

        public Type Type { get; }

        public string Name { get; }

        public string Signature { get; }

        public string GetPath() => _childMembers.GetFullName();

        public IQualifiedMember Append(Member childMember) => new ConfiguredSourceMember(this, childMember);

        public IQualifiedMember RelativeTo(IQualifiedMember otherMember)
        {
            var otherConfiguredMember = (ConfiguredSourceMember)otherMember;
            var relativeMemberChain = _childMembers.RelativeTo(otherConfiguredMember._childMembers);

            return new ConfiguredSourceMember(
                Type,
                Name,
                _matchedTargetMember,
                relativeMemberChain);
        }

        public bool CouldMatch(QualifiedMember otherMember) => _matchedTargetMember.CouldMatch(otherMember);

        public bool Matches(IQualifiedMember otherMember) => _matchedTargetMember.Matches(otherMember);

        public Expression GetQualifiedAccess(Expression instance) => _childMembers.GetQualifiedAccess(instance);

        public IQualifiedMember WithType(Type runtimeType) => this;
    }
}