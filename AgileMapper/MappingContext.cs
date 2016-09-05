using AgileObjects.AgileMapper.Caching;

namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using ObjectPopulation;

    internal class MappingContext : IDisposable
    {
        internal static readonly MethodInfo TryGetOrRegisterMethod = typeof(MappingContext)
            .GetMethod("TryGetOrRegister", Constants.PublicInstance);

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

        public bool TryGetOrRegister<TKey, TComplex>(TKey key, out TComplex complexType, Func<TComplex> complexTypeFactory)
        {
            if (key == null)
            {
                complexType = complexTypeFactory.Invoke();
                return false;
            }

            var objectAlreadyRegistered = true;

            complexType = ObjectCache<TKey, TComplex>.Cache.GetOrAdd(key, k =>
            {
                objectAlreadyRegistered = false;

                _cleanupActions.Add(() => ObjectCache<TKey, TComplex>.Cache.Remove(k));

                return complexTypeFactory.Invoke();
            });

            return objectAlreadyRegistered;
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
                    .GetMethod("CreateFor", Constants.PublicInstance)
                    .MakeGenericMethod(data.MapperData.SourceType, data.MapperData.TargetType);

                var createMapperCall = Expression.Call(
                    mapperFactoryParameter,
                    typedCreateMapperMethod,
                    Parameters.ObjectMappingCreationData);

                var createMapperLambda = Expression
                    .Lambda<Func<ObjectMapperFactory, IObjectMapperCreationData, IObjectMapper<TTarget>>>(
                        createMapperCall,
                        mapperFactoryParameter,
                        Parameters.ObjectMappingCreationData);

                return createMapperLambda.Compile();
            });

            mapper = createMapperFunc.Invoke(MapperContext.ObjectMapperFactory, data);

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
            public static readonly ICache<TKey, TObject> Cache = GlobalContext.Instance.Cache.CreateNew<TKey, TObject>();
        }
    }
}