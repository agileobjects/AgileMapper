namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using Caching;
    using Caching.Dictionaries;
    using DataSources.Factories.Mapping;
    using MapperKeys;

    internal class ObjectMapperFactory
    {
        private readonly ICache<IRootMapperKey, IObjectMapper> _rootMappersCache;
        private ISimpleDictionary<MapperCreationCallbackKey, Action<IObjectMapper>> _creationCallbacksByKey;

        public ObjectMapperFactory(CacheSet mapperScopedCacheSet)
        {
            _rootMappersCache = mapperScopedCacheSet
                .CreateScoped<IRootMapperKey, IObjectMapper>(CompareRootMapperKeys);
        }

        private static bool CompareRootMapperKeys(IRootMapperKey x, IRootMapperKey y)
        {
            // ReSharper disable PossibleNullReferenceException
            return ReferenceEquals(x.RuleSet, y.RuleSet) && x.Equals(y);
            // ReSharper restore PossibleNullReferenceException
        }

        public IEnumerable<IObjectMapper> RootMappers => _rootMappersCache.Values;

        public void RegisterCreationCallback(MapperCreationCallbackKey creationCallbackKey, Action<IObjectMapper> callback)
        {
            _creationCallbacksByKey ??=
                new ExpandableSimpleDictionary<MapperCreationCallbackKey, Action<IObjectMapper>>(
                    3,
                    default(MapperCreationCallbackKey.Comparer));

            _creationCallbacksByKey.Add(creationCallbackKey, callback);
        }

        public IObjectMapper GetOrCreateRoot<TSource, TTarget>(
            IObjectMapperFactoryData mappingContext)
        {
            if (StaticMapperCache<TSource, TTarget>.TryGetMapperFor(mappingContext, out var mapper))
            {
                return mapper;
            }

            var rootMapperKey = (IRootMapperKey)mappingContext.GetMapperKey();

            var untypedMapper = _rootMappersCache.GetOrAdd(
                rootMapperKey,
                key => key.CreateMappingData().GetOrCreateMapper());

            return untypedMapper;
        }

        public ObjectMapper<TSource, TTarget> Create<TSource, TTarget>(ObjectMappingData<TSource, TTarget> mappingData)
        {
            var mappingExpression = MappingDataSourceSetFactory.CreateFor(mappingData).BuildValue();

            if (mappingExpression == Constants.EmptyExpression)
            {
                return null;
            }

            mappingExpression = MappingFactory
                .UseLocalSourceValueVariableIfAppropriate(mappingExpression, mappingData.MapperData);

            var mapper = new ObjectMapper<TSource, TTarget>(mappingExpression, mappingData);

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
            foreach (var mapper in _rootMappersCache.Values)
            {
                mapper.Reset();
            }

            _rootMappersCache.Empty();
        }
    }
}