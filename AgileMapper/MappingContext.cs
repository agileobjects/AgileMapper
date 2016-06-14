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

        internal TDeclaredTarget MapStart<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget existing)
        {
            if (source == null)
            {
                return existing;
            }

            CreateRootObjectContext(source, existing);

            return Map<TDeclaredSource, TDeclaredTarget, TDeclaredTarget>();
        }

        internal IObjectMappingContext CreateRootObjectContext<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget existing)
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

        internal TDeclaredMember MapChild<TRuntimeSource, TRuntimeTarget, TDeclaredMember>(
            ObjectMappingCommand<TRuntimeSource, TRuntimeTarget, TDeclaredMember> command)
        {
            CurrentObjectMappingContext = command.ToOmc();

            return Map<TRuntimeSource, TRuntimeTarget, TDeclaredMember>();
        }

        private TInstance Map<TSource, TTarget, TInstance>()
        {
            IObjectMapper<TInstance> mapper;

            if (typeof(ObjectMappingContext<TSource, TTarget, TInstance>).IsAssignableFrom(CurrentObjectMappingContext.Parameter.Type))
            {
                mapper = MapperContext.ObjectMapperFactory.CreateFor<TSource, TTarget, TInstance>(CurrentObjectMappingContext);
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
                    .Lambda<Func<IObjectMappingContext, IObjectMapper<TInstance>>>(
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