namespace AgileObjects.AgileMapper.Members.Sources
{
    using ObjectPopulation;

    internal class TargetMemberNameChildMembersSource : IChildMembersSource
    {
        private readonly IObjectMappingData _parent;

        public TargetMemberNameChildMembersSource(
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

        public IQualifiedMember GetSourceMember<TSource>()
            => _parent.MapperData.GetSourceMemberFor(TargetMemberRegistrationName, DataSourceIndex);

        public QualifiedMember GetTargetMember<TTarget>()
            => _parent.MapperData.GetTargetMemberFor(TargetMemberRegistrationName);
    }
}