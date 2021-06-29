namespace AgileObjects.AgileMapper.Members
{
    using NetStandardPolyfills;

    internal class MappingInstanceData<TSource, TTarget> : IMappingData<TSource, TTarget>, IMappingData
    {
        private readonly IMappingData _parent;
        private readonly IMappingContext _mappingContext;

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

        protected MappingInstanceData(
            TSource source,
            TTarget target,
            int? elementIndex,
            object elementKey,
            IMappingData parent,
            IMappingContext mappingContext)
        {
            _parent = parent;
            _mappingContext = mappingContext;
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
                return (T)((object)Source);
            }

            return default(T);
        }

        T IMappingData.GetTarget<T>()
        {
            if (typeof(TTarget).IsAssignableTo(typeof(T)))
            {
                return (T)((object)Target);
            }

            return default(T);
        }

        public int? GetElementIndex() => ElementIndex ?? _parent?.GetElementIndex();

        public object GetElementKey() => ElementKey ?? _parent?.GetElementKey();

        IMappingData<TDataSource, TDataTarget> IMappingData.As<TDataSource, TDataTarget>()
        {
            var thisMappingData = (IMappingData)this;

            return new MappingInstanceData<TDataSource, TDataTarget>(
                thisMappingData.GetSource<TDataSource>(),
                thisMappingData.GetTarget<TDataTarget>(),
                GetElementIndex(),
                GetElementKey(),
                _parent,
                _mappingContext);
        }

        TService IServiceProviderAccessor.GetService<TService>()
            => ((IServiceProviderAccessor)this).GetService<TService>(name: null);

        TService IServiceProviderAccessor.GetService<TService>(string name)
        {
            return _mappingContext
                .MapperContext
                .UserConfigurations
                .GetServiceOrThrow<TService>(name);
        }

        TServiceProvider IServiceProviderAccessor.GetServiceProvider<TServiceProvider>()
        {
            return _mappingContext
                .MapperContext
                .UserConfigurations
                .GetServiceProviderOrThrow<TServiceProvider>();
        }
    }
}