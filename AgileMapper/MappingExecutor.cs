namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
#if FEATURE_DYNAMIC
    using System.Dynamic;
#endif
    using System.Linq;
    using System.Linq.Expressions;
    using Api;
    using Api.Configuration;
    using Extensions;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ObjectPopulation.MapperKeys;
    using Plans;

    internal class MappingExecutor<TSource> :
        ITargetSelector<TSource>,
        IFlatteningSelector<TSource>,
        IUnflatteningSelector<TSource>,
        IEntryPointMappingContext,
        IMappingExecutionContext
    {
        private readonly TSource _source;
        private object _target;
        private MappingTypes _mappingTypes;
        private IRootMapperKey _rootMapperKey;
        private Func<IObjectMappingData> _rootMappingDataFactory;
        private Dictionary<object, List<object>> _mappedObjectsBySource;

        public MappingExecutor(MapperContext mapperContext, TSource source)
        {
            MapperContext = mapperContext.ThrowIfDisposed();
            _source = source;
        }

        public MapperContext MapperContext { get; private set; }

        public MappingRuleSet RuleSet { get; private set; }

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

            RuleSet = ruleSet;

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

            RuleSet = ruleSet;
            MapperContext = MapperContext.InlineContexts.GetContextFor(configurations, this);

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
            }
            else
            {
                _mappingTypes = GetRuntimeMappingTypes(target);
            }

            //var rootMappingData = ObjectMappingDataFactory.ForRoot(_source, target, this);
            //var result = rootMappingData.MapStart();

            //return (TTarget)default(object);

            var mapper = MapperContext.ObjectMapperFactory
              .GetOrCreateRoot<TSource, TTarget>(this);

            return mapper.Map(_source, target, this);
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

        MappingPlanSettings IMappingContext.PlanSettings => MappingPlanSettings.Default.LazyPlanned;

        #endregion

        #region IEntryPointMappingContext Members

        MappingTypes IEntryPointMappingContext.MappingTypes => _mappingTypes;

        IRootMapperKey IEntryPointMappingContext.GetRootMapperKey()
            => _rootMapperKey ??= (IRootMapperKey)RuleSet.RootMapperKeyFactory.Invoke(this);

        IObjectMappingData IEntryPointMappingContext.ToMappingData()
            => _rootMappingDataFactory.Invoke();

        T IEntryPointMappingContext.GetSource<T>()
        {
            if (typeof(TSource).IsAssignableTo(typeof(T)))
            {
                return (T)(object)_source;
            }

            return default;
        }

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

        #endregion

        #region IMappingExecutionContext Members

        private Dictionary<object, List<object>> MappedObjectsBySource
            => _mappedObjectsBySource ??= new Dictionary<object, List<object>>(13);

        bool IMappingExecutionContext.TryGet<TKey, TComplex>(
            TKey key,
            out TComplex complexType)
            where TComplex : class
        {
            if (MappedObjectsBySource.TryGetValue(key, out var mappedTargets))
            {
                complexType = mappedTargets.OfType<TComplex>().FirstOrDefault();
                return complexType != null;
            }

            complexType = default;
            return false;
        }

        void IMappingExecutionContext.Register<TKey, TComplex>(
            TKey key,
            TComplex complexType)
        {
            if (MappedObjectsBySource.TryGetValue(key, out var mappedTargets))
            {
                mappedTargets.Add(complexType);
                return;
            }

            _mappedObjectsBySource[key] = new List<object> { complexType };
        }

        #endregion
    }
}