namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
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

            if (MapperData.HasMapperFuncs)
            {
                _recursionMappingFuncsByKey = CreateRecursionMapperFuncsCache(mappingData);
            }
        }

        #region Setup

        private ICache<ObjectMapperKeyBase, IRecursionMapperFunc> CreateRecursionMapperFuncsCache(
            IObjectMappingData mappingData)
        {
            var cache = MapperData.MapperContext.Cache.CreateNew<ObjectMapperKeyBase, IRecursionMapperFunc>();

            for (var i = 0; i < MapperData.RequiredMapperFuncKeys.Count; i++)
            {
                var mapperKey = MapperData.RequiredMapperFuncKeys[i];

                var typesKey = new SourceAndTargetTypesKey(
                    mapperKey.MapperData.SourceType,
                    mapperKey.MapperData.TargetType);

                var mapperFuncCreator = GlobalContext.Instance.Cache.GetOrAdd(typesKey, key =>
                {
                    var mapperFuncType = typeof(RecursionMapperFunc<,>).MakeGenericType(key.SourceType, key.TargetType);
                    var mapperDataParameter = Parameters.Create<IObjectMappingData>("mappingData");
                    var eagerLoadParameter = Parameters.Create<bool>("eagerLoadFuncs");

                    var mapperFuncCreation = Expression.New(
                        mapperFuncType.GetPublicInstanceConstructor(typeof(IObjectMappingData), typeof(bool)),
                        mapperDataParameter,
                        eagerLoadParameter);

                    var mapperCreationLambda = Expression.Lambda<Func<IObjectMappingData, bool, IRecursionMapperFunc>>(
                        mapperFuncCreation,
                        mapperDataParameter,
                        eagerLoadParameter);

                    return mapperCreationLambda.Compile();
                });

                var mapperFunc = mapperFuncCreator.Invoke(
                    mapperKey.MappingData,
                    mappingData.MappingContext.EagerLoadRecursionMappingFuncs);

                cache.GetOrAdd(mapperKey, k => mapperFunc);
            }

            return cache;
        }

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
            // a recursive member mapping turns out to have all-unmappable 
            // members, i.e. it doesn't map anything:
            var mapperFunc = _recursionMappingFuncsByKey?
                .GetOrAdd(childMappingData.MapperKey, null);

            return mapperFunc?.Map(childMappingData);
        }
    }
}