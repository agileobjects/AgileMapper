namespace AgileObjects.AgileMapper;
using ObjectPopulation;
using Plans;

internal abstract class SubObjectMappingExecutionContextBase :
    MappingExecutionContextBase2
{
    private readonly MappingExecutionContextBase2 _parent;

    protected SubObjectMappingExecutionContextBase(
        object source,
        object target,
        int? elementIndex,
        object elementKey,
        MappingExecutionContextBase2 parent)
        : base(source, elementIndex, elementKey, parent)
    {
        _parent = parent;
        Target = target;
    }

    public override MapperContext MapperContext => _parent.MapperContext;

    public override MappingRuleSet RuleSet => _parent.RuleSet;

    public override MappingPlanSettings PlanSettings => _parent.PlanSettings;

    public override MappingTypes MappingTypes => GetMapperKey().MappingTypes;

    public override IObjectMapper GetRootMapper() => _parent.GetRootMapper();

    public override object Target { get; }

    protected override void Set(object target)
    {
        // Only ever called on root objects
    }

    protected IObjectMappingData GetParentMappingData() => _parent.GetMappingData();
}