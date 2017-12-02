namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Configuration;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    internal class MemberIdentifierSet
    {
        private readonly Dictionary<Type, LambdaExpression> _identifierLambdasByType;

        public MemberIdentifierSet()
        {
            _identifierLambdasByType = new Dictionary<Type, LambdaExpression>();
        }

        public void Add(Type type, LambdaExpression idMember)
        {
            if (_identifierLambdasByType.ContainsKey(type))
            {
                throw new MappingConfigurationException(
                    $"An identifier has already been configured for type '{type.GetFriendlyName()}'");
            }

            _identifierLambdasByType.Add(type, idMember);
        }

        public LambdaExpression GetIdentifierOrNullFor(Type type)
        {
            if (_identifierLambdasByType.TryGetValue(type, out var identifier))
            {
                return identifier;
            }

            var matchingKey = _identifierLambdasByType.Keys.FirstOrDefault(type.IsAssignableTo);

            return (matchingKey != null) ? _identifierLambdasByType[matchingKey] : null;
        }

        public void CloneTo(MemberIdentifierSet identifiers)
        {
            foreach (var idTypeAndLambdaPair in _identifierLambdasByType)
            {
                identifiers._identifierLambdasByType
                    .Add(idTypeAndLambdaPair.Key, idTypeAndLambdaPair.Value);
            }
        }

        public void Reset()
        {
            _identifierLambdasByType.Clear();
        }
    }
}