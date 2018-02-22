namespace AgileObjects.AgileMapper.Members.Sources
{
    using ObjectPopulation;

    internal class MemberLookupsChildMembersSource : IChildMembersSource
    {
        private readonly ObjectMapperData _parentMapperData;

        public MemberLookupsChildMembersSource(
            ObjectMapperData parentMapperData,
            string targetMemberRegistrationName,
            int dataSourceIndex)
        {
            _parentMapperData = parentMapperData;
            TargetMemberRegistrationName = targetMemberRegistrationName;
            DataSourceIndex = dataSourceIndex;
        }

        public string TargetMemberRegistrationName { get; }

        public int DataSourceIndex { get; }

        public IQualifiedMember GetSourceMember<TSource, TTarget>()
            => _parentMapperData.GetSourceMemberFor(TargetMemberRegistrationName, DataSourceIndex);

        public QualifiedMember GetTargetMember<TSource, TTarget>()
            => _parentMapperData.GetTargetMemberFor(TargetMemberRegistrationName);
    }
}