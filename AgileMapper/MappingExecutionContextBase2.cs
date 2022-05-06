namespace AgileObjects.AgileMapper;

using System.Collections.Generic;
using System.Linq;
using Members;
using ObjectPopulation;
using ObjectPopulation.MapperKeys;
using Plans;

internal abstract class MappingExecutionContextBase2 :
    IMappingExecutionContext,
    IMapperKeyData,
    IObjectMapperFactoryData
{
    private readonly IMappingExecutionContext _parent;
    private Dictionary<object, List<object>> _mappedObjectsBySource;

    protected MappingExecutionContextBase2(
        object source,
        int? elementIndex,
        object elementKey,
        IMappingExecutionContext parent)
        : this(source, parent)
    {
        ElementIndex = elementIndex;
        ElementKey = elementKey;
    }

    protected MappingExecutionContextBase2(
        object source,
        IMappingExecutionContext parent)
    {
        _parent = parent;
        Source = source;
    }

    public abstract MapperContext MapperContext { get; }

    public abstract MappingRuleSet RuleSet { get; }

    public abstract MappingPlanSettings PlanSettings { get; }

    public abstract MappingTypes MappingTypes { get; }

    public abstract ObjectMapperKeyBase GetMapperKey();

    public abstract IObjectMappingData GetMappingData();

    public abstract IObjectMapper GetRootMapper();

    public object Source { get; }

    public abstract object Target { get; }

    public int? ElementIndex { get; }

    public object ElementKey { get; }

    #region IMappingExecutionContext Members

    public IMappingExecutionContext SetTarget(object target)
    {
        Set(target);
        return this;
    }

    protected abstract void Set(object target);

    private Dictionary<object, List<object>> MappedObjectsBySource
        => _mappedObjectsBySource ??= new Dictionary<object, List<object>>(13);

    bool IMappingExecutionContext.TryGet<TKey, TComplex>(
        TKey key,
        out TComplex complexType)
        where TComplex : class
    {
        if (_parent != null)
        {
            return _parent.TryGet(key, out complexType);
        }

        if (MappedObjectsBySource.TryGetValue(key, out var mappedTargets))
        {
            complexType = mappedTargets.OfType<TComplex>().FirstOrDefault();
            return complexType != null;
        }

        complexType = default;
        return false;
    }

    void IMappingExecutionContext.Register<TKey, TComplex>(
        TKey key,
        TComplex complexType)
    {
        if (_parent != null)
        {
            _parent.Register(key, complexType);
            return;
        }

        if (!MappedObjectsBySource.TryGetValue(key, out var mappedTargets))
        {
            MappedObjectsBySource[key] = mappedTargets = new List<object>();
        }

        mappedTargets.Add(complexType);
    }

    IMappingExecutionContext IMappingExecutionContext.AddChild<TSourceValue, TTargetValue>(
        TSourceValue sourceValue,
        TTargetValue targetValue,
        int? elementIndex,
        object elementKey,
        string targetMemberName,
        int dataSourceIndex)
    {
        return new ChildMappingExecutionContext<TSourceValue, TTargetValue>(
            sourceValue,
            targetValue,
            elementIndex,
            elementKey,
            targetMemberName,
            dataSourceIndex,
            this);
    }

    IMappingExecutionContext IMappingExecutionContext.AddElement<TSourceElement, TTargetElement>(
        TSourceElement sourceElement,
        TTargetElement targetElement,
        int elementIndex,
        object elementKey)
    {
        return new ElementMappingExecutionContext<TSourceElement, TTargetElement>(
            sourceElement,
            targetElement,
            elementIndex,
            elementKey,
            this);
    }

    object IMappingExecutionContext.Map(IMappingExecutionContext context)
    {
        var rootMapper = GetRootMapper();
        var result = rootMapper.MapSubObject((MappingExecutionContextBase2)context);

        return result;
    }

    object IMappingExecutionContext.MapRepeated(IMappingExecutionContext context)
    {
        // TODO - is this needed?
        //if (IsRoot || MappingTypes.RuntimeTypesNeeded)
        //{
        //    childMappingData.IsPartOfRepeatedMapping = true;
        //}

        var rootMapper = GetRootMapper();
        var result = rootMapper.MapRepeated((MappingExecutionContextBase2)context);

        return result;
    }

    #endregion

    #region IMappingData Members

    public IMappingData Parent => _parent;

    public TSource GetSource<TSource>()
    {
        if (Source is TSource typedSource)
        {
            return typedSource;
        }

        return default;
    }

    public TTarget GetTarget<TTarget>()
    {
        if (Target is TTarget typedTarget)
        {
            return typedTarget;
        }

        return default;
    }

    IMappingData<TSource, TTarget> IMappingData.As<TSource, TTarget>()
        => WithTypes<TSource, TTarget>();

    public abstract IMappingData<TSource, TTarget> WithTypes<TSource, TTarget>();

    #endregion

    #region IServiceProviderAccessor Members

    TService IServiceProviderAccessor.GetService<TService>()
        => this.GetServiceOrThrow<TService>(name: null);

    TService IServiceProviderAccessor.GetService<TService>(string name)
        => this.GetServiceOrThrow<TService>(name);

    TServiceProvider IServiceProviderAccessor.GetServiceProvider<TServiceProvider>()
        => this.GetServiceProviderOrThrow<TServiceProvider>();

    #endregion
}