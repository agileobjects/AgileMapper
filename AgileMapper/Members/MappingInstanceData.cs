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
                mappingData.Parent,
                ((IMappingContextOwner)mappingData).MappingContext)
        {
        }

        protected MappingInstanceData(
            TSource source,
            TTarget target,
            int? elementIndex,
            IMappingData parent,
            IMappingContext mappingContext)
        {
            _parent = parent;
            _mappingContext = mappingContext;
            Source = source;
            Target = target;
            ElementIndex = elementIndex;
        }

        IMappingData IMappingData.Parent => _parent;

        IMappingData IMappingData<TSource, TTarget>.Parent => _parent;

        public TSource Source { get; }

        public TTarget Target { get; set; }

        int? IMappingData<TSource, TTarget>.EnumerableIndex => ElementIndex;

        public int? ElementIndex { get; }

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

        int? IMappingData.GetEnumerableIndex() => GetElementIndex();

        public int? GetElementIndex() => ElementIndex ?? _parent?.GetElementIndex();

        IMappingData<TDataSource, TDataTarget> IMappingData.As<TDataSource, TDataTarget>()
        {
            var thisMappingData = (IMappingData)this;

            return new MappingInstanceData<TDataSource, TDataTarget>(
                thisMappingData.GetSource<TDataSource>(),
                thisMappingData.GetTarget<TDataTarget>(),
                GetElementIndex(),
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