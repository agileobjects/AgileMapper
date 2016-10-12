namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using Api;
    using ObjectPopulation;

    internal class MappingExecutor<TSource> : ITargetTypeSelector, IMappingContext
    {
        private readonly TSource _source;

        public MappingExecutor(TSource source, MapperContext mapperContext)
        {
            _source = source;
            MapperContext = mapperContext;
        }

        public MappingExecutor(MappingRuleSet ruleSet, MapperContext mapperContext)
        {
            RuleSet = ruleSet;
            MapperContext = mapperContext;
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

            var rootMapperCreationData = CreateRootMappingContextData(existing);

            return Map<TSource, TTarget>(rootMapperCreationData);
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

            var cacheKey = DeclaredAndRuntimeTypesKey.ForCreateMapperCall(data);

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
    }
}