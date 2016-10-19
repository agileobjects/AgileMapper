namespace AgileObjects.AgileMapper.Members.Sources
{
    internal class TargetMemberChildMembersSource : IChildMembersSource
    {
        private readonly IQualifiedMember _sourceMember;
        private readonly QualifiedMember _targetMember;

        public TargetMemberChildMembersSource(
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            int dataSourceIndex)
        {
            _sourceMember = sourceMember;
            _targetMember = targetMember;
            DataSourceIndex = dataSourceIndex;
        }

        public string TargetMemberRegistrationName => _targetMember.RegistrationName;

        public int DataSourceIndex { get; }

        public IQualifiedMember GetSourceMember<TSource>() => _sourceMember;

        public QualifiedMember GetTargetMember<TTarget>() => _targetMember;
    }
}