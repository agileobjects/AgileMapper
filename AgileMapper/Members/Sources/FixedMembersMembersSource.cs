namespace AgileObjects.AgileMapper.Members.Sources
{
    internal class FixedMembersMembersSource : IChildMembersSource
    {
        private readonly IQualifiedMember _sourceMember;
        private readonly QualifiedMember _targetMember;

        public FixedMembersMembersSource(
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            int dataSourceIndex = 0)
        {
            _sourceMember = sourceMember;
            _targetMember = targetMember;
            DataSourceIndex = dataSourceIndex;
        }

        public string TargetMemberRegistrationName => _targetMember.RegistrationName;

        public int DataSourceIndex { get; }

        public IQualifiedMember GetSourceMember<TSource, TTarget>() => _sourceMember;

        public QualifiedMember GetTargetMember<TSource, TTarget>() => _targetMember;
    }
}