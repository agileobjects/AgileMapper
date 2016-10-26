namespace AgileObjects.AgileMapper.Members.Sources
{
    using ObjectPopulation;

    internal class ElementMembersSource : IMembersSource
    {
        public ElementMembersSource(IObjectMappingData parent)
        {
            Parent = parent;
        }

        public IObjectMappingData Parent { get; }

        public IQualifiedMember GetSourceMember<TSource>() => Parent.MapperData.SourceMember.GetElementMember();

        public QualifiedMember GetTargetMember<TTarget>() => Parent.MapperData.TargetMember.GetElementMember();
    }
}