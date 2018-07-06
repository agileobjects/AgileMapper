namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using Caching;
    using ComplexTypes;
    using Enumerables;
    using Extensions.Internal;
    using MapperKeys;
    using Queryables;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ObjectMapperFactory
    {
        private readonly IList<MappingExpressionFactoryBase> _mappingExpressionFactories;
        private readonly ICache<IRootMapperKey, IObjectMapper> _rootMappersCache;
        private Dictionary<MapperCreationCallbackKey, Action<IObjectMapper>> _creationCallbacksByKey;

        public ObjectMapperFactory(CacheSet mapperScopedCacheSet)
        {
            _mappingExpressionFactories = new[]
            {
                QueryProjectionExpressionFactory.Instance,
                DictionaryMappingExpressionFactory.Instance,
                SimpleTypeMappingExpressionFactory.Instance,
                EnumerableMappingExpressionFactory.Instance,
                ComplexTypeMappingExpressionFactory.Instance
            };

            _rootMappersCache = mapperScopedCacheSet.CreateScoped<IRootMapperKey, IObjectMapper>(default(RootMapperKeyComparer));
        }

        public IEnumerable<IObjectMapper> RootMappers => _rootMappersCache.Values;

        public void RegisterCreationCallback(MapperCreationCallbackKey creationCallbackKey, Action<IObjectMapper> callback)
        {
            if (_creationCallbacksByKey == null)
            {
                _creationCallbacksByKey =
                    new Dictionary<MapperCreationCallbackKey, Action<IObjectMapper>>(default(MapperCreationCallbackKey.Comparer));
            }

            _creationCallbacksByKey.Add(creationCallbackKey, callback);
        }

        public ObjectMapper<TSource, TTarget> GetOrCreateRoot<TSource, TTarget>(ObjectMappingData<TSource, TTarget> mappingData)
        {
            if (StaticMapperCache<TSource, TTarget>.TryGetMapperFor(mappingData, out var mapper))
            {
                return mapper;
            }

            var rootMapperKey = mappingData.EnsureRootMapperKey();

            mapper = (ObjectMapper<TSource, TTarget>)_rootMappersCache.GetOrAdd(
                rootMapperKey,
                key => key.MappingData.GetOrCreateMapper());

            return mapper;
        }

        public ObjectMapper<TSource, TTarget> Create<TSource, TTarget>(ObjectMappingData<TSource, TTarget> mappingData)
        {
            var mappingExpressionFactory = _mappingExpressionFactories.First(mef => mef.IsFor(mappingData));
            var mappingExpression = mappingExpressionFactory.Create(mappingData);

            if (mappingExpression.NodeType == ExpressionType.Default)
            {
                return null;
            }

            mappingExpression = MappingFactory
                .UseLocalSourceValueVariableIfAppropriate(mappingExpression, mappingData.MapperData);

            var mappingLambda = Expression.Lambda<MapperFunc<TSource, TTarget>>(
                mappingExpression,
                mappingData.MapperData.MappingDataObject);

            var mapper = new ObjectMapper<TSource, TTarget>(mappingLambda, mappingData);

            if (_creationCallbacksByKey == null)
            {
                return mapper;
            }

            var creationCallbackKey = new MapperCreationCallbackKey(mappingData.MapperData);

            if (_creationCallbacksByKey.TryGetValue(creationCallbackKey, out var creationCallback))
            {
                creationCallback.Invoke(mapper);
            }

            return mapper;
        }

        public void Reset()
        {
            foreach (var mappingExpressionFactory in _mappingExpressionFactories)
            {
                mappingExpressionFactory.Reset();
            }

            foreach (var mapper in _rootMappersCache.Values)
            {
                mapper.Reset();
            }

            _rootMappersCache.Empty();
        }
    }
}