namespace AgileObjects.AgileMapper.Members.Dictionaries
{
    using System;
    using Extensions.Internal;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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
                wrappedSourceMember.Type.GetDictionaryType(),
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
                var dictionaryTypes = Type.GetDictionaryTypes();
                KeyType = dictionaryTypes.Key;
                ValueType = dictionaryTypes.Value;
            }
            else
            {
                KeyType = keyType;
                ValueType = valueType;
            }

            EntryMember = (wrappedSourceMember as DictionaryEntrySourceMember) ??
                           new DictionaryEntrySourceMember(ValueType, matchedTargetMember, this);

            HasObjectEntries = ValueType == typeof(object);

            CouldContainSourceInstance =
                HasObjectEntries || (matchedTargetMember.IsEnumerable == ValueType.IsEnumerable());
        }

        public bool IsRoot => _wrappedSourceMember.IsRoot;

        public Type Type { get; }

        public Type ElementType => ValueType;

        public string GetFriendlyTypeName() => _wrappedSourceMember.GetFriendlyTypeName();

        public Type KeyType { get; }

        public Type ValueType { get; }

        public DictionaryEntrySourceMember EntryMember { get; }

        public bool HasObjectEntries { get; }

        public bool IsEnumerable => true;

        public bool IsSimple => false;

        public bool CouldContainSourceInstance { get; }

        public bool IsEntireDictionaryMatch { get; }

        public string Name => _wrappedSourceMember.Name;

        public string GetPath() => _wrappedSourceMember.GetPath();

        public IQualifiedMember GetElementMember()
        {
            return EntryMember.IsEnumerable
                ? EntryMember.GetElementMember()
                : EntryMember.GetInstanceElementMember();
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

        public IQualifiedMember WithType(Type runtimeType)
            => (runtimeType != _wrappedSourceMember.Type) ? EntryMember.WithType(runtimeType) : this;

        public bool HasCompatibleType(Type type) => _wrappedSourceMember.HasCompatibleType(type);

        public bool CouldMatch(QualifiedMember otherMember) => _wrappedSourceMember.CouldMatch(otherMember);

        public bool Matches(IQualifiedMember otherMember) => _wrappedSourceMember.Matches(otherMember);

        public Expression GetQualifiedAccess(Expression parentInstance)
        {
            return IsEntireDictionaryMatch
                ? _wrappedSourceMember.GetQualifiedAccess(parentInstance)
                : EntryMember.GetQualifiedAccess(parentInstance);
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString() => _wrappedSourceMember.ToString();
    }
}