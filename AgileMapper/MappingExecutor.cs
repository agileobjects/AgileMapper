namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
#if FEATURE_DYNAMIC
    using System.Dynamic;
#endif
    using System.Linq.Expressions;
    using Api;
    using Api.Configuration;
    using Extensions;
    using Extensions.Internal;
    using ObjectPopulation;
    using ObjectPopulation.MapperKeys;
    using Plans;

    internal class MappingExecutor<TSource> :
        MappingExecutionContextBase2,
        ITargetSelector<TSource>,
        IFlatteningSelector<TSource>,
        IUnflatteningSelector<TSource>
    {
        private readonly TSource _source;
        private object _target;
        private MapperContext _mapperContext;
        private MappingRuleSet _ruleSet;
        private MappingTypes _mappingTypes;
        private ObjectMapperKeyBase _rootMapperKey;
        private Func<IObjectMappingData> _rootMappingDataFactory;
        private IObjectMappingData _rootMappingData;
        private IObjectMapper _rootMapper;

        public MappingExecutor(MapperContext mapperContext, TSource source)
            : base(source, parent: null)
        {
            _mapperContext = mapperContext.ThrowIfDisposed();
            _source = source;
        }

        public override MapperContext MapperContext => _mapperContext;

        public override MappingRuleSet RuleSet => _ruleSet;

        #region ToANew Overloads

        public object ToANew(Type resultType) => MappingExecutorBridge<TSource>.CreateNew(resultType, this);

        public TResult ToANew<TResult>() => PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult));

        public TResult ToANew<TResult>(Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>> configuration)
        {
            return (configuration != null)
                ? PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult), new[] { configuration })
                : PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult));
        }

        public TResult ToANew<TResult>(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>[] configurations)
        {
            return configurations.Any()
                ? PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult), configurations)
                : PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult));
        }

        #endregion

        #region OnTo Overloads

        public TTarget OnTo<TTarget>(TTarget existing) => PerformMapping(MapperContext.RuleSets.Merge, existing);

        public TTarget OnTo<TTarget>(TTarget existing, Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>> configuration)
        {
            return (configuration != null)
                ? PerformMapping(MapperContext.RuleSets.Merge, existing, new[] { configuration })
                : PerformMapping(MapperContext.RuleSets.Merge, existing);
        }

        public TTarget OnTo<TTarget>(
            TTarget existing,
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            return configurations.Any()
                ? PerformMapping(MapperContext.RuleSets.Merge, existing, configurations)
                : PerformMapping(MapperContext.RuleSets.Merge, existing);
        }

        #endregion

        #region Over Overloads

        public TTarget Over<TTarget>(TTarget existing) => PerformMapping(MapperContext.RuleSets.Overwrite, existing);

        public TTarget Over<TTarget>(TTarget existing, Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>> configuration)
        {
            return (configuration != null)
                ? PerformMapping(MapperContext.RuleSets.Overwrite, existing, new[] { configuration })
                : PerformMapping(MapperContext.RuleSets.Overwrite, existing);
        }

        public TTarget Over<TTarget>(
            TTarget existing,
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            return configurations.Any()
                ? PerformMapping(MapperContext.RuleSets.Overwrite, existing, configurations)
                : PerformMapping(MapperContext.RuleSets.Overwrite, existing);
        }

        #endregion

        private TTarget PerformMapping<TTarget>(MappingRuleSet ruleSet, TTarget target)
        {
            if (_source == null)
            {
                return target;
            }

            _ruleSet = ruleSet;

            return PerformMapping(target);
        }

        private TTarget PerformMapping<TTarget>(
            MappingRuleSet ruleSet,
            TTarget target,
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            if (_source == null)
            {
                return target;
            }

            _ruleSet = ruleSet;
            _mapperContext = MapperContext.InlineContexts.GetContextFor(configurations, this);

            return PerformMapping(target);
        }

        private TTarget PerformMapping<TTarget>(TTarget target)
        {
            if (target != null)
            {
                _target = target;
            }

            if (MappingTypes<TSource, TTarget>.SkipTypesCheck)
            {
                // Optimise for the most common scenario:
                _mappingTypes = MappingTypes<TSource, TTarget>.Fixed;
                _rootMappingDataFactory = CreateFixedTypeRootMappingData<TTarget>;

                var typedRootMapper = (ObjectMapper<TSource, TTarget>)GetOrCreateRootMapper<TTarget>();
                _rootMapper = typedRootMapper;

                return typedRootMapper.Map(_source, target, this);
            }

            _mappingTypes = GetRuntimeMappingTypes(target);

            var untypedMapper = GetOrCreateRootMapper<TTarget>();
            _rootMapper = untypedMapper;

            return (TTarget)untypedMapper.Map(this);
            //var rootMappingData = ObjectMappingDataFactory.ForRoot(_source, target, this);
            //var result = rootMappingData.MapStart();

            //return (TTarget)default(object);

            //var mapper = MapperContext.ObjectMapperFactory
            //  .GetOrCreateRoot<TSource, TTarget>(this);

            //_rootMapper = mapper;
            //return (TTarget)mapper.Map(this);
        }

        #region IFlatteningSelector Members

