namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Api;
    using ObjectPopulation;

    internal class MappingExecutor<TSource> : ITargetTypeSelector, IMappingContext, IDisposable
    {
        private readonly ICollection<ICachedItemRemover> _cacheCleaners;
        private readonly TSource _source;

        public MappingExecutor(TSource source, MapperContext mapperContext)
            : this(mapperContext)
        {
            _source = source;
        }

        public MappingExecutor(MappingRuleSet ruleSet, MapperContext mapperContext)
            : this(mapperContext)
        {
            RuleSet = ruleSet;
        }

        private MappingExecutor(MapperContext mapperContext)
        {
            MapperContext = mapperContext;
            _cacheCleaners = new List<ICachedItemRemover>();
        }

        public MapperContext MapperContext { get; }

        public MappingRuleSet RuleSet { get; private set; }

        public TResult ToANew<TResult>() where TResult : class
            => PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult));

        public TTarget OnTo<TTarget>(TTarget existing) where TTarget : class
            => PerformMapping(MapperContext.RuleSets.Merge, existing);

        public TTarget Over<TTarget>(TTarget existing) where TTarget : class
            => PerformMapping(MapperContext.RuleSets.Overwrite, existing);

        private TTarget PerformMapping<TTarget>(MappingRuleSet ruleSet, TTarget existing)
        {
            if (_source == null)
            {
                return existing;
            }

            RuleSet = ruleSet;

            using (this)
            {
                var rootMapperCreationData = CreateRootMappingContextData(existing);

                return Map<TSource, TTarget>(rootMapperCreationData);
            }
        }

        private IObjectMappingContextData CreateRootMappingContextData<TTarget>(TTarget target)
            => CreateRootMappingContextData(_source, target);

        public IObjectMappingContextData CreateRootMappingContextData<TDataSource, TDataTarget>(TDataSource source, TDataTarget target)
            => ObjectMappingContextDataFactory.ForRoot(source, target, this);

        public TDataTarget Map<TDataSource, TDataTarget>(IObjectMappingContextData data)
        {
            IObjectMapper<TDataTarget> mapper;

            if (data.RuntimeTypesAreTheSame)
            {
                mapper = MapperContext.ObjectMapperFactory.CreateFor<TDataSource, TDataTarget>(data);

                return mapper.Execute(data);
            }

            var cacheKey = DeclaredAndRuntimeTypesKey.ForCreateMapperCall<TDataSource, TDataTarget>(data.MapperData);

            var createMapperFunc = GlobalContext.Instance.Cache.GetOrAdd(cacheKey, key =>
            {
                var mapperFactoryParameter = Parameters.Create<ObjectMapperFactory>();

                var typedCreateMapperMethod = mapperFactoryParameter.Type
                    .GetMethod("CreateFor")
                    .MakeGenericMethod(key.RuntimeSourceType, key.RuntimeTargetType);

                var createMapperCall = Expression.Call(
                    mapperFactoryParameter,
                    typedCreateMapperMethod,
                    Parameters.ObjectMappingContextData);

                var createMapperLambda = Expression
                    .Lambda<Func<ObjectMapperFactory, IObjectMappingContextData, IObjectMapper<TDataTarget>>>(
                        createMapperCall,
                        mapperFactoryParameter,
                        Parameters.ObjectMappingContextData);

                return createMapperLambda.Compile();
            });

            mapper = createMapperFunc.Invoke(MapperContext.ObjectMapperFactory, data);

            return mapper.Execute(data);
        }

        public bool TryGet<TKey, TComplex>(TKey key, out TComplex complexType)
        {
            if (key != null)
            {
                return ObjectCache<TKey, TComplex>.Cache.TryGetValue(key, out complexType);
            }

            complexType = default(TComplex);
            return false;
        }

        public void Register<TKey, TComplex>(TKey key, TComplex complexType)
        {
            if (key == null)
            {
                return;
            }

            ObjectCache<TKey, TComplex>.Cache.Add(key, complexType);

            _cacheCleaners.Add(new ObjectCacheRemover<TKey, TComplex>(key));
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (var cleanupAction in _cacheCleaners)
            {
                cleanupAction.RemoveCachedItem();
            }

            _cacheCleaners.Clear();
        }

        #endregion

        private static class ObjectCache<TKey, TObject>
        {
            public static readonly Dictionary<TKey, TObject> Cache = new Dictionary<TKey, TObject>();
        }

        private interface ICachedItemRemover
        {
            void RemoveCachedItem();
        }

        private class ObjectCacheRemover<TKey, TObject> : ICachedItemRemover
        {
            private readonly TKey _key;

            public ObjectCacheRemover(TKey key)
            {
                _key = key;
            }

            public void RemoveCachedItem()
            {
                ObjectCache<TKey, TObject>.Cache.Remove(_key);
            }
        }
    }
}