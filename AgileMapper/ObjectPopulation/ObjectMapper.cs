namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Caching;

    internal class ObjectMapper<TSource, TTarget> : IObjectMapper
    {
        private readonly MapperFunc<TSource, TTarget> _mapperFunc;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _subMappersByKey;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapperFunc> _recursionMappingFuncsByKey;

        public ObjectMapper(
            Expression<MapperFunc<TSource, TTarget>> mappingLambda,
            ObjectMapperData mapperData)
        {
            MappingLambda = mappingLambda;
            MapperData = mapperData;

            if (mapperData.Context.Compile)
            {
                _mapperFunc = mappingLambda.Compile();
            }
            else if (mapperData.Context.NeedsSubMapping)
            {
                mapperData.Mapper = this;
            }

            if (mapperData.Context.NeedsSubMapping)
            {
                _subMappersByKey = mapperData.MapperContext.Cache.CreateNew<ObjectMapperKeyBase, IObjectMapper>();
            }

            if (mapperData.HasMapperFuncs)
            {
                _recursionMappingFuncsByKey = CreateRecursionMapperFuncs();
            }
        }

        private ICache<ObjectMapperKeyBase, IObjectMapperFunc> CreateRecursionMapperFuncs()
        {
            var cache = MapperData.MapperContext.Cache.CreateNew<ObjectMapperKeyBase, IObjectMapperFunc>();

            foreach (var mappingLambdaAndKey in MapperData.RequiredMapperFuncsByKey)
            {
                var mappingLambda = mappingLambdaAndKey.Value;
                var mappingDataObject = mappingLambda.Parameters[0];
                var mappingDataTypes = mappingDataObject.Type.GetGenericArguments();

                var typesKey = new SourceAndTargetTypesKey(mappingDataTypes[0], mappingDataTypes[1]);

                var mapperFuncCreator = GlobalContext.Instance.Cache.GetOrAdd(typesKey, key =>
                {
                    var mapperFuncType = typeof(RecursionMapperFunc<,>).MakeGenericType(key.SourceType, key.TargetType);
                    var lambdaParameter = Parameters.Create<LambdaExpression>("lambda");

                    var mapperFuncCreation = Expression.New(mapperFuncType.GetConstructors()[0], lambdaParameter);

                    var mapperCreationLambda = Expression.Lambda<Func<LambdaExpression, IObjectMapperFunc>>(
                        mapperFuncCreation,
                        lambdaParameter);

                    return mapperCreationLambda.Compile();
                });

                var mapperFunc = mapperFuncCreator.Invoke(mappingLambda);

                cache.GetOrAdd(mappingLambdaAndKey.Key, k => mapperFunc);
            }

            return cache;
        }

        #region Setup

        #endregion

        public LambdaExpression MappingLambda { get; }

        public Expression MappingExpression => MappingLambda.Body;

        public ObjectMapperData MapperData { get; }

        public object Map(IObjectMappingData mappingData) => Map((ObjectMappingData<TSource, TTarget>)mappingData);

        public TTarget Map(ObjectMappingData<TSource, TTarget> mappingData) => _mapperFunc.Invoke(mappingData);

        public object MapSubObject(IObjectMappingData mappingData)
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

        public object MapRecursion(IObjectMappingData childMappingData)
        {
            var mapperFunc = _recursionMappingFuncsByKey
                .GetOrAdd(childMappingData.MapperKey, null);

            return mapperFunc.Map(childMappingData);
        }
    }
}