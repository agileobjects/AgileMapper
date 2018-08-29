namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using Caching;
    using MapperKeys;
    using NetStandardPolyfills;
    using RepeatedMappings;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ObjectMapper<TSource, TTarget> : IObjectMapper
    {
        private readonly ObjectMapperKeyBase _mapperKey;
        private readonly MapperFunc<TSource, TTarget> _mapperFunc;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _subMappersByKey;
        private readonly ICache<ObjectMapperKeyBase, IRepeatedMapperFunc> _repeatedMappingFuncsByKey;
        private Action _resetCallback;

        public ObjectMapper(
            Expression<MapperFunc<TSource, TTarget>> mappingLambda,
            IObjectMappingData mappingData)
        {
            _mapperKey = mappingData.MapperKey;
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
                _repeatedMappingFuncsByKey = MapperData.MapperContext.Cache.CreateNew<ObjectMapperKeyBase, IRepeatedMapperFunc>();
                MapperData.Mapper = this;

                CacheRepeatedMappingFuncs();
            }
        }

        #region Setup

        public void CacheRepeatedMappingFuncs()
        {
            // Using a for loop here because creation of a repeated mapping func can
            // cause additions to MapperData.RequiredMapperFuncKeys
            for (var i = _repeatedMappingFuncsByKey.Count; i < MapperData.RequiredMapperFuncKeys.Count; i++)
            {
                var mapperKey = MapperData.RequiredMapperFuncKeys[i];

                var typesKey = new SourceAndTargetTypesKey(
                    mapperKey.MapperData.SourceType,
                    mapperKey.MapperData.TargetType);

                var mapperFuncCreator = GlobalContext.Instance.Cache.GetOrAdd(typesKey, key =>
                {
                    var mapperFuncType = typeof(RepeatedMapperFunc<,>).MakeGenericType(key.SourceType, key.TargetType);
                    var mapperDataParameter = Parameters.Create<IObjectMappingData>("mappingData");
                    var lazyLoadParameter = Parameters.Create<bool>("lazyLoadFuncs");

                    var mapperFuncCreation = Expression.New(
                        mapperFuncType.GetPublicInstanceConstructor(typeof(IObjectMappingData), typeof(bool)),
                        mapperDataParameter,
                        lazyLoadParameter);

                    var mapperCreationLambda = Expression.Lambda<Func<IObjectMappingData, bool, IRepeatedMapperFunc>>(
                        mapperFuncCreation,
                        mapperDataParameter,
                        lazyLoadParameter);

                    return mapperCreationLambda.Compile();
                },
                default(HashCodeComparer<SourceAndTargetTypesKey>));

                var mapperFunc = mapperFuncCreator.Invoke(
                    mapperKey.MappingData,
                    mapperKey.MappingData.MappingContext.LazyLoadRepeatMappingFuncs);

                _repeatedMappingFuncsByKey.GetOrAdd(mapperKey, k => mapperFunc);
            }
        }

        #endregion

        public LambdaExpression MappingLambda { get; }

        public Expression MappingExpression => MappingLambda.Body;

        public ObjectMapperData MapperData { get; }

        public IEnumerable<IRepeatedMapperFunc> RepeatedMappingFuncs => _repeatedMappingFuncsByKey.Values;

        public bool IsStaticallyCacheable()
        {
            if (_mapperKey.HasTypeTester)
            {
                return false;
            }

            if (_subMappersByKey == null)
            {
                return true;
            }

            for (var i = 0; i < _subMappersByKey.Count; i++)
            {
                var subMapperByKey = _subMappersByKey[i];

                if (!subMapperByKey.Value.IsStaticallyCacheable())
                {
                    return false;
                }
            }

            return true;
        }

        public object Map(IObjectMappingData mappingData) => Map((ObjectMappingData<TSource, TTarget>)mappingData);

        public TTarget Map(ObjectMappingData<TSource, TTarget> mappingData) => _mapperFunc.Invoke(mappingData);

        public object MapSubObject(IObjectMappingData mappingData)
        {
            mappingData.MapperKey.MappingData = mappingData;

            var mapper = _subMappersByKey.GetOrAdd(
                mappingData.MapperKey,
                key => key.MappingData.GetOrCreateMapper());

            return mapper.Map(mappingData);
        }

        public object MapRepeated(IObjectMappingData childMappingData)
        {
            var mapperFunc = _repeatedMappingFuncsByKey
                .GetOrAdd(childMappingData.MapperKey, null);

            return mapperFunc.Map(childMappingData);
        }

        public ObjectMapper<TSource, TTarget> WithResetCallback(Action callback)
        {
            _resetCallback = callback;
            return this;
        }

        public void Reset() => _resetCallback?.Invoke();
    }
}