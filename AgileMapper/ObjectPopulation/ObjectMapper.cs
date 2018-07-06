namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using Caching;
    using MapperKeys;
    using Recursion;
    using NetStandardPolyfills;
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
        private readonly ICache<ObjectMapperKeyBase, IRecursionMapperFunc> _recursionMapperFuncsByKey;
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
                _recursionMapperFuncsByKey = MapperData.MapperContext.Cache.CreateNew<ObjectMapperKeyBase, IRecursionMapperFunc>();
                MapperData.Mapper = this;
                
                CacheRecursionMapperFuncs();
            }
        }

        #region Setup

        public void CacheRecursionMapperFuncs()
        {
            // Using a for loop here because creation of a recursion mapper func can
            // cause additions to MapperData.RequiredMapperFuncKeys
            for (var i = _recursionMapperFuncsByKey.Count; i < MapperData.RequiredMapperFuncKeys.Count; i++)
            {
                var mapperKey = MapperData.RequiredMapperFuncKeys[i];

                var typesKey = new SourceAndTargetTypesKey(
                    mapperKey.MapperData.SourceType,
                    mapperKey.MapperData.TargetType);

                var mapperFuncCreator = GlobalContext.Instance.Cache.GetOrAdd(typesKey, key =>
                {
                    var mapperFuncType = typeof(RecursionMapperFunc<,>).MakeGenericType(key.SourceType, key.TargetType);
                    var mapperDataParameter = Parameters.Create<IObjectMappingData>("mappingData");
                    var lazyLoadParameter = Parameters.Create<bool>("lazyLoadFuncs");

                    var mapperFuncCreation = Expression.New(
                        mapperFuncType.GetPublicInstanceConstructor(typeof(IObjectMappingData), typeof(bool)),
                        mapperDataParameter,
                        lazyLoadParameter);

                    var mapperCreationLambda = Expression.Lambda<Func<IObjectMappingData, bool, IRecursionMapperFunc>>(
                        mapperFuncCreation,
                        mapperDataParameter,
                        lazyLoadParameter);

                    return mapperCreationLambda.Compile();
                });

                var mapperFunc = mapperFuncCreator.Invoke(
                    mapperKey.MappingData,
                    mapperKey.MappingData.MappingContext.LazyLoadRecursionMappingFuncs);

                _recursionMapperFuncsByKey.GetOrAdd(mapperKey, k => mapperFunc);
            }
        }

        #endregion

        public LambdaExpression MappingLambda { get; }

        public Expression MappingExpression => MappingLambda.Body;

        public ObjectMapperData MapperData { get; }

        public IEnumerable<IRecursionMapperFunc> RecursionMapperFuncs => _recursionMapperFuncsByKey.Values;

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

        public object MapRecursion(IObjectMappingData childMappingData)
        {
            var mapperFunc = _recursionMapperFuncsByKey
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