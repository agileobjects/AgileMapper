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

        private static readonly MethodInfo _stringStartsWithMethod = typeof(string)
            .GetPublicInstanceMethods()
            .First(m => (m.Name == "StartsWith") && (m.GetParameters().Length == 2));

        private static readonly MethodInfo _stringJoinMethod = typeof(string)
            .GetPublicStaticMethods()
            .First(m => (m.Name == "Join") &&
                        (m.GetParameters().Length == 2) &&
                        (m.GetParameters()[1].ParameterType == typeof(string[])));

        private static readonly MethodInfo[] _stringConcatMethods = typeof(string)
            .GetPublicStaticMethods()
            .Where(m => m.Name == "Concat")
            .Select(m => new
            {
                Method = m,
                Parameters = m.GetParameters(),
                FirstParameterType = m.GetParameters().First().ParameterType
            })
            .Where(m => m.FirstParameterType == typeof(string))
            .OrderBy(m => m.Parameters.Length)
            .Select(m => m.Method)
            .ToArray();

        #endregion

        private readonly IMemberMapperData _mapperData;
        private readonly Type _dictionaryEntryType;
        private readonly string _targetMemberName;
        private ParameterExpression _key;
        private ParameterExpression _value;

        public DictionaryEntryVariablePair(DictionarySourceMember sourceMember, IMemberMapperData mapperData)
        {
            _mapperData = mapperData;
            SourceMember = sourceMember;
            _dictionaryEntryType = sourceMember.EntryType;
            _targetMemberName = mapperData.TargetMember.Name.ToCamelCase();
            UseDirectValueAccess = mapperData.TargetMember.Type.IsAssignableFrom(_dictionaryEntryType);
        }

        public DictionarySourceMember SourceMember { get; }

        public ParameterExpression Key
            => _key ?? (_key = Expression.Variable(typeof(string), _targetMemberName + "Key"));

        public ParameterExpression Value
            => _value ?? (_value = Expression.Variable(_dictionaryEntryType, _targetMemberName.ToCamelCase()));

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

        public Expression GetTargetMemberDictionaryEnumerableElementKey(Expression index)
        {
            var keyParts = GetTargetMemberDictionaryKeyParts();
            var elementKeyParts = GetTargetMemberDictionaryElementKeyParts(_mapperData, index);

            foreach (var elementKeyPart in elementKeyParts)
            {
                keyParts.Add(elementKeyPart);
            }

            OptimiseNamePartsForStringConcat(keyParts);

            return (TargetMemberKey = GetStringConcatCall(keyParts));
        }

        public Expression GetMatchingKeyAssignment()
            => GetMatchingKeyAssignment(GetTargetMemberDictionaryKey());

        private Expression GetTargetMemberDictionaryKey()
        {
            var configuredKey = _mapperData.MapperContext
                .UserConfigurations
                .Dictionaries
                .GetFullKeyOrNull(_mapperData);

            if (configuredKey != null)
            {
                return configuredKey;
            }

            var keyParts = GetTargetMemberDictionaryKeyParts();

            OptimiseNamePartsForStringConcat(keyParts);

            if (keyParts.HasOne() && (keyParts.First().NodeType == ExpressionType.Constant))
            {
                return keyParts.First();
            }

            return GetStringConcatCall(keyParts);
        }

        private IList<Expression> GetTargetMemberDictionaryKeyParts()
        {
            var joinedName = string.Empty;
            var memberPartExpressions = new List<Expression>();
            var joinedNameIsConstant = true;
            var mapperData = _mapperData;

            while (!mapperData.IsRoot)
            {
                if (mapperData.TargetMemberIsEnumerableElement())
                {
                    AddEnumerableMemberNamePart(memberPartExpressions, mapperData);
                    joinedNameIsConstant = false;
                }
                else
                {
                    var namePart = AddMemberNamePart(memberPartExpressions, mapperData);
                    joinedNameIsConstant = joinedNameIsConstant && namePart.NodeType == ExpressionType.Constant;

                    if (joinedNameIsConstant)
                    {
                        joinedName = (string)((ConstantExpression)namePart).Value + joinedName;
                    }
                }

                mapperData = mapperData.Parent;
            }

            if (joinedNameIsConstant)
            {
                memberPartExpressions.Clear();
                memberPartExpressions.Add(Expression.Constant(joinedName, typeof(string)));
            }

            return memberPartExpressions;
        }

        private static void AddEnumerableMemberNamePart(
            List<Expression> memberPartExpressions,
            IMemberMapperData mapperData)
        {
            var index = mapperData.Parent.EnumerablePopulationBuilder.Counter;
            var elementKeyParts = GetTargetMemberDictionaryElementKeyParts(mapperData, index);

            memberPartExpressions.InsertRange(0, elementKeyParts);
        }

        private static IEnumerable<Expression> GetTargetMemberDictionaryElementKeyParts(
            IMemberMapperData mapperData,
            Expression index)
        {
            yield return Expression.Constant("[");
            yield return mapperData.MapperContext.ValueConverters.GetConversion(index, typeof(string));
            yield return Expression.Constant("]");
        }

        private static Expression AddMemberNamePart(
            IList<Expression> memberPartExpressions,
            IMemberMapperData mapperData)
        {
            var dictionarySettings = mapperData.MapperContext.UserConfigurations.Dictionaries;

            var memberName = dictionarySettings
                .GetMemberKeyOrNull(mapperData) ?? mapperData.TargetMember.LeafMember.JoiningName;

            var memberNamePart = dictionarySettings.GetJoiningName(memberName, mapperData);

            memberPartExpressions.Insert(0, memberNamePart);

            return memberNamePart;
        }

        private static Expression GetStringConcatCall(ICollection<Expression> elements)
        {
            if (_stringConcatMethods.Length >= elements.Count - 1)
            {
                var concatMethod = _stringConcatMethods[elements.Count - 2];

                return Expression.Call(null, concatMethod, elements);
            }

            var emptyString = Expression.Field(null, typeof(string), "Empty");
            var newStringArray = Expression.NewArrayInit(typeof(string), elements);

            return Expression.Call(null, _stringJoinMethod, emptyString, newStringArray);
        }

        private static void OptimiseNamePartsForStringConcat(IList<Expression> nameParts)
        {
            if (nameParts.HasOne())
            {
                return;
            }

            var currentNamePart = string.Empty;

            for (var i = nameParts.Count - 1; i >= 0; --i)
            {
                var namePart = nameParts[i];

                if (namePart.NodeType == ExpressionType.Constant)
                {
                    if ((i == 0) && (currentNamePart == string.Empty))
                    {
                        return;
                    }

                    currentNamePart = (string)((ConstantExpression)namePart).Value + currentNamePart;
                    nameParts.RemoveAt(i);
                    continue;
                }

                if (currentNamePart == string.Empty)
                {
                    continue;
                }

                nameParts.Insert(i + 1, Expression.Constant(currentNamePart, typeof(string)));
                currentNamePart = string.Empty;
            }

            nameParts.Insert(0, Expression.Constant(currentNamePart, typeof(string)));
        }

        public Expression GetMatchingKeyAssignment(Expression targetMemberKey)
        {
            TargetMemberKey = targetMemberKey;

            var firstMatchingKeyOrNull = GetKeyMatchingQuery(
                HasConstantTargetMemberKey ? targetMemberKey : Key,
                Expression.Equal,
                (keyParameter, targetKey) => keyParameter.GetCaseInsensitiveEquals(targetKey),
                _linqFirstOrDefaultMethod);

            var keyVariableAssignment = GetKeyAssignment(firstMatchingKeyOrNull);

            return keyVariableAssignment;
        }

        public Expression GetKeyAssignment(Expression value) => Expression.Assign(Key, value);

        public Expression GetNoKeysWithMatchingStartQuery(Expression targetMemberKey)
        {
            TargetMemberKey = targetMemberKey;

            var noKeysStartWithTarget = GetKeyMatchingQuery(
                targetMemberKey,
                (keyParameter, targetKey) => GetKeyStartsWithCall(keyParameter, targetKey, StringComparison.Ordinal),
                (keyParameter, targetKey) => GetKeyStartsWithCall(keyParameter, targetKey, StringComparison.OrdinalIgnoreCase),
                _enumerableNoneMethod);

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

        private Expression GetKeyMatchingQuery(
            Expression targetMemberKey,
            Func<Expression, Expression, Expression> rootKeyMatcherFactory,
            Func<Expression, Expression, Expression> nestedKeyMatcherFactory,
            MethodInfo queryMethod)
        {
            var keyParameter = Expression.Parameter(typeof(string), "key");

            var keyMatcher = _mapperData.IsRoot
                ? rootKeyMatcherFactory.Invoke(keyParameter, targetMemberKey)
                : nestedKeyMatcherFactory.Invoke(keyParameter, targetMemberKey);

            var keyMatchesLambda = Expression.Lambda<Func<string, bool>>(keyMatcher, keyParameter);

            var dictionaryKeys = Expression.Property(_mapperData.SourceObject, "Keys");
            var keyMatchesQuery = Expression.Call(queryMethod, dictionaryKeys, keyMatchesLambda);

            return keyMatchesQuery;
        }

        public Expression GetEntryValueAccess() => GetEntryValueAccess(Key);

        public Expression GetEntryValueAccess(Expression key)
            => _mapperData.SourceObject.GetIndexAccess(key);
    }
}