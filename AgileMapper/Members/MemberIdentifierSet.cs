namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Api.Configuration;
    using ReadableExpressions.Extensions;

    internal class MemberIdentifierSet
    {
        private readonly MemberFinder _memberFinder;
        private readonly Dictionary<Type, Member> _identifierNamesByType;

        public MemberIdentifierSet(MemberFinder memberFinder)
        {
            _memberFinder = memberFinder;
            _identifierNamesByType = new Dictionary<Type, Member>();
        }

        public Member GetIdentifierOrNullFor(Type type)
        {
            Member identifier;

            return _identifierNamesByType.TryGetValue(type, out identifier) ? identifier : null;
        }

        public void Add(Type type, LambdaExpression idMember)
        {
            if (_identifierNamesByType.ContainsKey(type))
            {
                throw new MappingConfigurationException(
                    $"An identifier has already been configured for type '{type.GetFriendlyName()}'");
            }

            var identifier = idMember.ToSourceMember(_memberFinder).LeafMember;

            _identifierNamesByType.Add(type, identifier);
        }
    }
}