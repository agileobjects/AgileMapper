namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
#if !NET_STANDARD
    using System.Diagnostics.CodeAnalysis;
#endif
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Extensions;
    using ReadableExpressions.Extensions;

    internal class DictionaryTargetMember : QualifiedMember
    {
        private readonly DictionaryTargetMember _rootDictionaryMember;
        private readonly bool _createDictionaryChildMembers;
        private Expression _key;

        public DictionaryTargetMember(QualifiedMember wrappedTargetMember)
            : base(wrappedTargetMember.MemberChain, wrappedTargetMember)
        {
            var dictionaryTypes = wrappedTargetMember.Type.GetGenericArguments();
            KeyType = dictionaryTypes[0];
            ValueType = dictionaryTypes[1];
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
            _createDictionaryChildMembers = HasObjectEntries || ValueType.IsSimple();
        }

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

        public DictionaryTargetMember Append(ParameterExpression key)
        {
            var childMember = Append(key.Name);

            childMember._key = key;

            return childMember;
        }

        public DictionaryTargetMember Append(string entryKey)
        {
            var targetEntryMember = Member.DictionaryEntry(entryKey, this);
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

            return new DictionaryTargetMember(runtimeTypedTargetEntryMember, _rootDictionaryMember);
        }

        public override Expression GetAccess(Expression instance, IMemberMapperData mapperData)
        {
            if (this == _rootDictionaryMember)
            {
                return base.GetAccess(instance, mapperData);
            }

            if (ReturnNullAccess())
            {
                return Type.ToDefaultExpression();
            }

            return GetAccess(mapperData);
        }

        private bool ReturnNullAccess()
        {
            if (Type == ValueType)
            {
                return false;
            }

            if (Type.IsSimple())
            {
                return false;
            }

            return true;
        }

        public Expression GetAccess(IMemberMapperData mapperData)
        {
            var index = GetKey(mapperData);
            var dictionaryMapperData = FindDictionaryMapperData(mapperData);
            var indexAccess = dictionaryMapperData.InstanceVariable.GetIndexAccess(index);

            return indexAccess;
        }

        private Expression GetKey(IMemberMapperData mapperData)
            => _key ?? (_key = mapperData.GetValueConversion(mapperData.GetTargetMemberDictionaryKey(), KeyType));

        private IMemberMapperData FindDictionaryMapperData(IMemberMapperData mapperData)
        {
            var dictionaryMapperData = mapperData;

            while (dictionaryMapperData.TargetMember != _rootDictionaryMember)
            {
                dictionaryMapperData = dictionaryMapperData.Parent;
            }

            return dictionaryMapperData;
        }

        public override Expression GetHasDefaultValueCheck(IMemberMapperData mapperData)
        {
            var existingValueVariable = Expression.Variable(ValueType, "existingValue");

            var tryGetValueCall = GetTryGetValueCall(existingValueVariable, mapperData);
            var existingValueIsDefault = existingValueVariable.GetIsDefaultComparison();

            var valueMissingOrDefault = Expression.OrElse(Expression.Not(tryGetValueCall), existingValueIsDefault);

            return Expression.Block(new[] { existingValueVariable }, valueMissingOrDefault);
        }

        public Expression GetTryGetValueCall(Expression valueVariable, IMemberMapperData mapperData)
        {
            var dictionaryMapperData = FindDictionaryMapperData(mapperData);
            var tryGetValueMethod = dictionaryMapperData.InstanceVariable.Type.GetMethod("TryGetValue");
            var index = GetKey(mapperData);

            var tryGetValueCall = Expression.Call(
                dictionaryMapperData.InstanceVariable,
                tryGetValueMethod,
                index,
                valueVariable);

            return tryGetValueCall;
        }

        public override Expression GetPopulation(Expression value, IMemberMapperData mapperData)
        {
            if (this == _rootDictionaryMember)
            {
                return base.GetPopulation(value, mapperData);
            }

            BlockExpression flattening;

            if (ValueIsFlattening(value, out flattening))
            {
                return flattening;
            }

            var indexAccess = GetAccess(mapperData.InstanceVariable, mapperData);
            var convertedValue = mapperData.GetValueConversion(value, ValueType);
            var indexAssignment = indexAccess.AssignTo(convertedValue);

            return indexAssignment;
        }

        private bool ValueIsFlattening(Expression value, out BlockExpression flattening)
        {
            if (!(HasObjectEntries || HasSimpleEntries))
            {
                flattening = null;
                return false;
            }

            ICollection<ParameterExpression> blockParameters;

            if (value.NodeType == ExpressionType.Block)
            {
                flattening = (BlockExpression)value;
                blockParameters = flattening.Variables;
                value = flattening.Expressions[0];
            }
            else
            {
                blockParameters = Enumerable<ParameterExpression>.EmptyArray;
            }

            if (value.NodeType != ExpressionType.Try)
            {
                flattening = null;
                return false;
            }

            flattening = (BlockExpression)((TryExpression)value).Body;
            var flatteningExpressions = GetMappingExpressions(flattening);

            flattening = blockParameters.Any()
                ? Expression.Block(blockParameters, flatteningExpressions)
                : flatteningExpressions.HasOne()
                    ? (BlockExpression)flatteningExpressions[0]
                    : Expression.Block(flatteningExpressions);

            return true;
        }

        private static IList<Expression> GetMappingExpressions(Expression mapping)
        {
            var expressions = new List<Expression>();

            while (mapping.NodeType == ExpressionType.Block)
            {
                var mappingBlock = (BlockExpression)mapping;
                expressions.AddRange(mappingBlock.Expressions);
                expressions.Remove(mappingBlock.Result);

                mapping = mappingBlock.Result;
            }

            return expressions;
        }

        public DictionaryTargetMember WithTypeOf(Member sourceMember)
            => (DictionaryTargetMember)WithType(sourceMember.Type);

        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString()
        {
            if (LeafMember.IsRoot)
            {
                return base.ToString();
            }

            var path = GetPath().Substring("Target.".Length);

            return "[\"" + path + "\"]: " + Type.GetFriendlyName();
        }
    }
}