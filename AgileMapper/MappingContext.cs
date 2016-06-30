namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class MappingContext : IDisposable
    {
        #region Cached Items

        private static readonly object _cacheLock = new object();

        #endregion

        private readonly ICollection<Action> _cleanupActions;

        internal MappingContext(MappingRuleSet ruleSet, MapperContext mapperContext)
        {
            RuleSet = ruleSet;
            MapperContext = mapperContext;
            _cleanupActions = new List<Action>();
        }

        internal GlobalContext GlobalContext => MapperContext.GlobalContext;

        internal MapperContext MapperContext { get; }

        public MappingRuleSet RuleSet { get; }

        internal IObjectMappingContext CurrentObjectMappingContext { get; private set; }

        internal TTarget MapStart<TSource, TTarget>(TSource source, TTarget existing)
        {
            if (source == null)
            {
                return existing;
            }

            CreateRootObjectContext(source, existing);

            return Map<TSource, TTarget>();
        }

        internal IObjectMappingContext CreateRootObjectContext<TSource, TTarget>(
            TSource source,
            TTarget existing)
            => (CurrentObjectMappingContext = ObjectMappingContextFactory.CreateRoot(source, existing, this));

        public void Register<TKey, TComplex>(TKey key, TComplex complexType)
        {
            lock (_cacheLock)
            {
                ObjectCache<TKey, TComplex>.Cache.Add(key, complexType);
            }

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

        internal TTarget MapChild<TSource, TTarget>(ObjectMappingCommand<TSource, TTarget> command)
        {
            CurrentObjectMappingContext = command.ToOmc();

            return Map<TSource, TTarget>();
        }

        private TTarget Map<TSource, TTarget>()
        {
            IObjectMapper<TTarget> mapper;

            if ((typeof(TSource) == CurrentObjectMappingContext.SourceType) &&
                (typeof(TTarget) == CurrentObjectMappingContext.TargetType))
            {
                mapper = MapperContext.ObjectMapperFactory.CreateFor<TSource, TTarget>(CurrentObjectMappingContext);
            }
            else
            {
                var typedCreateMapperMethod = typeof(ObjectMapperFactory)
                    .GetMethod("CreateFor", Constants.PublicInstance)
                    .MakeGenericMethod(CurrentObjectMappingContext.Parameter.Type.GetGenericArguments());

                var mapperContext = Expression.Property(Parameters.ObjectMappingContext, "MapperContext");
                var mapperFactory = Expression.Property(mapperContext, "ObjectMapperFactory");

                var createMapperCall = Expression.Call(
                    mapperFactory,
                    typedCreateMapperMethod,
                    Parameters.ObjectMappingContext);

                var createMapperLambda = Expression
                    .Lambda<Func<IObjectMappingContext, IObjectMapper<TTarget>>>(
                        createMapperCall,
                        Parameters.ObjectMappingContext);

                var createMapperFunc = createMapperLambda.Compile();

                mapper = createMapperFunc.Invoke(CurrentObjectMappingContext);
            }

            var result = mapper.Execute(CurrentObjectMappingContext);

            CurrentObjectMappingContext = CurrentObjectMappingContext.Parent;

            return result;
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