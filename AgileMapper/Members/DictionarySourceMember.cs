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

        public DictionarySourceMember(IMemberMapperData mapperData)
            : this(mapperData.SourceMember, mapperData.TargetMember)
        {
        }

        public DictionarySourceMember(IQualifiedMember wrappedSourceMember, QualifiedMember targetMember)
            : this(wrappedSourceMember, wrappedSourceMember.Type, targetMember)
        {
        }

        private DictionarySourceMember(
            IQualifiedMember wrappedSourceMember,
            Type sourceType,
            QualifiedMember matchedTargetMember)
        {
            _wrappedSourceMember = wrappedSourceMember;
            IsEntireDictionaryMatch = wrappedSourceMember.Matches(matchedTargetMember);
            Type = sourceType;
            var dictionaryTypes = Type.GetGenericArguments();
            KeyType = dictionaryTypes[0];
            EntryMember = new DictionaryEntrySourceMember(dictionaryTypes[1], matchedTargetMember, this);
            HasObjectEntries = EntryType == typeof(object);

            CouldContainSourceInstance =
                HasObjectEntries || (matchedTargetMember.IsEnumerable == EntryType.IsEnumerable());
        }

        public Type Type { get; }

        public Type KeyType { get; }

        public Type EntryType => EntryMember.Type;

        public DictionaryEntrySourceMember EntryMember { get; }

        public bool HasObjectEntries { get; }

        public bool IsEnumerable => true;

        public bool CouldContainSourceInstance { get; }

        public bool IsEntireDictionaryMatch { get; }

        public string Name => _wrappedSourceMember.Name;

        public string GetPath() => _wrappedSourceMember.GetPath();

        public IQualifiedMember GetElementMember()
            => HasObjectEntries ? EntryMember.GetObjectElementMember() : EntryMember.GetElementMember();

        public IQualifiedMember Append(Member childMember) => EntryMember.Append(childMember);

        public IQualifiedMember RelativeTo(IQualifiedMember otherMember) => this;

        public IQualifiedMember WithType(Type runtimeType) => this;

        public bool CouldMatch(QualifiedMember otherMember) => _wrappedSourceMember.CouldMatch(otherMember);

        public bool Matches(IQualifiedMember otherMember) => _wrappedSourceMember.Matches(otherMember);

        public Expression GetQualifiedAccess(Expression instance)
        {
            return IsEntireDictionaryMatch
                ? _wrappedSourceMember.GetQualifiedAccess(instance)
                : EntryMember.GetQualifiedAccess(instance);
        }

        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString() => _wrappedSourceMember.ToString();
    }
}