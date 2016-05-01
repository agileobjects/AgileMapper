namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class MappingContext : IDisposable
    {
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

        internal IObjectMappingContext RootObjectMappingContext { get; private set; }

        internal IObjectMappingContext CurrentObjectMappingContext { get; private set; }

        internal TDeclaredTarget MapStart<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget existing)
        {
            if (source == null)
            {
                return existing;
            }

            CurrentObjectMappingContext =
                RootObjectMappingContext =
                    ObjectMappingContextFactory.CreateRoot(source, existing, this);

            return Map<TDeclaredSource, TDeclaredTarget>();
        }

        public void Register<TKey, TComplex>(TKey key, TComplex complexType)
        {
            ObjectCache<TKey, TComplex>.Cache.Add(key, complexType);

            _cleanupActions.Add(() => ObjectCache<TKey, TComplex>.Cache.Remove(key));
        }

        public bool TryGet<TKey, TComplex>(TKey key, out TComplex complexType)
        {
            return ObjectCache<TKey, TComplex>.Cache.TryGetValue(key, out complexType);
        }

        internal TDeclaredMember MapChild<TRuntimeSource, TRuntimeTarget, TDeclaredMember>(
            TRuntimeSource source,
            TRuntimeTarget existing,
            Expression<Func<TRuntimeTarget, TDeclaredMember>> childMemberExpression)
        {
            CurrentObjectMappingContext = ObjectMappingContextFactory.Create(
                source,
                existing,
                childMemberExpression,
                this);

            return Map<TRuntimeSource, TDeclaredMember>();
        }

        public TDeclaredTarget MapEnumerableElement<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceElement,
            TDeclaredTarget existingElement,
            int enumerableIndex)
        {
            if (sourceElement == null)
            {
                return existingElement;
            }

            CurrentObjectMappingContext = ObjectMappingContextFactory.Create(
                sourceElement,
                existingElement,
                enumerableIndex,
                this);

            return Map<TDeclaredSource, TDeclaredTarget>();
        }

        private TTarget Map<TSource, TTarget>()
        {
            IObjectMapper<TTarget> mapper;

            if (typeof(ObjectMappingContext<TSource, TTarget>).IsAssignableFrom(CurrentObjectMappingContext.Type))
            {
                mapper = MapperContext.ObjectMapperFactory.CreateFor<TSource, TTarget>(CurrentObjectMappingContext);
            }
            else
            {
                var typedCreateMapperMethod = typeof(ObjectMapperFactory)
                    .GetMethod("CreateFor", Constants.PublicInstance)
                    .MakeGenericMethod(
                        CurrentObjectMappingContext.SourceObject.Type,
                        CurrentObjectMappingContext.ExistingObject.Type);

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