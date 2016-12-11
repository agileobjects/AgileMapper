namespace AgileObjects.AgileMapper.Members.Sources
{
    using ObjectPopulation;

    internal class MemberLookupsChildMembersSource : IChildMembersSource
    {
        private readonly IObjectMappingData _parent;

        public MemberLookupsChildMembersSource(
            IObjectMappingData parent,
            string targetMemberRegistrationName,
            int dataSourceIndex)
        {
            _parent = parent;
            TargetMemberRegistrationName = targetMemberRegistrationName;
            DataSourceIndex = dataSourceIndex;
        }

        public string TargetMemberRegistrationName { get; }

        public int DataSourceIndex { get; }

        public IQualifiedMember GetSourceMember<TSource, TTarget>()
            => _parent.MapperData.GetSourceMemberFor(TargetMemberRegistrationName, DataSourceIndex);

        public QualifiedMember GetTargetMember<TTarget>()
            => _parent.MapperData.GetTargetMemberFor(TargetMemberRegistrationName);
    }
}