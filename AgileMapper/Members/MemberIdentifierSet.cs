namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Configuration;
    using ReadableExpressions.Extensions;

    internal class MemberIdentifierSet
    {
        private readonly Dictionary<Type, LambdaExpression> _identifierNamesByType;

        public MemberIdentifierSet()
        {
            _identifierNamesByType = new Dictionary<Type, LambdaExpression>();
        }

        public void Add(Type type, LambdaExpression idMember)
        {
            if (_identifierNamesByType.ContainsKey(type))
            {
                throw new MappingConfigurationException(
                    $"An identifier has already been configured for type '{type.GetFriendlyName()}'");
            }

            _identifierNamesByType.Add(type, idMember);
        }

        public LambdaExpression GetIdentifierOrNullFor(Type type)
        {
            LambdaExpression identifier;

            if (_identifierNamesByType.TryGetValue(type, out identifier))
            {
                return identifier;
            }

            var matchingKey = _identifierNamesByType.Keys
                .FirstOrDefault(idType => idType.IsAssignableFrom(type));

            return (matchingKey != null) ? _identifierNamesByType[matchingKey] : null;
        }
    }
}