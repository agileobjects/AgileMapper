namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using ReadableExpressions;

    internal class ConfiguredQualifiedMember : IQualifiedMember
    {
        private readonly Expression _value;
        private readonly QualifiedMember _matchedTargetMember;
        private readonly Member[] _childMembers;

        public ConfiguredQualifiedMember(Expression value, IMappingData data)
            : this(value, data.TargetMember)
        {
        }

        private ConfiguredQualifiedMember(Expression value, QualifiedMember matchedTargetMember)
            : this(
                  value.Type,
                  value.ToReadableString(),
                  value,
                  matchedTargetMember)
        {
            IsSimple = value.Type.IsSimple();
            IsEnumerable = !IsSimple && value.Type.IsEnumerable();
            IsComplex = !(IsSimple || IsEnumerable);
        }

        private ConfiguredQualifiedMember(ConfiguredQualifiedMember parent, Member childMember)
            : this(
                  childMember.Type,
                  parent.Name + childMember.MemberName.JoiningName,
                  parent._value,
                  parent._matchedTargetMember.Append(childMember),
                  parent._childMembers.Append(childMember))
        {
            IsSimple = childMember.IsSimple;
            IsEnumerable = childMember.IsEnumerable;
            IsComplex = childMember.IsComplex;
        }

        private ConfiguredQualifiedMember(
            Type type,
            string name,
            Expression value,
            QualifiedMember matchedTargetMember,
            Member[] childMembers = null)
        {
            Type = type;
            Name = name;
            _value = value;
            _matchedTargetMember = matchedTargetMember;
            _childMembers = childMembers ?? new[] { Member.ConfiguredSource(name, type) };
            Signature = string.Join(">", _childMembers.Select(cm => cm.Signature));
        }

        public Type DeclaringType => _value.Type.DeclaringType;

        public Type Type { get; }

        public string Name { get; }

        public bool IsComplex { get; }

        public bool IsEnumerable { get; }

        public bool IsSimple { get; }

        public bool IsReadable => true;

        public string Signature { get; }

        public string Path => Signature;

        public IQualifiedMember Append(Member childMember) => new ConfiguredQualifiedMember(this, childMember);

        public IQualifiedMember RelativeTo(IQualifiedMember otherMember)
        {
            if (_childMembers.None())
            {
                return this;
            }

            var otherConfiguredMember = (ConfiguredQualifiedMember)otherMember;
            var relativeMemberChain = _childMembers.RelativeTo(otherConfiguredMember._childMembers);

            return new ConfiguredQualifiedMember(
                Type,
                Name,
                _value,
                _matchedTargetMember,
                relativeMemberChain);
        }

        public bool IsSameAs(IQualifiedMember otherMember) => false;

        public bool CouldMatch(IQualifiedMember otherMember) => _matchedTargetMember.CouldMatch(otherMember);

        public bool Matches(IQualifiedMember otherMember) => _matchedTargetMember.Matches(otherMember);

        public Expression GetAccess(Expression instance) => _value;

        public Expression GetQualifiedAccess(Expression instance) => _childMembers.GetQualifiedAccess(instance);

        public Expression GetPopulation(Expression instance, Expression value) => Constants.EmptyExpression;

        public IQualifiedMember WithType(Type runtimeType)
        {
            if (runtimeType == Type)
            {
                return this;
            }

            return new ConfiguredQualifiedMember(
                _value.GetConversionTo(runtimeType),
                _matchedTargetMember);
        }
    }
}