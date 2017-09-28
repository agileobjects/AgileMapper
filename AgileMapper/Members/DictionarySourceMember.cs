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

    internal class DictionarySourceMember : IQualifiedMember
    {
        private readonly IQualifiedMember _wrappedSourceMember;
        private readonly QualifiedMember _matchedTargetMember;

        public DictionarySourceMember(IMemberMapperData mapperData)
            : this(mapperData.SourceMember, mapperData.TargetMember)
        {
        }

        public DictionarySourceMember(IQualifiedMember wrappedSourceMember, QualifiedMember matchedTargetMember)
            : this(
                wrappedSourceMember.Type,
                wrappedSourceMember,
                matchedTargetMember,
                wrappedSourceMember.Matches(matchedTargetMember))
        {
        }

        private DictionarySourceMember(
            Type dictionaryType,
            IQualifiedMember wrappedSourceMember,
            QualifiedMember matchedTargetMember,
            bool isEntireDictionaryMatch,
            Type keyType = null,
            Type valueType = null)
        {
            _wrappedSourceMember = wrappedSourceMember;
            _matchedTargetMember = matchedTargetMember;
            IsEntireDictionaryMatch = isEntireDictionaryMatch;

            Type = dictionaryType;

            if (keyType == null)
            {
                var dictionaryTypes = Type.GetGenericArguments();
                KeyType = dictionaryTypes[0];
                ValueType = dictionaryTypes[1];
            }
            else
            {
                KeyType = keyType;
                ValueType = valueType;
            }

            EntryMember = new DictionaryEntrySourceMember(ValueType, matchedTargetMember, this);
            HasObjectEntries = ValueType == typeof(object);

            CouldContainSourceInstance =
                HasObjectEntries || (matchedTargetMember.IsEnumerable == ValueType.IsEnumerable());
        }

        public Type Type { get; }

        public Type KeyType { get; }

        public Type ValueType { get; }

        public DictionaryEntrySourceMember EntryMember { get; }

        public bool HasObjectEntries { get; }

        public bool IsEnumerable => true;

        public bool CouldContainSourceInstance { get; }

        public bool IsEntireDictionaryMatch { get; }

        public string Name => _wrappedSourceMember.Name;

        public string GetPath() => _wrappedSourceMember.GetPath();

        public IQualifiedMember GetElementMember()
        {
            if (EntryMember.IsEnumerable)
            {
                return EntryMember.GetElementMember();
            }

            return EntryMember.GetInstanceElementMember();
        }

        public IQualifiedMember Append(Member childMember) => EntryMember.Append(childMember);

        public IQualifiedMember RelativeTo(IQualifiedMember otherMember)
        {
            return new DictionarySourceMember(
                Type,
                _wrappedSourceMember.RelativeTo(otherMember),
                _matchedTargetMember,
                IsEntireDictionaryMatch,
                KeyType,
                ValueType);
        }

        public IQualifiedMember WithType(Type runtimeType) => this;

        public bool CouldMatch(QualifiedMember otherMember) => _wrappedSourceMember.CouldMatch(otherMember);

        public bool Matches(IQualifiedMember otherMember) => _wrappedSourceMember.Matches(otherMember);

        public Expression GetQualifiedAccess(IMemberMapperData mapperData)
        {
            return IsEntireDictionaryMatch
                ? _wrappedSourceMember.GetQualifiedAccess(mapperData)
                : EntryMember.GetQualifiedAccess(mapperData);
        }

        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString() => _wrappedSourceMember.ToString();
    }
}