namespace AgileObjects.AgileMapper.Plans
{
    using ObjectPopulation;
    using ObjectPopulation.MapperKeys;

    internal abstract class PlanObjectMapperFactoryDataBase<TSourceElement, TResultElement> :
        IObjectMapperFactoryData,
        IMapperKeyData
    {
        protected PlanObjectMapperFactoryDataBase(
            object source,
            MappingRuleSet ruleSet,
            MapperContext mapperContext)
        {
            Source = source;
            MapperContext = mapperContext;
            RuleSet = ruleSet;
        }

        public MapperContext MapperContext { get; }

        public MappingRuleSet RuleSet { get; }

        public MappingTypes MappingTypes
            => MappingTypes<TSourceElement, TResultElement>.Fixed;

        public abstract MappingPlanSettings PlanSettings { get; }

        public object Source { get; }

        public ObjectMapperKeyBase GetMapperKey()
            => RuleSet.RootMapperKeyFactory.Invoke(this);

        public abstract IObjectMappingData GetMappingData();
    }
}