namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class MemberIdentifierSet
    {
        private readonly MapperContext _mapperContext;
        private readonly Dictionary<Type, LambdaExpression> _identifierLambdasByType;

        public MemberIdentifierSet(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
            _identifierLambdasByType = new Dictionary<Type, LambdaExpression>();
        }

        public void Add(Type type, LambdaExpression idMember)
        {
            if (_identifierLambdasByType.ContainsKey(type))
            {
                throw new MappingConfigurationException(
                    $"An identifier has already been configured for type '{type.GetFriendlyName()}'");
            }

            ThrowIfIdentifierIsRedundant(type, idMember);

            _identifierLambdasByType.Add(type, idMember);
        }

        private void ThrowIfIdentifierIsRedundant(Type type, LambdaExpression idMember)
        {
            if (idMember.Body.NodeType != ExpressionType.MemberAccess)
            {
                return;
            }

            var defaultIdentifier = _mapperContext.Naming.GetIdentifierOrNull(type);

            if (defaultIdentifier.Name != idMember.Body.GetMemberName())
            {
                return;
            }

            throw new MappingConfigurationException(
                $"{defaultIdentifier.Name} is automatically used as the identifier for Type '{type.GetFriendlyName()}', " +
                 "and does not need to be configured.");
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