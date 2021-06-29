namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching;
    using MapperKeys;
    using NetStandardPolyfills;
    using RepeatedMappings;

    internal class ObjectMapper<TSource, TTarget> : IObjectMapper
    {
        private readonly ObjectMapperKeyBase _mapperKey;
        private MapperFunc<TSource, TTarget> _mapperFunc;
        private readonly object _lazyMapperFuncSync;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _subMappersByKey;
        private readonly ICache<ObjectMapperKeyBase, IRepeatedMapperFunc> _repeatedMappingFuncsByKey;
        private bool _mapperFuncCompiled;
        private Action _resetCallback;

        public ObjectMapper(Expression mapping, IObjectMappingData mappingData)
        {
            _mapperKey = mappingData.MapperKey;
            Mapping = mapping;
            MapperData = mappingData.MapperData;

            var mapperDataContext = MapperData.Context;

            if (mapperDataContext.Compile)
            {
                if (mappingData.MappingContext.PlanSettings.LazyCompile)
                {
                    _mapperFunc = LazyCompileMapperFunc;
                    _lazyMapperFuncSync = new object();
                }
                else
                {
                    _mapperFunc = CompileMapperFunc();
                }
            }
            else if (mapperDataContext.NeedsRuntimeTypedMapping)
            {
                MapperData.Mapper = this;
            }

            if (mapperDataContext.NeedsRuntimeTypedMapping)
            {
                _subMappersByKey = Cache.CreateNew<ObjectMapperKeyBase, IObjectMapper>();
            }

            if (MapperData.HasRepeatedMapperFuncs)
            {
                _repeatedMappingFuncsByKey = Cache.CreateNew<ObjectMapperKeyBase, IRepeatedMapperFunc>();
                MapperData.Mapper = this;

                CacheRepeatedMappingFuncs();
            }
        }

        #region Setup

        private TTarget LazyCompileMapperFunc(ObjectMappingData<TSource, TTarget> mappingData)
        {
            lock (_lazyMapperFuncSync)
            {
                if (_mapperFuncCompiled)
                {
                    return Map(mappingData);
                }

                _mapperFunc = CompileMapperFunc();
                _mapperFuncCompiled = true;
            }

            return Map(mappingData);
        }

        private MapperFunc<TSource, TTarget> CompileMapperFunc()
            => GetMappingLambda().Compile();

        public void CacheRepeatedMappingFuncs()
        {
            // Using a for loop here because creation of a repeated mapping func can
            // cause additions to MapperData.RequiredMapperFuncKeys
            for (var i = _repeatedMappingFuncsByKey.Count; i < MapperData.RepeatedMapperFuncKeys.Count; i++)
            {
                var mapperKey = MapperData.RepeatedMapperFuncKeys[i];

                var typesKey = new SourceAndTargetTypesKey(
                    mapperKey.MapperData.SourceType,
                    mapperKey.MapperData.TargetType);

                var mapperFuncCreator = GlobalContext.Instance.Cache.GetOrAddWithHashCodes(typesKey, key =>
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
                });

                var mapperFunc = mapperFuncCreator.Invoke(
                    mapperKey.MappingData,
                    mapperKey.MappingData.MappingContext.PlanSettings.LazyLoadRepeatMappingFuncs);

                _repeatedMappingFuncsByKey.GetOrAdd(mapperKey, _ => mapperFunc);
            }
        }

        #endregion

        public Expression Mapping { get; }

        LambdaExpression IObjectMapper.GetMappingLambda() => GetMappingLambda();

        private Expression<MapperFunc<TSource, TTarget>> GetMappingLambda()
        {
            return Expression.Lambda<MapperFunc<TSource, TTarget>>(
                Mapping,
                MapperData.MappingDataObject);
        }

        public ObjectMapperData MapperData { get; }

        private CacheSet Cache => MapperData.MapperContext.Cache;

        public IEnumerable<IRepeatedMapperFunc> RepeatedMappingFuncs => _repeatedMappingFuncsByKey.Values;

        public bool IsStaticallyCacheable()
        {
            if (_mapperKey.HasTypeTester)
            {
                return false;
            }

            return _subMappersByKey?.Values.All(subMapperByKey => subMapperByKey.IsStaticallyCacheable()) == true;
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