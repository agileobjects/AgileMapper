namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;

    [DebuggerDisplay("{Signature}")]
    internal class DictionarySourceMember : IQualifiedMember
    {
        private readonly IQualifiedMember _wrappedSourceMember;
        private readonly QualifiedMember _targetMember;

        public DictionarySourceMember(IMemberMappingContext context)
        {
            _wrappedSourceMember = context.SourceMember;
            _targetMember = context.TargetMember;
            Type = context.SourceType;
        }

        public Type Type { get; }

        public string Name => _wrappedSourceMember.Name;

        public string Signature => _wrappedSourceMember.Signature;

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