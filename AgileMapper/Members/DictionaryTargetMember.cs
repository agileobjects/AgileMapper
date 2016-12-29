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

        public DictionaryTargetMember Append(string sourceMemberName)
        {
            var targetEntryMember = Member.DictionaryEntry(sourceMemberName, this);
            var childMember = Append(targetEntryMember);

            return (DictionaryTargetMember)childMember;
        }

        protected override QualifiedMember CreateChildMember(Member childMember)
        {
            var matchedTargetEntryMember = base.CreateChildMember(childMember);

            return new DictionaryTargetMember(matchedTargetEntryMember, _rootDictionaryMember);
        }

        public override Expression GetAccess(Expression instance, IMemberMapperData mapperData)
        {
            var index = mapperData.GetTargetMemberDictionaryKey();
            var indexAccess = mapperData.InstanceVariable.GetIndexAccess(index);

            return indexAccess;
        }

        public override Expression GetPopulation(Expression value, IMemberMapperData mapperData)
        {
            if (mapperData.InstanceVariable.Type != _rootDictionaryMember.Type)
            {
                return base.GetPopulation(value, mapperData);
            }

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