#if FEATURE_DYNAMIC
        dynamic IFlatteningSelector<TSource>.ToDynamic(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, ExpandoObject>>>[] configurations)
        {
            return configurations.Any() ? ToANew(configurations) : ToANew<ExpandoObject>();
        }
#endif
        Dictionary<string, object> IFlatteningSelector<TSource>.ToDictionary(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, Dictionary<string, object>>>>[] configurations)
        {
            return configurations.Any() ? ToANew(configurations) : ToANew<Dictionary<string, object>>();
        }

        Dictionary<string, TValue> IFlatteningSelector<TSource>.ToDictionary<TValue>(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, Dictionary<string, TValue>>>>[] configurations)
        {
            return configurations.Any() ? ToANew(configurations) : ToANew<Dictionary<string, TValue>>();
        }

        string IFlatteningSelector<TSource>.ToQueryString(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, Dictionary<string, string>>>>[] configurations)
        {
            var flattened = configurations.Any() ? ToANew(configurations) : ToANew<Dictionary<string, string>>();

            var queryString = flattened
                .Project(kvp => Uri.EscapeDataString(kvp.Key) + "=" + Uri.EscapeDataString(kvp.Value))
                .Join("&");
#if NET35
            queryString = queryString.Replace("!", "%21");
#endif
            return queryString.Replace(".", "%2E");
        }

        #endregion

        #region IUnflatteningSelector Members

        object IUnflatteningSelector<TSource>.To(Type resultType) => ToANew(resultType);

        TResult IUnflatteningSelector<TSource>.To<TResult>(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>[] configurations)
        {
            return configurations.Any() ? ToANew(configurations) : ToANew<TResult>();
        }

        #endregion

        #region IMappingContext Members

        public override MappingPlanSettings PlanSettings => MappingPlanSettings.Default.LazyPlanned;

        #endregion

        #region MappingExecutionContextBase Members

        public override MappingTypes MappingTypes => _mappingTypes;

        public override ObjectMapperKeyBase GetMapperKey()
            => _rootMapperKey ??= RuleSet.RootMapperKeyFactory.Invoke(this);

        public override object Target => _target;

        protected override void Set(object target) => _target = target;

        public override IObjectMappingData GetMappingData()
            => _rootMappingData ??= _rootMappingDataFactory.Invoke();

        public override IObjectMapper GetRootMapper() => _rootMapper;

        private MappingTypes GetRuntimeMappingTypes<TTarget>(TTarget target)
        {
#if FEATURE_DYNAMIC
            if ((target == null) && (typeof(TTarget) == typeof(object)))
            {
                // This is a 'create new' mapping where the target type has come 
                // through as 'object'. This happens when you use .ToANew<dynamic>(),
                // and I can't see how to differentiate that from .ToANew<object>().
                // Given that the former is more likely and that people asking for 
                // .ToANew<object>() are doing something weird, default the target 
                // type to ExpandoObject:
                _rootMappingDataFactory = CreateRuntimeTypedRootMappingData<ExpandoObject>;
                return MappingTypes.For(_source, default(ExpandoObject));
            }
#endif
            _rootMappingDataFactory = CreateRuntimeTypedRootMappingData<TTarget>;
            return MappingTypes.For(_source, target);
        }

        private IObjectMappingData CreateFixedTypeRootMappingData<TTarget>()
        {
            return ObjectMappingDataFactory
                .ForRootFixedTypes(_source, (TTarget)_target, _mappingTypes, this, createMapper: false);
        }

        private IObjectMappingData CreateRuntimeTypedRootMappingData<TTarget>()
            => ObjectMappingDataFactory.ForRoot(_source, (TTarget)_target, _mappingTypes, this);

        private IObjectMapper GetOrCreateRootMapper<TTarget>()
        {
            return MapperContext.ObjectMapperFactory
                .GetOrCreateRoot<TSource, TTarget>(this);
        }

        #endregion
    }
}