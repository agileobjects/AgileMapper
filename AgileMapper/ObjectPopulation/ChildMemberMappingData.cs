namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal class ChildMemberMappingData<TSource, TTarget> : IChildMemberMappingData
    {
        private readonly ObjectMappingData<TSource, TTarget> _parent;

        public ChildMemberMappingData(ObjectMappingData<TSource, TTarget> parent, IMemberMapperData mapperData)
        {
            _parent = parent;
            MapperData = mapperData;
        }

        public MappingRuleSet RuleSet => _parent.MappingContext.RuleSet;

        public IObjectMappingData Parent => _parent;

        public IMemberMapperData MapperData { get; }
    }
}