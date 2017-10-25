﻿namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq.Expressions;
    using Api;
    using Api.Configuration;
    using Extensions;
    using Members;
    using ObjectPopulation;

    internal class MappingExecutor<TSource> : ITargetTypeSelector<TSource>, IMappingContext
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

        public MappingExecutor(TSource source, MappingRuleSet ruleSet, MapperContext mapperContext)
            : this(source, mapperContext)
        {
            RuleSet = ruleSet;
        }

        public MapperContext MapperContext { get; private set; }

        public MappingRuleSet RuleSet { get; private set; }

        #region Inline Configuration

        public TResult ToANew<TResult>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>[] configurations)
        {
            return PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult), configurations);
        }

        public TTarget OnTo<TTarget>(
            TTarget existing,
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            return PerformMapping(MapperContext.RuleSets.Merge, existing, configurations);
        }

        public TTarget Over<TTarget>(
            TTarget existing,
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations)
        {
            return PerformMapping(MapperContext.RuleSets.Overwrite, existing, configurations);
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
            MapperContext = MapperContext.InlineMappers.GetContextFor(configurations, this);

            return PerformMapping(target);
        }

        #endregion

        public TResult ToANew<TResult>()
            => PerformMapping(MapperContext.RuleSets.CreateNew, default(TResult));

        public TTarget OnTo<TTarget>(TTarget existing)
            => PerformMapping(MapperContext.RuleSets.Merge, existing);

        public TTarget Over<TTarget>(TTarget existing)
            => PerformMapping(MapperContext.RuleSets.Overwrite, existing);

        private TTarget PerformMapping<TTarget>(MappingRuleSet ruleSet, TTarget target)
        {
            if (_source == null)
            {
                return target;
            }

            RuleSet = ruleSet;

            return PerformMapping(target);
        }

        private TTarget PerformMapping<TTarget>(TTarget target)
        {
            if (SkipTypeChecks<TTarget>())
            {
                // Optimise for the most common scenario:
                var typedRootMappingData = CreateTypedRootMappingData(target);

                return typedRootMappingData.MapStart();
            }

            var rootMappingData = CreateRootMappingData(target);
            var result = rootMappingData.MapStart();

            return (TTarget)result;
        }

        private static bool SkipTypeChecks<TTarget>()
            => !(TypeInfo<TSource>.RuntimeTypeNeeded || TypeInfo<TTarget>.RuntimeTypeNeeded);

        private ObjectMappingData<TSource, TTarget> CreateTypedRootMappingData<TTarget>(TTarget target)
        {
            return new ObjectMappingData<TSource, TTarget>(
                _source,
                target,
                null, // <- No enumerable index because we're at the root
                new RootObjectMapperKey(MappingTypes<TSource, TTarget>.Fixed, this),
                this,
                parent: null);
        }

        private IObjectMappingData CreateRootMappingData<TTarget>(TTarget target)
            => CreateRootMappingData(_source, target);

        public IObjectMappingData CreateRootMappingData<TDataSource, TDataTarget>(TDataSource source, TDataTarget target)
            => ObjectMappingDataFactory.ForRoot(source, target, this);
    }
}