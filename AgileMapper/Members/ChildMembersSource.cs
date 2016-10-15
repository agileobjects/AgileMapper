namespace AgileObjects.AgileMapper.Members
{
    using ObjectPopulation;

    internal class ChildMembersSource : IMembersSource
    {
        private readonly IObjectMappingData _parent;

        public ChildMembersSource(
            IObjectMappingData parent,
            string targetMemberName,
            int dataSourceIndex)
        {
            _parent = parent;
            TargetMemberName = targetMemberName;
            DataSourceIndex = dataSourceIndex;
        }

        public string TargetMemberName { get; }

        public int DataSourceIndex { get; }

        public IQualifiedMember GetSourceMember<TSource>()
            => _parent.MapperData.GetSourceMemberFor(TargetMemberName, DataSourceIndex);

        public QualifiedMember GetTargetMember<TTarget>()
            => _parent.MapperData.GetTargetMemberFor(TargetMemberName);
    }
}