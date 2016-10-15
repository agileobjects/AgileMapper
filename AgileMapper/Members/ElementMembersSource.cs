namespace AgileObjects.AgileMapper.Members
{
    using ObjectPopulation;

    internal class ElementMembersSource : IMembersSource
    {
        public ElementMembersSource(IObjectMappingData parent)
        {
            Parent = parent;
        }

        public IObjectMappingData Parent { get; }

        public IQualifiedMember GetSourceMember<TSource>() => Parent.MapperData.SourceElementMember;

        public QualifiedMember GetTargetMember<TTarget>() => Parent.MapperData.TargetElementMember;
    }
}