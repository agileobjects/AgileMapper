namespace AgileObjects.AgileMapper.Members
{
    using System;
#if NET_STANDARD
    using System.Reflection;
#endif

    internal class DictionaryTargetMember : QualifiedMember
    {
        public DictionaryTargetMember(QualifiedMember wrappedTargetMember)
            : base((Member[])wrappedTargetMember.MemberChain, wrappedTargetMember)
        {
            var dictionaryTypes = wrappedTargetMember.Type.GetGenericArguments();
            KeyType = dictionaryTypes[0];
            ValueType = dictionaryTypes[1];
        }

        public Type KeyType { get; }

        public Type ValueType { get; }
    }
}