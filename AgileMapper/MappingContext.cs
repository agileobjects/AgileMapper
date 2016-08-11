namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Members;
    using ObjectPopulation;

    internal class MappingContext : IDisposable
    {
        internal static readonly MethodInfo TryGetMethod = typeof(MappingContext).GetMethod("TryGet", Constants.PublicInstance);
        internal static readonly MethodInfo RegisterMethod = typeof(MappingContext).GetMethod("Register", Constants.PublicInstance);

        private readonly ICollection<Action> _cleanupActions;

        internal MappingContext(MappingRuleSet ruleSet, MapperContext mapperContext)
        {
            RuleSet = ruleSet;
            MapperContext = mapperContext;
            _cleanupActions = new List<Action>();
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

            _cleanupActions.Add(() => ObjectCache<TKey, TComplex>.Cache.Remove(key));
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

        internal TTarget MapChild<TSource, TTarget>(IObjectMapperCreationData childData)
            => Map<TSource, TTarget>(childData);

        private TTarget Map<TSource, TTarget>(IObjectMapperCreationData data)
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
                var mapperContext = Expression.Property(Parameters.ObjectMapperData, "MapperContext");
                var mapperFactory = Expression.Property(mapperContext, "ObjectMapperFactory");

                var typedCreateMapperMethod = typeof(ObjectMapperFactory)
                    .GetMethod("CreateFor", Constants.PublicInstance)
                    .MakeGenericMethod(data.MapperData.SourceType, data.MapperData.TargetType);

                var createMapperCall = Expression.Call(
                    mapperFactory,
                    typedCreateMapperMethod,
                    Parameters.ObjectMapperData);

                var createMapperLambda = Expression
                    .Lambda<Func<ObjectMapperData, IObjectMapper<TTarget>>>(
                        createMapperCall,
                        Parameters.ObjectMapperData);

                return createMapperLambda.Compile();
            });

            mapper = createMapperFunc.Invoke(data.MapperData);

            return mapper.Execute(data);
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (var cleanupAction in _cleanupActions)
            {
                cleanupAction.Invoke();
            }
        }

        #endregion

        private static class ObjectCache<TKey, TObject>
        {
            public static readonly Dictionary<TKey, TObject> Cache = new Dictionary<TKey, TObject>();
        }
    }
}