namespace AgileObjects.AgileMapper.Members
{
    using System;
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
        private Expression _key;

        public DictionaryTargetMember(QualifiedMember wrappedTargetMember)
            : base(wrappedTargetMember.MemberChain, wrappedTargetMember)
        {
            var dictionaryTypes = wrappedTargetMember.Type.GetGenericArguments();
            KeyType = dictionaryTypes[0];
            ValueType = dictionaryTypes[1];
            _rootDictionaryMember = this;
        }

        private DictionaryTargetMember(
            QualifiedMember matchedTargetMember,
            DictionaryTargetMember rootDictionaryMember)
            : base(matchedTargetMember.MemberChain, matchedTargetMember)
        {
            KeyType = rootDictionaryMember.KeyType;
            ValueType = rootDictionaryMember.ValueType;
            _rootDictionaryMember = rootDictionaryMember;
        }

        public Type KeyType { get; }

        public Type ValueType { get; }

        public bool HasObjectEntries => ValueType == typeof(object);

        public override bool GuardObjectValuePopulations => true;

        public DictionaryTargetMember Append(ParameterExpression key)
        {
            var childMember = Append(key.Name);

            childMember._key = key;

            return childMember;
        }

        public DictionaryTargetMember Append(string sourceMemberName)
        {
            var targetEntryMember = Member.DictionaryEntry(sourceMemberName, this);
            var childMember = Append(targetEntryMember);

            return (DictionaryTargetMember)childMember;
        }

        protected override QualifiedMember CreateChildMember(Member childMember)
        {
            var matchedTargetEntryMember = base.CreateChildMember(childMember);

            if (CreateDictionaryChildMember(matchedTargetEntryMember))
            {
                return new DictionaryTargetMember(matchedTargetEntryMember, _rootDictionaryMember);
            }

            return matchedTargetEntryMember;
        }

        private bool CreateDictionaryChildMember(QualifiedMember entryMember)
        {
            if (_rootDictionaryMember.HasObjectEntries)
            {
                return true;
            }

            if (entryMember.MemberChain.Length <= 2)
            {
                return true;
            }

            for (var i = entryMember.MemberChain.Length - 1; i > 0; --i)
            {
                var member = entryMember.MemberChain[i];

                if (member.IsSimple)
                {
                    continue;
                }

                if (member.IsDictionary)
                {
                    return true;
                }

                if (member.IsComplex && (member.Type != typeof(object)))
                {
                    return false;
                }
            }

            return true;
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

            var index = _key ?? mapperData.GetTargetMemberDictionaryKey();
            var dictionaryMapperData = FindDictionaryMapperData(mapperData);
            var indexAccess = dictionaryMapperData.InstanceVariable.GetIndexAccess(index);

            return indexAccess;
        }

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
            var tryGetValueMethod = mapperData.InstanceVariable.Type.GetMethod("TryGetValue");
            var index = _key ?? mapperData.GetTargetMemberDictionaryKey();

            var tryGetValueCall = Expression.Call(
                mapperData.InstanceVariable,
                tryGetValueMethod,
                index,
                existingValueVariable);

            var existingValueIsDefault = existingValueVariable.GetIsDefaultComparison();

            var valueMissingOrDefault = Expression.OrElse(Expression.Not(tryGetValueCall), existingValueIsDefault);

            return Expression.Block(new[] { existingValueVariable }, valueMissingOrDefault);
        }

        public override Expression GetPopulation(Expression value, IMemberMapperData mapperData)
        {
            var indexAccess = GetAccess(mapperData.InstanceVariable, mapperData);
            var indexAssignment = indexAccess.AssignTo(value);

            return indexAssignment;
        }

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