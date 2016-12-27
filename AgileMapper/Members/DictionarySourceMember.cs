namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Extensions;

    [DebuggerDisplay("{GetPath()}")]
    internal class DictionarySourceMember : IQualifiedMember
    {
        private readonly IQualifiedMember _wrappedSourceMember;
        private readonly bool _isEntireDictionaryMatch;

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
            _isEntireDictionaryMatch = wrappedSourceMember.Matches(matchedTargetMember);
            Type = sourceType;
            EntryMember = new DictionaryEntrySourceMember(Type.GetGenericArguments()[1], matchedTargetMember, this);
            HasObjectEntries = EntryType == typeof(object);

            CouldContainSourceInstance =
                HasObjectEntries || (matchedTargetMember.IsEnumerable == EntryType.IsEnumerable());
        }

        public Type Type { get; }

        public Type EntryType => EntryMember.Type;

        public DictionaryEntrySourceMember EntryMember { get; }

        public bool HasObjectEntries { get; }

        public bool IsEnumerable => true;

        public bool CouldContainSourceInstance { get; }

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
            return _isEntireDictionaryMatch
                ? _wrappedSourceMember.GetQualifiedAccess(instance)
                : EntryMember.GetQualifiedAccess(instance);
        }
    }
}