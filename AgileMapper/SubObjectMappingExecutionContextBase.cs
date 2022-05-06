namespace AgileObjects.AgileMapper;

using Members;
using NetStandardPolyfills;
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

    public override IMappingData<TSource, TTarget> WithTypes<TSource, TTarget>()
    {
        var mappingTypes = MappingTypes;
        var sourceType = mappingTypes.SourceType;
        var targetType = mappingTypes.TargetType;

        var typesMatch =
            sourceType.IsAssignableTo(typeof(TSource)) &&
            targetType.IsAssignableTo(typeof(TTarget));

        return typesMatch
            ? this.ToTyped<TSource, TTarget>()
            : _parent.WithTypes<TSource, TTarget>();
    }
}