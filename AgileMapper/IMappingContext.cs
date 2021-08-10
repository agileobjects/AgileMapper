namespace AgileObjects.AgileMapper
{
    using Plans;

    internal interface IMappingContext : IMapperContextOwner, IRuleSetOwner
    {
        MappingPlanSettings PlanSettings { get; }
    }
}