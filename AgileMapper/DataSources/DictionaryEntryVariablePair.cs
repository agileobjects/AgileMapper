namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;
    using NetStandardPolyfills;

    internal class DictionaryEntryVariablePair
    {
        private static readonly MethodInfo _linqFirstOrDefaultMethod = typeof(Enumerable)
            .GetPublicStaticMethods()
            .First(m => (m.Name == "FirstOrDefault") && (m.GetParameters().Length == 2))
            .MakeGenericMethod(typeof(string));

        private static readonly MethodInfo _stringEqualsMethod = typeof(string)
            .GetPublicInstanceMethods()
            .First(m => (m.Name == "Equals") && (m.GetParameters().Length == 2));

        private readonly Type _dictionaryEntryType;
        private readonly string _targetMemberName;
        private ParameterExpression _key;
        private ParameterExpression _value;

        public DictionaryEntryVariablePair(DictionarySourceMember sourceMember, IBasicMapperData mapperData)
        {
            _dictionaryEntryType = sourceMember.EntryType;
            _targetMemberName = mapperData.TargetMember.Name.ToCamelCase();
        }

        public ParameterExpression Key
            => _key ?? (_key = Expression.Variable(typeof(string), _targetMemberName + "Key"));

        public ParameterExpression Value
            => _value ?? (_value = Expression.Variable(_dictionaryEntryType, _targetMemberName.ToCamelCase()));

        public Expression GetMatchingKeyAssignment(IMemberMapperData mapperData)
            => GetMatchingKeyAssignment(GetTargetMemberDictionaryKey(mapperData), mapperData);

        private static Expression GetTargetMemberDictionaryKey(IBasicMapperData childMapperData)
        {
            var joinedName = string.Join(
                string.Empty,
                childMapperData.TargetMember.MemberChain.Skip(1).Select(m => m.JoiningName));

            if (joinedName.StartsWith('.'))
            {
                joinedName = joinedName.Substring(1);
            }

            return Expression.Constant(joinedName, typeof(string));
        }

        public Expression GetMatchingKeyAssignment(Expression targetMemberKey, IMemberMapperData mapperData)
        {
            var keyParameter = Expression.Parameter(typeof(string), "key");

            var parameterEqualsTargetKey = mapperData.IsRoot
                ? (Expression)Expression.Equal(keyParameter, targetMemberKey)
                : Expression.Call(
                    keyParameter,
                    _stringEqualsMethod,
                    targetMemberKey,
                    Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison)));

            var keyMatchesLambda = Expression.Lambda<Func<string, bool>>(parameterEqualsTargetKey, keyParameter);

            var dictionaryKeys = Expression.Property(mapperData.SourceObject, "Keys");

            var firstMatchingKeyOrNull = Expression.Call(
                _linqFirstOrDefaultMethod,
                dictionaryKeys,
                keyMatchesLambda);

            var keyVariableAssignment = Expression.Assign(Key, firstMatchingKeyOrNull);

            return keyVariableAssignment;
        }

        public Expression GetEntryValueAccess(IMemberMapperData mapperData)
            => mapperData.SourceObject.GetIndexAccess(Key);
    }
}