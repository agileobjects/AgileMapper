namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Extensions.Internal;
    using Members;
    using Members.Dictionaries;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class DictionaryEntryVariablePair
    {
        #region Cached MethodInfos

        private static readonly MethodInfo _linqFirstOrDefaultMethod = typeof(Enumerable)
            .GetPublicStaticMethod("FirstOrDefault", parameterCount: 2)
            .MakeGenericMethod(typeof(string));

        private static readonly MethodInfo _stringStartsWithMethod = typeof(string)
            .GetPublicInstanceMethod("StartsWith", typeof(string), typeof(StringComparison));

        #endregion

        private readonly string _targetMemberName;
        private ParameterExpression _key;
        private ParameterExpression _value;

        public DictionaryEntryVariablePair(IMemberMapperData mapperData)
            : this(mapperData.GetDictionarySourceMemberOrNull(), mapperData)
        {
        }

        public DictionaryEntryVariablePair(DictionarySourceMember sourceMember, IMemberMapperData mapperData)
        {
            SourceMember = sourceMember;
            MapperData = mapperData;
            _targetMemberName = GetTargetMemberName(mapperData);
            UseDirectValueAccess = sourceMember.ValueType.IsAssignableTo(mapperData.TargetMember.Type);
            Variables = UseDirectValueAccess ? new[] { Key } : new[] { Key, Value };
        }

        private static string GetTargetMemberName(IBasicMapperData mapperData)
            => mapperData.TargetMember.Name.ToCamelCase();

        public DictionarySourceMember SourceMember { get; }

        public IMemberMapperData MapperData { get; }

        public ICollection<ParameterExpression> Variables { get; }

        public ParameterExpression Key
            => _key ?? (_key = Expression.Variable(SourceMember.KeyType, _targetMemberName + "Key"));

        public ParameterExpression Value
            => _value ?? (_value = Expression.Variable(SourceMember.ValueType, _targetMemberName.ToCamelCase()));

        public bool HasConstantTargetMemberKey
        {
            get
            {
                return TargetMemberKey.NodeType == ExpressionType.Constant ||
                       TargetMemberKey.NodeType == ExpressionType.Parameter;
            }
        }

        public Expression TargetMemberKey { get; private set; }

        public bool UseDirectValueAccess { get; }

        public Expression GetTargetMemberDictionaryEnumerableElementKey(Expression index, IMemberMapperData mapperData)
        {
            var keyParts = mapperData.GetTargetMemberDictionaryKeyParts();
            var elementKeyParts = MapperData.GetTargetMemberDictionaryElementKeyParts(index);

            foreach (var elementKeyPart in elementKeyParts)
            {
                keyParts.Add(elementKeyPart);
            }

            return TargetMemberKey = keyParts.GetStringConcatCall();
        }

        public Expression GetKeyNotFoundShortCircuit(Expression shortCircuitReturn)
        {
            var sourceValueKeyAssignment = GetMatchingKeyAssignment();
            var keyNotFound = Key.GetIsDefaultComparison();
            var ifKeyNotFoundShortCircuit = Expression.IfThen(keyNotFound, shortCircuitReturn);

            return Expression.Block(sourceValueKeyAssignment, ifKeyNotFoundShortCircuit);
        }

        public Expression GetMatchingKeyAssignment()
            => GetMatchingKeyAssignment(MapperData.GetTargetMemberDictionaryKey());

        public Expression GetMatchingKeyAssignment(Expression targetMemberKey)
        {
            TargetMemberKey = targetMemberKey;

            var firstMatchingKeyOrNull = GetKeyMatchingQuery(
                HasConstantTargetMemberKey ? targetMemberKey : Key,
                (keyParameter, targetKey) =>
                {
                    var separator = MapperData.Parent.IsRoot
                        ? null
                        : MapperData.GetDictionaryKeyPartSeparator();

                    var elementKeyPartMatcher = MapperData.Parent.TargetMemberIsEnumerableElement()
                        ? MapperData.GetDictionaryElementKeyPartMatcher()
                        : null;

                    return keyParameter.GetMatchesKeyCall(targetKey, separator, elementKeyPartMatcher);
                },
                Expression.Equal,
                _linqFirstOrDefaultMethod);

            var keyVariableAssignment = GetKeyAssignment(firstMatchingKeyOrNull);

            return keyVariableAssignment;
        }

        public Expression GetNonConstantKeyAssignment() => GetKeyAssignment(TargetMemberKey);

        public Expression GetKeyAssignment(Expression value) => Key.AssignTo(value);

        public Expression GetNoKeysWithMatchingStartQuery(Expression targetMemberKey)
        {
            TargetMemberKey = targetMemberKey;

            var noKeysStartWithTarget = GetKeyMatchingQuery(
                targetMemberKey,
                GetKeyStartsWithIgnoreCaseCall,
                (keyParameter, targetKey) => GetKeyStartsWithCall(keyParameter, targetKey, StringComparison.Ordinal),
                EnumerableExtensions.EnumerableNoneMethod);

            return noKeysStartWithTarget;
        }

        public Expression GetKeyStartsWithIgnoreCaseCall(
            Expression keyAccess,
            Expression targetKey)
        {
            return GetKeyStartsWithCall(keyAccess, targetKey, StringComparison.OrdinalIgnoreCase);
        }

        private static Expression GetKeyStartsWithCall(
            Expression keyParameter,
            Expression targetKey,
            StringComparison comparison)
        {
            return Expression.Call(
                keyParameter,
                _stringStartsWithMethod,
                targetKey,
                comparison.ToConstantExpression());
        }

        private Expression GetKeyMatchingQuery(
            Expression targetMemberKey,
            Func<Expression, Expression, Expression> keyMatcherFactory,
            Func<Expression, Expression, Expression> elementKeyMatcherFactory,
            MethodInfo queryMethod)
        {
            var keyParameter = Expression.Parameter(typeof(string), "key");

            var keyMatcher = MapperData.TargetMemberIsEnumerableElement()
                ? elementKeyMatcherFactory.Invoke(keyParameter, targetMemberKey)
                : keyMatcherFactory.Invoke(keyParameter, targetMemberKey);

            var keyMatchesLambda = Expression.Lambda<Func<string, bool>>(keyMatcher, keyParameter);

            var dictionaryKeys = Expression.Property(MapperData.SourceObject, "Keys");
            var keyMatchesQuery = Expression.Call(queryMethod, dictionaryKeys, keyMatchesLambda);

            return keyMatchesQuery;
        }

        public Expression GetEntryValueAssignment() => Value.AssignTo(GetEntryValueAccess());

        public Expression GetEntryValueAccess() => GetEntryValueAccess(Key);

        public Expression GetEntryValueAccess(Expression key)
            => MapperData.SourceObject.GetIndexAccess(key);
    }
}