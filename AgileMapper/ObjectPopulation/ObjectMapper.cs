namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Caching;

    internal class ObjectMapper<TSource, TTarget> : IObjectMapper
    {
        public static readonly ObjectMapper<TSource, TTarget> Unmappable = new ObjectMapper<TSource, TTarget>();

        private readonly MapperFunc<TSource, TTarget> _mapperFunc;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _subMappersByKey;

        private ObjectMapper()
        {
        }

        public ObjectMapper(
            Expression<MapperFunc<TSource, TTarget>> mappingLambda,
            IObjectMappingData mappingData)
        {
            MappingLambda = mappingLambda;
            MapperData = mappingData.MapperData;

            if (MapperData.Context.Compile)
            {
                _mapperFunc = mappingLambda.Compile();
            }
            else if (MapperData.Context.NeedsSubMapping)
            {
                MapperData.Mapper = this;
            }

            if (MapperData.Context.NeedsSubMapping)
            {
                _subMappersByKey = MapperData.MapperContext.Cache.CreateNew<ObjectMapperKeyBase, IObjectMapper>();
            }
        }

        public LambdaExpression MappingLambda { get; }

        public bool IsNullObject => this == Unmappable;

        public Expression MappingExpression => MappingLambda.Body;

        public ObjectMapperData MapperData { get; }

        public object Map(IObjectMappingData mappingData) => Map((ObjectMappingData<TSource, TTarget>)mappingData);

        public TTarget Map(ObjectMappingData<TSource, TTarget> mappingData) => _mapperFunc.Invoke(mappingData);

        public object MapRuntimeTypedSubObject(IObjectMappingData mappingData)
        {
            mappingData.MapperKey.MappingData = mappingData;

            var mapper = _subMappersByKey.GetOrAdd(
                mappingData.MapperKey,
                key =>
                {
                    var mapperToCache = key.MappingData.Mapper;

                    key.MappingData = null;

                    return mapperToCache;
                });

            return mapper.Map(mappingData);
        }
    }
}