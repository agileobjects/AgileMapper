namespace AgileObjects.AgileMapper.Members;

using NetStandardPolyfills;

internal class MappingInstanceData<TSource, TTarget> :
    IMapperContextOwner,
    IMappingData,
    IMappingData<TSource, TTarget>
{
    private readonly IMappingData _parent;
    private readonly IMapperContextOwner _mapperContextOwner;
    private readonly int? _elementIndex;
    private readonly object _elementKey;

    protected MappingInstanceData(IMappingData<TSource, TTarget> mappingData)
        : this(
            mappingData.Source,
            mappingData.Target,
            mappingData.ElementIndex,
            mappingData.ElementKey,
            mappingData.Parent,
           (IMapperContextOwner)mappingData)
    {
    }

    public MappingInstanceData(IMappingData mappingData)
        : this(
            mappingData.GetSource<TSource>(),
            mappingData.GetTarget<TTarget>(),
            mappingData.ElementIndex,
            mappingData.ElementKey,
            mappingData.Parent,
           (IMapperContextOwner)mappingData)
    {
    }

    public MappingInstanceData(
        TSource source,
        TTarget target,
        int? elementIndex,
        object elementKey,
        IMappingData parent,
        IMapperContextOwner mapperContextOwner)
    {
        _parent = parent;
        _mapperContextOwner = mapperContextOwner;
        Source = source;
        Target = target;
        _elementIndex = elementIndex;
        _elementKey = elementKey;
    }

    MapperContext IMapperContextOwner.MapperContext
        => _mapperContextOwner.MapperContext;

    IMappingData IMappingData.Parent => _parent;

    IMappingData IMappingData<TSource, TTarget>.Parent => _parent;

    public TSource Source { get; }

    object IMappingData.Source => Source;

    public TTarget Target { get; set; }

    object IMappingData.Target => Target;

    public int? ElementIndex => _elementIndex ?? _parent?.ElementIndex;

    public object ElementKey => _elementKey ?? _parent?.ElementKey;

    T IMappingData.GetSource<T>()
    {
        if (typeof(TSource).IsAssignableTo(typeof(T)))
        {
            return (T)(object)Source;
        }

        return default;
    }

    T IMappingData.GetTarget<T>()
    {
        if (typeof(TTarget).IsAssignableTo(typeof(T)))
        {
            return (T)(object)Target;
        }

        return default;
    }

    IMappingData<TDataSource, TDataTarget> IMappingData.As<TDataSource, TDataTarget>()
        => this.ToTyped<TDataSource, TDataTarget>();

    TService IServiceProviderAccessor.GetService<TService>()
        => _mapperContextOwner.GetServiceOrThrow<TService>(name: null);

    TService IServiceProviderAccessor.GetService<TService>(string name)
        => _mapperContextOwner.GetServiceOrThrow<TService>(name);

    TServiceProvider IServiceProviderAccessor.GetServiceProvider<TServiceProvider>()
        => _mapperContextOwner.GetServiceProviderOrThrow<TServiceProvider>();
}