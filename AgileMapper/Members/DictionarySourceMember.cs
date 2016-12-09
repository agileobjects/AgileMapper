namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using Extensions;

#if NET_STANDARD
    using System.Reflection;
#endif

    [DebuggerDisplay("{GetPath()}")]
    internal class DictionarySourceMember : IQualifiedMember
    {
        private readonly IQualifiedMember _wrappedSourceMember;
        private readonly QualifiedMember _targetMember;

        public DictionarySourceMember(IMemberMapperData mapperData)
        {
            _wrappedSourceMember = mapperData.SourceMember;
            _targetMember = mapperData.TargetMember;
            Type = mapperData.SourceType;
            EntryMember = new DictionaryEntrySourceMember(Type.GetGenericArguments()[1], _targetMember, this);

            CouldContainSourceInstance =
                HasObjectEntries ||
                (mapperData.TargetMember.IsEnumerable == EntryType.IsEnumerable());
        }

        public Type Type { get; }

        public Type EntryType => EntryMember.Type;

        public DictionaryEntrySourceMember EntryMember { get; }

        public bool HasObjectEntries => EntryType == typeof(object);

        public bool IsEnumerable => true;

        public bool CouldContainSourceInstance { get; }

        public string Name => _wrappedSourceMember.Name;

        public string GetPath() => _wrappedSourceMember.GetPath();

        public IQualifiedMember GetElementMember()
            => HasObjectEntries ? EntryMember.GetObjectElementMember() : EntryMember.GetElementMember();

        public IQualifiedMember Append(Member childMember) => EntryMember.Append(childMember);

        public IQualifiedMember RelativeTo(IQualifiedMember otherMember)
        {
            throw new NotImplementedException();
        }

        public IQualifiedMember WithType(Type runtimeType) => this;

        public bool CouldMatch(QualifiedMember otherMember)
        {
            throw new NotImplementedException();
        }

        public bool Matches(IQualifiedMember otherMember) => _targetMember.Matches(otherMember);

        public Expression GetQualifiedAccess(Expression instance)
            => _wrappedSourceMember.GetQualifiedAccess(instance);
    }
}