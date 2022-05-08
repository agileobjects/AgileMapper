namespace AgileObjects.AgileMapper.Plans;

using ObjectPopulation;
using static MappingPlanSettings.Default;

internal class PlanObjectMapperFactoryData<TSource, TTarget> :
    PlanObjectMapperFactoryDataBase<TSource, TTarget>
{
    private readonly IObjectMappingData _mappingData;

    public PlanObjectMapperFactoryData(
        MappingRuleSet ruleSet,
        MapperContext mapperContext)
        : base(default(TSource), ruleSet, mapperContext)
    {
        _mappingData = ObjectMappingDataFactory
            .ForRootFixedTypes<TSource, TTarget>(this);
    }

    public override MappingPlanSettings PlanSettings => EagerPlanned;

    public override IObjectMappingData GetMappingData() => _mappingData;
}