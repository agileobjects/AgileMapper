namespace AgileObjects.AgileMapper.Members.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif
    using Caching;
    using Extensions;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
    using TypeConversion;

    internal class DictionaryTargetMember : QualifiedMember
    {
        private readonly DictionaryTargetMember _rootDictionaryMember;
        private bool _createDictionaryChildMembers;

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

        public override bool IsRoot
            => (this == _rootDictionaryMember) ? base.IsRoot : _rootDictionaryMember.IsRoot;

        public override string RegistrationName => GetKeyNameOrNull() ?? base.RegistrationName;

        public Type KeyType { get; }

        public Type ValueType { get; }

        public Expression Key { get; private set; }

        public bool HasKey => Key != null;

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

            return (_rootDictionaryMember.Type != typeof(ExpandoObject)) && type.IsDictionary();
        }

        public DictionaryTargetMember Append(ParameterExpression key)
        {
            var memberKey = new DictionaryMemberKey(ValueType, key.Name, this);
            var childMember = Append(memberKey);

            childMember.Key = key;

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
                },
                default(HashCodeComparer<DictionaryMemberKey>));

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
                Key = Key
            };
        }

        public override bool Matches(IQualifiedMember otherMember)
            => HasKey ? GetKeyNameOrNull() == otherMember.Name : base.Matches(otherMember);

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
        {
            return mapperData.GetValueConversion(
               (Key?.NodeType != Parameter) ? mapperData.GetTargetMemberDictionaryKey() : Key,
                KeyType);
        }

        private Expression GetDictionaryAccess(IMemberMapperData mapperData)
        {
            var parentContextAccess = mapperData
                .GetAppropriateMappingContextAccess(typeof(object), _rootDictionaryMember.Type);

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

        public override bool CheckExistingElementValue
            => !(HasObjectEntries || HasSimpleEntries || HasEnumerableEntries);

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

        public void SetCustomKey(string key) => Key = key.ToConstantExpression();

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

            var keyedAccess = GetKeyedAccess(mapperData);
            var convertedValue = mapperData.GetValueConversionOrCreation(value, ValueType);
            var keyedAssignment = keyedAccess.AssignTo(convertedValue);

            return keyedAssignment;
        }

        private bool ValueIsFlattening(Expression value, out Expression flattening)
        {
            if (HasObjectEntries || HasSimpleEntries)
            {
                return TryGetMappingBody(value, out flattening);
            }

            flattening = null;
            return false;
        }

        private static bool TryGetMappingBody(Expression value, out Expression mapping)
        {
            if (value.NodeType == Try)
            {
                value = ((TryExpression)value).Body;
            }

            if ((value.NodeType != Block))
            {
                mapping = null;
                return false;
            }

            var mappingBlock = (BlockExpression)value;

            if (mappingBlock.Expressions.HasOne())
            {
                mapping = null;
                return false;
            }

            var mappingExpressions = GetMappingExpressions(mappingBlock);

            if (mappingExpressions.HasOne() &&
               (mappingExpressions[0].NodeType == Block))
            {
                IList<ParameterExpression> mappingVariables = mappingBlock.Variables;
                mappingBlock = (BlockExpression)mappingExpressions[0];
                mappingVariables = mappingVariables.Append(mappingBlock.Variables);
                mapping = mappingBlock.Update(mappingVariables, mappingBlock.Expressions);
                return true;
            }

            mapping = mappingBlock.Variables.Any()
                ? Expression.Block(mappingBlock.Variables, mappingExpressions)
                : mappingExpressions.ToExpression();

            return true;
        }

        private static IList<Expression> GetMappingExpressions(Expression mapping)
        {
            var expressions = new List<Expression>();

            while (mapping.NodeType == Block)
            {
                var mappingBlock = (BlockExpression)mapping;

                expressions.AddRange(mappingBlock.Expressions.Take(mappingBlock.Expressions.Count - 1));
                mapping = mappingBlock.Result;
            }

            return expressions;
        }

        public DictionaryTargetMember WithTypeOf(Member sourceMember)
        {
            if (sourceMember.Type == Type)
            {
                return this;
            }

            return (DictionaryTargetMember)WithType(sourceMember.Type);
        }

        public override void MapCreating(Type sourceType)
        {
            if (DoNotFlattenSourceObjects(sourceType))
            {
                _createDictionaryChildMembers = false;
            }

            base.MapCreating(sourceType);
        }

        private bool DoNotFlattenSourceObjects(Type sourceType)
        {
            // If this target Dictionary member's type matches the type of source
            // objects being mapped into it, we switch from flattening source
            // objects into Dictionary entries to mapping entire objects:
            return this.IsEnumerableElement() &&
                  (ValueType == sourceType) &&
                  (MemberChain[Depth - 2] == _rootDictionaryMember.LeafMember);
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

        private string GetKeyNameOrNull() => (string)((ConstantExpression)Key)?.Value;

        #region Helper Classes

        private class DictionaryMemberKey
        {
            private readonly Type _entryDeclaringType;
            private readonly string _entryKey;
            private readonly int _hashCode;

            public DictionaryMemberKey(
                Type entryDeclaringType,
                string entryKey,
                DictionaryTargetMember dictionaryMember)
            {
                _entryDeclaringType = entryDeclaringType;
                _entryKey = entryKey;
                DictionaryMember = dictionaryMember;

                _hashCode = dictionaryMember.ValueType.GetHashCode();

                unchecked
                {
                    _hashCode = (_hashCode * 397) ^ _entryDeclaringType.GetHashCode();
                    _hashCode = (_hashCode * 397) ^ _entryKey.GetHashCode();
                }
            }

            public DictionaryTargetMember DictionaryMember { private get; set; }

            public Member GetDictionaryEntryMember()
            {
                var typedTargetMember = (DictionaryTargetMember)DictionaryMember.WithType(_entryDeclaringType);

                return Member.DictionaryEntry(_entryKey, typedTargetMember);
            }

            public override int GetHashCode() => _hashCode;
        }

        #endregion
    }
}