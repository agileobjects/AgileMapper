namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;

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
        }

        public Type Type { get; }

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

        public bool Matches(QualifiedMember otherMember) => _targetMember.Matches(otherMember);

        public Expression GetQualifiedAccess(Expression instance)
        {
            throw new NotImplementedException();
        }
    }
}