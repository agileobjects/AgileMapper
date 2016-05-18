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
        private readonly IEnumerable<Member> _childMembers;

        public ConfiguredQualifiedMember(Expression value)
            : this(value, value.ToReadableString())
        {
            IsSimple = value.Type.IsSimple();
            IsEnumerable = !IsSimple && value.Type.IsEnumerable();
            IsComplex = !(IsSimple || IsEnumerable);
        }

        private ConfiguredQualifiedMember(ConfiguredQualifiedMember parent, Member childMember)
            : this(
                parent._value,
                parent.Name,
                childMember.IsSimple,
                childMember.IsEnumerable,
                childMember.IsComplex,
                parent._childMembers.Concat(childMember).ToArray())
        {
        }

        private ConfiguredQualifiedMember(
            Expression value,
            string name,
            bool isSimple = false,
            bool isEnumerable = false,
            bool isComplex = false,
            IEnumerable<Member> childMembers = null)
        {
            _value = value;
            Name = name;
            IsSimple = isSimple;
            IsEnumerable = isEnumerable;
            IsComplex = isComplex;
            _childMembers = childMembers ?? Enumerable.Empty<Member>();
        }

        public Type DeclaringType => _value.Type.DeclaringType;

        public Type Type => _value.Type;

        public string Name { get; }

        public bool IsComplex { get; }

        public bool IsEnumerable { get; }

        public bool IsSimple { get; }

        public bool ExistingValueCanBeChecked => true;

        public IQualifiedMember Append(Member childMember)
            => new ConfiguredQualifiedMember(this, childMember);

        public IQualifiedMember RelativeTo(int depth)
        {
            if (depth == 0)
            {
                return this;
            }

            var members = _childMembers.Skip(depth).ToArray();

            return QualifiedMember.From(members);
        }

        public bool IsSameAs(IQualifiedMember otherMember)
            => (otherMember as ConfiguredQualifiedMember)?._value == _value;

        public bool Matches(IQualifiedMember otherMember)
        {
            throw new NotImplementedException();
        }

        public Expression GetAccess(Expression instance) => _value;

        public Expression GetQualifiedAccess(Expression instance) => _value;

        public Expression GetPopulation(Expression instance, Expression value) => Constants.EmptyExpression;

        public IQualifiedMember WithType(Type runtimeType)
        {
            return runtimeType != Type
                ? new ConfiguredQualifiedMember(_value.GetConversionTo(runtimeType))
                : this;
        }
    }
}