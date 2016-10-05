namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using ObjectPopulation;

    internal class MappingContext : IDisposable
    {
        internal static readonly MethodInfo TryGetMethod = typeof(MappingContext).GetMethod("TryGet");
        internal static readonly MethodInfo RegisterMethod = typeof(MappingContext).GetMethod("Register");

        private readonly ICollection<ICachedItemRemover> _cacheCleaners;

        internal MappingContext(MappingRuleSet ruleSet, MapperContext mapperContext)
        {
            RuleSet = ruleSet;
            MapperContext = mapperContext;
            _cacheCleaners = new List<ICachedItemRemover>();
        }

        internal MapperContext MapperContext { get; }

        public MappingRuleSet RuleSet { get; }

        internal TTarget MapStart<TSource, TTarget>(TSource source, TTarget target)
        {
            if (source == null)
            {
                return target;
            }

            var rootMapperCreationData = CreateRootMapperCreationData(source, target);

            return Map<TSource, TTarget>(rootMapperCreationData);
        }

        internal IObjectMapperCreationData CreateRootMapperCreationData<TSource, TTarget>(TSource source, TTarget target)
            => MapperCreationDataFactory.CreateRoot(this, source, target);

        public void Register<TKey, TComplex>(TKey key, TComplex complexType)
        {
            if (key == null)
            {
                return;
            }

            ObjectCache<TKey, TComplex>.Cache.Add(key, complexType);

            _cacheCleaners.Add(new ObjectCacheRemover<TKey, TComplex>(key));
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

        internal TTarget Map<TSource, TTarget>(IObjectMapperCreationData data)
        {
            IObjectMapper<TTarget> mapper;

            if (data.MapperData.RuntimeTypesAreTheSame)
            {
                mapper = MapperContext.ObjectMapperFactory.CreateFor<TSource, TTarget>(data);

                return mapper.Execute(data);
            }

            var cacheKey = DeclaredAndRuntimeTypesKey.ForCreateMapperCall<TSource, TTarget>(data.MapperData);

            var createMapperFunc = GlobalContext.Instance.Cache.GetOrAdd(cacheKey, k =>
            {
                var mapperFactoryParameter = Parameters.Create<ObjectMapperFactory>();

                var typedCreateMapperMethod = mapperFactoryParameter.Type
                    .GetMethod("CreateFor")
                    .MakeGenericMethod(data.MapperData.SourceType, data.MapperData.TargetType);

                var createMapperCall = Expression.Call(
                    mapperFactoryParameter,
                    typedCreateMapperMethod,
                    Parameters.ObjectMapperCreationData);

                var createMapperLambda = Expression
                    .Lambda<Func<ObjectMapperFactory, IObjectMapperCreationData, IObjectMapper<TTarget>>>(
                        createMapperCall,
                        mapperFactoryParameter,
                        Parameters.ObjectMapperCreationData);

                return createMapperLambda.Compile();
            });

            mapper = createMapperFunc.Invoke(MapperContext.ObjectMapperFactory, data);

            return mapper.Execute(data);
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