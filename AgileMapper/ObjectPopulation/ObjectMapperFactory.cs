namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Caching;
    using ComplexTypes;
    using Enumerables;
    using Extensions.Internal;
    using Queryables;
    using Validation;

    internal class ObjectMapperFactory
    {
        private readonly IList<MappingExpressionFactoryBase> _mappingExpressionFactories;
        private readonly ICache<ObjectMapperKeyBase, IObjectMapper> _rootMappersCache;
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

            _rootMappersCache = mapperScopedCacheSet.CreateScoped<ObjectMapperKeyBase, IObjectMapper>();
        }

        public IEnumerable<IObjectMapper> RootMappers => _rootMappersCache.Values;

        public void RegisterCreationCallback(MapperCreationCallbackKey creationCallbackKey, Action<IObjectMapper> callback)
        {
            if (_creationCallbacksByKey == null)
            {
                _creationCallbacksByKey =
                    new Dictionary<MapperCreationCallbackKey, Action<IObjectMapper>>(MapperCreationCallbackKey.Comparer);
            }

            _creationCallbacksByKey.Add(creationCallbackKey, callback);
        }

        public ObjectMapper<TSource, TTarget> GetOrCreateRoot<TSource, TTarget>(ObjectMappingData<TSource, TTarget> mappingData)
        {
            if (TryGetStaticallyCachedMapper(mappingData, out var mapper))
            {
                return mapper;
            }

            // TODO: This is not thread-safe when using a fixed-types RootObjectMapperKey!
            mappingData.MapperKey.MappingData = mappingData;

            mapper = (ObjectMapper<TSource, TTarget>)_rootMappersCache.GetOrAdd(
                mappingData.MapperKey,
                key =>
                {
                    var mapperToCache = key.MappingData.Mapper;
                    var data = key.MappingData;

                    key.MappingData = null;

                    if (mapperToCache.MapperData.MapperContext.UserConfigurations.ValidateMappingPlans)
                    {
                        MappingValidator.Validate(mapperToCache.MapperData);
                    }

                    if (mapperToCache.IsStaticallyCacheable(key))
                    {
                        var ruleSets = data.MapperData.MapperContext.RuleSets;

                        if (data.MappingContext.RuleSet == ruleSets.CreateNew)
                        {
                            if (StaticCreateNewMapperCache<TSource, TTarget>.Mapper != null)
                            {
                                goto SkipStaticCaching;
                            }

                            return StaticCreateNewMapperCache<TSource, TTarget>.SetMapper((ObjectMapper<TSource, TTarget>)mapperToCache);
                        }

                        if (data.MappingContext.RuleSet == ruleSets.Overwrite)
                        {
                            if (StaticOverwriteMapperCache<TSource, TTarget>.Mapper != null)
                            {
                                goto SkipStaticCaching;
                            }

                            return StaticOverwriteMapperCache<TSource, TTarget>.SetMapper((ObjectMapper<TSource, TTarget>)mapperToCache);
                        }

                        if (data.MappingContext.RuleSet == ruleSets.Project)
                        {
                            if (StaticProjectionMapperCache<TSource, TTarget>.Mapper != null)
                            {
                                goto SkipStaticCaching;
                            }

                            return StaticProjectionMapperCache<TSource, TTarget>.SetMapper((ObjectMapper<TSource, TTarget>)mapperToCache);
                        }

                        if (data.MappingContext.RuleSet == ruleSets.Merge)
                        {
                            if (StaticMergeMapperCache<TSource, TTarget>.Mapper != null)
                            {
                                goto SkipStaticCaching;
                            }

                            return StaticMergeMapperCache<TSource, TTarget>.SetMapper((ObjectMapper<TSource, TTarget>)mapperToCache);
                        }
                    }

                    SkipStaticCaching:

                    return mapperToCache;
                });

            return mapper;
        }

        public ObjectMapper<TSource, TTarget> Create<TSource, TTarget>(ObjectMappingData<TSource, TTarget> mappingData)
        {
            var mappingExpressionFactory = _mappingExpressionFactories.First(mef => mef.IsFor(mappingData));
            var mappingExpression = mappingExpressionFactory.Create(mappingData);

            if (mappingExpression.NodeType == ExpressionType.Default)
            {
                return ObjectMapper<TSource, TTarget>.Unmappable;
            }

            mappingExpression = MappingFactory
                .UseLocalSourceValueVariableIfAppropriate(mappingExpression, mappingData.MapperData);

            if (mappingData.MapperData.IsEntryPoint)
            {
                mappingExpression = mappingData.MapperData.Finalise(mappingExpression);
            }

            var mappingLambda = Expression.Lambda<MapperFunc<TSource, TTarget>>(
                mappingExpression,
                mappingData.MapperData.MappingDataObject,
                Parameters.RepeatedMappingFuncs);

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

        #region Static Caches

        private static bool TryGetStaticallyCachedMapper<TSource, TTarget>(
            ObjectMappingData<TSource, TTarget> mappingData,
            out ObjectMapper<TSource, TTarget> mapper)
        {
            var ruleSet = mappingData.MappingContext.RuleSet;
            var ruleSets = mappingData.MapperContext.RuleSets;

            if (ruleSet == ruleSets.CreateNew)
            {
                if (StaticCreateNewMapperCache<TSource, TTarget>.Mapper?.MapperData.MapperContext != mappingData.MapperContext)
                {
                    goto NoCachedMapper;
                }

                mapper = StaticCreateNewMapperCache<TSource, TTarget>.Mapper;
                return true;
            }

            if (ruleSet == ruleSets.Overwrite)
            {
                if (StaticOverwriteMapperCache<TSource, TTarget>.Mapper?.MapperData.MapperContext != mappingData.MapperContext)
                {
                    goto NoCachedMapper;
                }

                mapper = StaticOverwriteMapperCache<TSource, TTarget>.Mapper;
                return true;
            }

            if (ruleSet == ruleSets.Project)
            {
                if (StaticProjectionMapperCache<TSource, TTarget>.Mapper?.MapperData.MapperContext != mappingData.MapperContext)
                {
                    goto NoCachedMapper;
                }

                mapper = StaticProjectionMapperCache<TSource, TTarget>.Mapper;
                return true;
            }

            if (ruleSet == ruleSets.Merge)
            {
                if (StaticMergeMapperCache<TSource, TTarget>.Mapper?.MapperData.MapperContext != mappingData.MapperContext)
                {
                    goto NoCachedMapper;
                }

                mapper = StaticMergeMapperCache<TSource, TTarget>.Mapper;
                return true;
            }

            NoCachedMapper:

            mapper = null;
            return false;
        }

        private static class StaticCreateNewMapperCache<TSource, TTarget>
        {
            public static ObjectMapper<TSource, TTarget> Mapper { get; private set; }

            public static ObjectMapper<TSource, TTarget> SetMapper(ObjectMapper<TSource, TTarget> mapper)
                => Mapper = mapper.WithResetCallback(Reset);

            private static void Reset() => Mapper = null;
        }

        private static class StaticOverwriteMapperCache<TSource, TTarget>
        {
            public static ObjectMapper<TSource, TTarget> Mapper { get; private set; }

            public static ObjectMapper<TSource, TTarget> SetMapper(ObjectMapper<TSource, TTarget> mapper)
                => Mapper = mapper.WithResetCallback(Reset);

            private static void Reset() => Mapper = null;
        }

        private static class StaticProjectionMapperCache<TSource, TTarget>
        {
            public static ObjectMapper<TSource, TTarget> Mapper { get; private set; }

            public static ObjectMapper<TSource, TTarget> SetMapper(ObjectMapper<TSource, TTarget> mapper)
                => Mapper = mapper.WithResetCallback(Reset);

            private static void Reset() => Mapper = null;
        }

        private static class StaticMergeMapperCache<TSource, TTarget>
        {
            public static ObjectMapper<TSource, TTarget> Mapper { get; private set; }

            public static ObjectMapper<TSource, TTarget> SetMapper(ObjectMapper<TSource, TTarget> mapper)
                => Mapper = mapper.WithResetCallback(Reset);

            private static void Reset() => Mapper = null;
        }

        #endregion

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