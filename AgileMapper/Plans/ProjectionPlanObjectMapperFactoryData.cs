namespace AgileObjects.AgileMapper.Plans;

using System.Linq;
using ObjectPopulation;
using static MappingPlanSettings.Default;

internal class ProjectionPlanObjectMapperFactoryData<TSourceElement, TResultElement> :
    PlanObjectMapperFactoryDataBase<TSourceElement, TResultElement>
{
    private readonly IObjectMappingData _mappingData;

    public ProjectionPlanObjectMapperFactoryData(
        IQueryable<TSourceElement> exampleQueryable,
        MapperContext mapperContext)
        : base(
            exampleQueryable,
            mapperContext.RuleSets.Project,
            mapperContext)
    {
        _mappingData = ObjectMappingDataFactory
            .ForProjection<TSourceElement, TResultElement>(exampleQueryable, this);
    }

    public override MappingPlanSettings PlanSettings => LazyPlanned;

    public override IObjectMappingData GetMappingData() => _mappingData;
}