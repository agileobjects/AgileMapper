namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Caching;
    using Recursion;
    using NetStandardPolyfills;

    internal class ObjectMapper<TSource, TTarget> : IObjectMapper
    {
        public static readonly ObjectMapper<TSource, TTarget> Unmappable = new ObjectMapper<TSource, TTarget>();

        private readonly MapperFunc<TSource, TTarget> _mapperFunc;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _subMappersByKey;
        private readonly ICache<ObjectMapperKeyBase, IRecursionMapperFunc> _recursionMappingFuncsByKey;

        private ObjectMapper()
        {
        }

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

        private ICache<ObjectMapperKeyBase, IRecursionMapperFunc> CreateRecursionMapperFuncs()
        {
            var cache = MapperData.MapperContext.Cache.CreateNew<ObjectMapperKeyBase, IRecursionMapperFunc>();

            foreach (var mappingLambdaAndKey in MapperData.RequiredMapperFuncsByKey)
            {
                var mappingLambda = mappingLambdaAndKey.Value;
                var mappingDataObject = mappingLambda.Parameters[0];
                var mappingDataTypes = mappingDataObject.Type.GetGenericTypeArguments();

                var typesKey = new SourceAndTargetTypesKey(mappingDataTypes[0], mappingDataTypes[1]);

                var mapperFuncCreator = GlobalContext.Instance.Cache.GetOrAdd(typesKey, key =>
                {
                    var mapperFuncType = typeof(RecursionMapperFunc<,>).MakeGenericType(key.SourceType, key.TargetType);
                    var lambdaParameter = Parameters.Create<LambdaExpression>("lambda");

                    var mapperFuncCreation = Expression.New(mapperFuncType.GetPublicInstanceConstructors().First(), lambdaParameter);

                    var mapperCreationLambda = Expression.Lambda<Func<LambdaExpression, IRecursionMapperFunc>>(
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

        public bool IsNullObject => this == Unmappable;

        public Expression MappingExpression => MappingLambda.Body;

        public ObjectMapperData MapperData { get; }

        public IEnumerable<IRecursionMapperFunc> RecursionMapperFuncs => _recursionMappingFuncsByKey.Values;

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
            // The cache can be null (and can contain null functions) if 
            // recursive members turn out to have all-unmappable members:
            var mapperFunc = _recursionMappingFuncsByKey?
                .GetOrAdd(childMappingData.MapperKey, null);

            return mapperFunc?.Map(childMappingData);
        }
    }
}