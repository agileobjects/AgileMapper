namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;
    using NetStandardPolyfills;

    internal class DictionaryEntryVariablePair
    {
        #region Cached MethodInfos

        private static readonly MethodInfo _linqFirstOrDefaultMethod = typeof(Enumerable)
            .GetPublicStaticMethods()
            .First(m => (m.Name == "FirstOrDefault") && (m.GetParameters().Length == 2))
            .MakeGenericMethod(typeof(string));

        private static readonly MethodInfo _enumerableNoneMethod = typeof(EnumerableExtensions)
            .GetPublicStaticMethods()
            .First(m => (m.Name == "None") && (m.GetParameters().Length == 2))
            .MakeGenericMethod(typeof(string));

        private static readonly MethodInfo _stringEqualsMethod = typeof(string)
            .GetPublicInstanceMethods()
            .First(m => (m.Name == "Equals") && (m.GetParameters().Length == 2));

        private static readonly MethodInfo _stringStartsWithMethod = typeof(string)
            .GetPublicInstanceMethods()
            .First(m => (m.Name == "StartsWith") && (m.GetParameters().Length == 2));

        private static readonly MethodInfo _stringConcatTwoMethod = typeof(string)
            .GetPublicStaticMethods()
            .First(m => (m.Name == "Concat") &&
                        (m.GetParameters().Length == 2) &&
                        (m.GetParameters().First().ParameterType == typeof(string)));

        private static readonly MethodInfo _stringConcatThreeMethod = typeof(string)
            .GetPublicStaticMethods()
            .First(m => (m.Name == "Concat") &&
                        (m.GetParameters().Length == 3) &&
                        (m.GetParameters().First().ParameterType == typeof(string)));

        #endregion

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

        private static Expression GetTargetMemberDictionaryKey(IMemberMapperData childMapperData)
        {
            var mapperData = childMapperData;
            var joinedName = string.Empty;
            var targetMemberIsNotWithinEnumerable = true;
            var memberPartExpressions = new List<Expression>();

            while (!mapperData.IsRoot)
            {
                if (mapperData.TargetMemberIsEnumerableElement())
                {
                    var index = mapperData.Parent.EnumerablePopulationBuilder.Counter;
                    var indexString = GetTargetMemberDictionaryElementKey(mapperData, index);

                    memberPartExpressions.Add(indexString);

                    targetMemberIsNotWithinEnumerable = false;
                    mapperData = mapperData.Parent;
                    continue;
                }

                var memberName = mapperData.TargetMember.LeafMember.JoiningName;

                memberPartExpressions.Add(Expression.Constant(memberName, typeof(string)));

                joinedName = memberName + joinedName;
                mapperData = mapperData.Parent;
            }

            if (targetMemberIsNotWithinEnumerable)
            {
                if (joinedName.StartsWith('.'))
                {
                    joinedName = joinedName.Substring(1);
                }

                return Expression.Constant(joinedName, typeof(string));
            }

            var targetMemberKeyConcatenation = memberPartExpressions.Chain(
                firstPart => firstPart,
                (namePart, nameSoFar) => Expression.Call(_stringConcatTwoMethod, nameSoFar, namePart));

            return targetMemberKeyConcatenation;
        }

        public Expression GetTargetMemberDictionaryEnumerableElementKey(
            IMemberMapperData mapperData,
            Expression index)
        {
            return GetTargetMemberDictionaryElementKey(
                mapperData,
                index,
                mapperData.IsRoot ? null : mapperData.TargetMember.Name);
        }

        private static Expression GetTargetMemberDictionaryElementKey(
            IMemberMapperData mapperData,
            Expression index,
            string memberName = null)
        {
            var openBrace = Expression.Constant(memberName + "[");
            var indexString = mapperData.MapperContext.ValueConverters.GetConversion(index, typeof(string));
            var closeBrace = Expression.Constant("]");

            var nameConstant = Expression.Call(
                null,
                _stringConcatThreeMethod,
                openBrace,
                indexString,
                closeBrace);

            return nameConstant;
        }

        public Expression GetMatchingKeyAssignment(Expression targetMemberKey, IMemberMapperData mapperData)
        {
            var firstMatchingKeyOrNull = GetKeyMatchingQuery(
                targetMemberKey,
                Expression.Equal,
                (keyParameter, targetKey) => Expression.Call(
                    keyParameter,
                    _stringEqualsMethod,
                    targetKey,
                    Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison))),
                _linqFirstOrDefaultMethod,
                mapperData);

            var keyVariableAssignment = Expression.Assign(Key, firstMatchingKeyOrNull);

            return keyVariableAssignment;
        }

        public Expression GetNoKeysWithMatchingStartQuery(IMemberMapperData mapperData)
        {
            var noKeysStartWithTarget = GetKeyMatchingQuery(
                Key,
                (keyParameter, targetKey) => GetKeyStartsWithCall(keyParameter, targetKey, StringComparison.Ordinal),
                (keyParameter, targetKey) => GetKeyStartsWithCall(keyParameter, targetKey, StringComparison.OrdinalIgnoreCase),
                _enumerableNoneMethod,
                mapperData);

            return noKeysStartWithTarget;
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
                Expression.Constant(comparison, typeof(StringComparison)));
        }

        private static Expression GetKeyMatchingQuery(
            Expression targetMemberKey,
            Func<Expression, Expression, Expression> rootKeyMatcherFactory,
            Func<Expression, Expression, Expression> nestedKeyMatcherFactory,
            MethodInfo queryMethod,
            IMemberMapperData mapperData)
        {
            var keyParameter = Expression.Parameter(typeof(string), "key");

            var keyMatchesQuery = mapperData.IsRoot
                ? rootKeyMatcherFactory.Invoke(keyParameter, targetMemberKey)
                : nestedKeyMatcherFactory.Invoke(keyParameter, targetMemberKey);

            var keyMatchesLambda = Expression.Lambda<Func<string, bool>>(keyMatchesQuery, keyParameter);

            var dictionaryKeys = Expression.Property(mapperData.SourceObject, "Keys");
            var keyMatchesQueryCall = Expression.Call(queryMethod, dictionaryKeys, keyMatchesLambda);

            return keyMatchesQueryCall;
        }

        public Expression GetEntryValueAccess(IMemberMapperData mapperData)
            => mapperData.SourceObject.GetIndexAccess(Key);
    }
}