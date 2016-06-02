namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using ReadableExpressions;

    internal class ConfiguredQualifiedMember : IQualifiedMember
    {
        private readonly Expression _value;
        private readonly IQualifiedMember _matchedTargetMember;
        private readonly IEnumerable<Member> _childMembers;

        public ConfiguredQualifiedMember(Expression value, IMappingData data)
            : this(value, data.TargetMember)
        {
        }

        private ConfiguredQualifiedMember(Expression value, IQualifiedMember matchedTargetMember)
            : this(
                  value.Type,
                  value.ToReadableString(),
                  value,
                  matchedTargetMember,
                  Enumerable.Empty<Member>())
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
                  parent._childMembers.Concat(childMember).ToArray())
        {
            IsSimple = childMember.IsSimple;
            IsEnumerable = childMember.IsEnumerable;
            IsComplex = childMember.IsComplex;
        }

        private ConfiguredQualifiedMember(
            Type type,
            string name,
            Expression value,
            IQualifiedMember matchedTargetMember,
            IEnumerable<Member> childMembers)
        {
            Type = type;
            Name = name;
            _value = value;
            _matchedTargetMember = matchedTargetMember;
            _childMembers = childMembers;
            Signature = string.Join(">", new[] { value.ToString() }.Concat(childMembers.Select(cm => cm.Signature)));
        }

        public Type DeclaringType => _value.Type.DeclaringType;

        public Type Type { get; }

        public string Name { get; }

        public bool IsComplex { get; }

        public bool IsEnumerable { get; }

        public bool IsSimple { get; }

        public bool IsReadable => true;

        public string Signature { get; }

        public IQualifiedMember Append(Member childMember)
            => new ConfiguredQualifiedMember(this, childMember);

        public IQualifiedMember RelativeTo(int depth) => this;

        public bool IsSameAs(IQualifiedMember otherMember) => false;

        public bool Matches(IQualifiedMember otherMember) => _matchedTargetMember.Matches(otherMember);

        public Expression GetAccess(Expression instance) => _value;

        public Expression GetQualifiedAccess(Expression instance) => _childMembers.GetQualifiedAccess(instance);

        public Expression GetPopulation(Expression instance, Expression value) => Constants.EmptyExpression;

        public IQualifiedMember WithType(Type runtimeType)
        {
            return runtimeType != Type
                ? new ConfiguredQualifiedMember(_value.GetConversionTo(runtimeType), _matchedTargetMember)
                : this;
        }
    }
}