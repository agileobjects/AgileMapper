namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
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
            EntryType = Type.GetGenericArguments()[1];
        }

        public Type Type { get; }

        public Type EntryType { get; }

        public bool IsEnumerable => false;

        public string Name => _wrappedSourceMember.Name;

        public string GetPath() => _wrappedSourceMember.GetPath();

        public IQualifiedMember Append(Member childMember)
        {
            throw new NotImplementedException();
        }

        public IQualifiedMember RelativeTo(IQualifiedMember otherMember)
        {
            throw new NotImplementedException();
        }

        public IQualifiedMember WithType(Type runtimeType)
            => new DictionaryEntrySourceMember(runtimeType, _targetMember, this);

        public bool CouldMatch(QualifiedMember otherMember)
        {
            throw new NotImplementedException();
        }

        public bool Matches(IQualifiedMember otherMember) => _targetMember.Matches(otherMember);

        public Expression GetQualifiedAccess(Expression instance)
        {
            throw new NotImplementedException();
        }
    }
}