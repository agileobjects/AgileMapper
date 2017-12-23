namespace AgileObjects.AgileMapper.Members.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
    using static System.Linq.Expressions.ExpressionType;

    internal class DictionaryTargetMember : QualifiedMember
    {
        private readonly DictionaryTargetMember _rootDictionaryMember;
        private bool _createDictionaryChildMembers;
        private Expression _key;
        //private ICache<Member, Expression>

        public DictionaryTargetMember(QualifiedMember wrappedTargetMember)
            : base(wrappedTargetMember.MemberChain, wrappedTargetMember)
        {
            var dictionaryTypes = wrappedTargetMember.Type.GetDictionaryTypes();
            KeyType = dictionaryTypes.Key;
            ValueType = dictionaryTypes.Value;
            _rootDictionaryMember = this;
            _createDictionaryChildMembers = true;
        }

        private DictionaryTargetMember(
            QualifiedMember matchedTargetMember,
            DictionaryTargetMember rootDictionaryMember)
            : base(matchedTargetMember.MemberChain, matchedTargetMember)
        {
            KeyType = rootDictionaryMember.KeyType;
            ValueType = rootDictionaryMember.ValueType;
            _rootDictionaryMember = rootDictionaryMember;
            _createDictionaryChildMembers = HasObjectEntries || HasSimpleEntries;
        }

        public override string RegistrationName => GetKeyNameOrNull() ?? base.RegistrationName;

        public Type KeyType { get; }

        public Type ValueType { get; }

        public bool HasObjectEntries => ValueType == typeof(object);

        public bool HasSimpleEntries => ValueType.IsSimple();

        public bool HasEnumerableEntries => ValueType.IsEnumerable();

        public bool HasComplexEntries => !HasObjectEntries && ValueType.IsComplex();

        public override Type GetElementType(Type sourceElementType)
        {
            if (HasObjectEntries || HasSimpleEntries)
            {
                return sourceElementType;
            }

            return base.GetElementType(sourceElementType);
        }

        public override bool GuardObjectValuePopulations => true;

        public override bool HasCompatibleType(Type type)
        {
            if (type == typeof(ExpandoObject))
            {
                return _rootDictionaryMember.Type == typeof(ExpandoObject);
            }

            if (base.HasCompatibleType(type))
            {
                return true;
            }

            if (this != _rootDictionaryMember)
            {
                return false;
            }

            return (_rootDictionaryMember.Type != typeof(ExpandoObject)) && type.IsDictionary();
        }

        public DictionaryTargetMember Append(ParameterExpression key)
        {
            var memberKey = new DictionaryMemberKey(ValueType, key.Name, this);
            var childMember = Append(memberKey);

            childMember._key = key;

            return childMember;
        }

        public DictionaryTargetMember Append(Type entryDeclaringType, string entryKey)
        {
            var memberKey = new DictionaryMemberKey(entryDeclaringType, entryKey, this);

            return Append(memberKey);
        }

        private DictionaryTargetMember Append(DictionaryMemberKey memberKey)
        {
            var targetEntryMember = GlobalContext.Instance.Cache.GetOrAdd(
                memberKey,
                key =>
                {
                    var member = key.GetDictionaryEntryMember();

                    key.DictionaryMember = null;

                    return member;
                });

            var childMember = Append(targetEntryMember);

            return (DictionaryTargetMember)childMember;
        }

        protected override QualifiedMember CreateChildMember(Member childMember)
        {
            var matchedTargetEntryMember = base.CreateChildMember(childMember);

            if (_createDictionaryChildMembers)
            {
                return new DictionaryTargetMember(matchedTargetEntryMember, _rootDictionaryMember);
            }

            return matchedTargetEntryMember;
        }

        protected override QualifiedMember CreateRuntimeTypedMember(Type runtimeType)
        {
            var runtimeTypedTargetEntryMember = base.CreateRuntimeTypedMember(runtimeType);

            return new DictionaryTargetMember(runtimeTypedTargetEntryMember, _rootDictionaryMember)
            {
                _createDictionaryChildMembers = _createDictionaryChildMembers,
                _key = _key
            };
        }

        public override bool Matches(IQualifiedMember otherMember)
        {
            if (_key == null)
            {
                return base.Matches(otherMember);
            }

            return GetKeyNameOrNull() == otherMember.Name;
        }

        public override Expression GetAccess(Expression instance, IMemberMapperData mapperData)
        {
            if (this == _rootDictionaryMember)
            {
                return base.GetAccess(instance, mapperData);
            }

            if (ReturnKeyedAccess())
            {
                return GetKeyedAccess(mapperData);
            }

            return Type.ToDefaultExpression();
        }

        private bool ReturnKeyedAccess() => (Type == ValueType) || Type.IsSimple();

        private Expression GetKeyedAccess(IMemberMapperData mapperData)
        {
            var key = GetKey(mapperData);
            var dictionaryAccess = GetDictionaryAccess(mapperData);
            var keyedAccess = dictionaryAccess.GetIndexAccess(key);

            return keyedAccess;
        }

        private Expression GetKey(IMemberMapperData mapperData)
            => _key ?? mapperData.GetValueConversion(mapperData.GetTargetMemberDictionaryKey(), KeyType);

        private Expression GetDictionaryAccess(IMemberMapperData mapperData)
        {
            var parentContextAccess = mapperData.GetAppropriateMappingContextAccess(typeof(object), _rootDictionaryMember.Type);

            if (parentContextAccess.NodeType != Parameter)
            {
                return MemberMapperDataExtensions.GetTargetAccess(parentContextAccess, _rootDictionaryMember.Type);
            }

            var dictionaryMapperData = mapperData;

            while (dictionaryMapperData.TargetMember != _rootDictionaryMember)
            {
                dictionaryMapperData = dictionaryMapperData.Parent;
            }

            return dictionaryMapperData.TargetInstance;
        }

        public override bool CheckExistingElementValue => !HasObjectEntries && !HasSimpleEntries;

        public override Expression GetHasDefaultValueCheck(IMemberMapperData mapperData)
        {
            var tryGetValueCall = GetTryGetValueCall(mapperData, out var existingValueVariable);
            var existingValueIsDefault = existingValueVariable.GetIsDefaultComparison();

            var valueMissingOrDefault = Expression.OrElse(Expression.Not(tryGetValueCall), existingValueIsDefault);

            return Expression.Block(new[] { existingValueVariable }, valueMissingOrDefault);
        }

        public override BlockExpression GetAccessChecked(IMemberMapperData mapperData)
        {
            var tryGetValueCall = GetTryGetValueCall(mapperData, out var existingValueVariable);

            return Expression.Block(new[] { existingValueVariable }, tryGetValueCall);
        }

        private Expression GetTryGetValueCall(IMemberMapperData mapperData, out ParameterExpression valueVariable)
        {
            var dictionaryAccess = GetDictionaryAccess(mapperData);
            var tryGetValueMethod = dictionaryAccess.Type.GetDictionaryType().GetPublicInstanceMethod("TryGetValue");
            var key = GetKey(mapperData);
            valueVariable = Expression.Variable(ValueType, "existingValue");

            var tryGetValueCall = Expression.Call(
                dictionaryAccess,
                tryGetValueMethod,
                key,
                valueVariable);

            return tryGetValueCall;
        }

        public void SetCustomKey(string key)
        {
            _key = key.ToConstantExpression();
        }

        public override Expression GetPopulation(Expression value, IMemberMapperData mapperData)
        {
            if (mapperData.TargetMember.IsRecursion)
            {
                return value;
            }

            if (this == _rootDictionaryMember)
            {
                return base.GetPopulation(value, mapperData);
            }

            if (ValueIsFlattening(value, out var flattening))
            {
                return flattening;
            }

            var keyedAccess = this.GetAccess(mapperData);

            var convertedValue = HasComplexEntries
                ? GetCheckedValue((BlockExpression)value, keyedAccess, mapperData)
                : mapperData.GetValueConversion(value, ValueType);

            var keyedAssignment = keyedAccess.AssignTo(convertedValue);

            return keyedAssignment;
        }

        private bool ValueIsFlattening(Expression value, out Expression flattening)
        {
            if (!(HasObjectEntries || HasSimpleEntries))
            {
                flattening = null;
                return false;
            }

            if (value.NodeType == Try)
            {
                value = ((TryExpression)value).Body;
            }

            if ((value.NodeType != Block))
            {
                flattening = null;
                return false;
            }

            var flatteningBlock = (BlockExpression)value;
            var flatteningVariables = flatteningBlock.Variables.ToList();

            if (flatteningBlock.Expressions[0].NodeType == Try)
            {
                flatteningBlock = (BlockExpression)((TryExpression)flatteningBlock.Expressions[0]).Body;
                flatteningVariables.AddRange(flatteningBlock.Variables);
            }
            else
            {
                flatteningBlock = (BlockExpression)value;
            }

            if (flatteningBlock.Expressions.HasOne())
            {
                flattening = null;
                return false;
            }

            flattening = flatteningBlock;

            var flatteningExpressions = GetMappingExpressions(flattening);

            if (flatteningExpressions.HasOne() &&
               (flatteningExpressions[0].NodeType == Block))
            {
                flatteningBlock = (BlockExpression)flatteningExpressions[0];
                flatteningVariables.AddRange(flatteningBlock.Variables);
                flattening = flatteningBlock.Update(flatteningVariables, flatteningBlock.Expressions);
                return true;
            }

            flattening = flatteningVariables.Any()
                ? Expression.Block(flatteningVariables, flatteningExpressions)
                : flatteningExpressions.HasOne()
                    ? flatteningExpressions[0]
                    : Expression.Block(flatteningExpressions);

            return true;
        }

        private static IList<Expression> GetMappingExpressions(Expression mapping)
        {
            var expressions = new List<Expression>();

            while (mapping.NodeType == Block)
            {
                var mappingBlock = (BlockExpression)mapping;

                expressions.AddRange(mappingBlock.Expressions.Except(new[] { mappingBlock.Result }));
                mapping = mappingBlock.Result;
            }

            return expressions;
        }

        private Expression GetCheckedValue(BlockExpression value, Expression keyedAccess, IMemberMapperData mapperData)
        {
            if (mapperData.SourceMember.IsEnumerable)
            {
                return value;
            }

            var checkedAccess = GetAccessChecked(mapperData);
            var existingValue = checkedAccess.Variables.First();
            var replacements = new ExpressionReplacementDictionary(1) { [keyedAccess] = existingValue };
            var checkedValue = value.Replace(replacements);

            return checkedValue.Update(
                checkedValue.Variables.Append(existingValue),
                checkedValue.Expressions.Prepend(checkedAccess.Expressions.First()));
        }

        public DictionaryTargetMember WithTypeOf(Member sourceMember)
        {
            if (sourceMember.Type == Type)
            {
                return this;
            }

            return (DictionaryTargetMember)WithType(sourceMember.Type);
        }

        public override void MapCreating(IQualifiedMember sourceMember)
        {
            if (CreateNonDictionaryChildMembers(sourceMember))
            {
                _createDictionaryChildMembers = false;
            }

            base.MapCreating(sourceMember);
        }

        private bool CreateNonDictionaryChildMembers(IQualifiedMember sourceMember)
        {
            // If this DictionaryTargetMember represents an object-typed dictionary 
            // entry and we're mapping from a source of type object, we switch from
            // mapping to flattened entries to mapping entire objects:
            return HasObjectEntries &&
                   LeafMember.IsEnumerableElement() &&
                  (MemberChain[MemberChain.Length - 2] == _rootDictionaryMember.LeafMember) &&
                  (sourceMember.Type == typeof(object));
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString()
        {
            if (LeafMember.IsRoot)
            {
                return base.ToString();
            }

            var path = GetKeyNameOrNull() ?? GetPath().Substring("Target.".Length);

            return $"[\"{path}\"]: {Type.GetFriendlyName()}";
        }

        private string GetKeyNameOrNull() => (string)((ConstantExpression)_key)?.Value;

        #region Helper Classes

        private class DictionaryMemberKey
        {
            private readonly Type _entryDeclaringType;
            private readonly Type _entryValueType;
            private readonly string _entryKey;

            public DictionaryMemberKey(
                Type entryDeclaringType,
                string entryKey,
                DictionaryTargetMember dictionaryMember)
            {
                _entryValueType = dictionaryMember.ValueType;
                _entryDeclaringType = entryDeclaringType;
                _entryKey = entryKey;
                DictionaryMember = dictionaryMember;
            }

            public DictionaryTargetMember DictionaryMember { private get; set; }

            public Member GetDictionaryEntryMember()
            {
                var typedTargetMember = (DictionaryTargetMember)DictionaryMember.WithType(_entryDeclaringType);

                return Member.DictionaryEntry(_entryKey, typedTargetMember);
            }

            public override bool Equals(object obj)
            {
                var otherKey = (DictionaryMemberKey)obj;

                // ReSharper disable once PossibleNullReferenceException
                return (otherKey._entryValueType == _entryValueType) &&
                       (otherKey._entryDeclaringType == _entryDeclaringType) &&
                       (otherKey._entryKey == _entryKey);
            }

            #region ExcludeFromCodeCoverage
#if DEBUG
            [ExcludeFromCodeCoverage]
#endif
            #endregion
            public override int GetHashCode() => 0;
        }

        #endregion
    }
}