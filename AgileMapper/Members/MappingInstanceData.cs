namespace AgileObjects.AgileMapper.Members
{
    using NetStandardPolyfills;

    internal class MappingInstanceData<TSource, TTarget> :
        IMappingData,
        IMappingData<TSource, TTarget>
    {
        private readonly IMappingData _parent;
        private readonly IMapperContextOwner _mapperContextOwner;

        protected MappingInstanceData(IMappingData<TSource, TTarget> mappingData)
            : this(
                mappingData.Source,
                mappingData.Target,
                mappingData.ElementIndex,
                mappingData.ElementKey,
                mappingData.Parent,
              ((IMappingContextOwner)mappingData).MappingContext)
        {
        }

        public MappingInstanceData(IMappingData mappingData)
            : this(
                mappingData.GetSource<TSource>(),
                mappingData.GetTarget<TTarget>(),
                mappingData.GetElementIndex(),
                mappingData.GetElementKey(),
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
            ElementIndex = elementIndex;
            ElementKey = elementKey;
        }

        IMappingData IMappingData.Parent => _parent;

        IMappingData IMappingData<TSource, TTarget>.Parent => _parent;

        public TSource Source { get; }

        public TTarget Target { get; set; }

        public int? ElementIndex { get; }

        public object ElementKey { get; }

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

        public int? GetElementIndex() => ElementIndex ?? _parent?.GetElementIndex();

        public object GetElementKey() => ElementKey ?? _parent?.GetElementKey();

        IMappingData<TDataSource, TDataTarget> IMappingData.As<TDataSource, TDataTarget>()
            => this.ToTyped<TDataSource, TDataTarget>();

        TService IServiceProviderAccessor.GetService<TService>()
            => _mapperContextOwner.GetServiceOrThrow<TService>(name: null);

        TService IServiceProviderAccessor.GetService<TService>(string name)
            => _mapperContextOwner.GetServiceOrThrow<TService>(name);

        TServiceProvider IServiceProviderAccessor.GetServiceProvider<TServiceProvider>()
            => _mapperContextOwner.GetServiceProviderOrThrow<TServiceProvider>();
    }
}