namespace AgileObjects.AgileMapper.Configuration.Inline
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Api.Configuration.Dictionaries;

    internal class InlineConfigurationSet<TSource, TTarget, TConfigurator> : IInlineConfigurationSet
    {
        private readonly Expression<Action<TConfigurator>>[] _configurations;
        private readonly Func<MappingConfigInfo, TConfigurator> _configuratorFactory;

        public InlineConfigurationSet(
            Expression<Action<TConfigurator>>[] configurations,
            Func<MappingConfigInfo, TConfigurator> configuratorFactory)
        {
            _configurations = configurations;
            _configuratorFactory = configuratorFactory;
        }

        public int Count => Lambdas.Count;

        // ReSharper disable once CoVariantArrayConversion
        public IList<LambdaExpression> Lambdas => _configurations;

        public void Apply(IMappingContext mappingContext)
            => Apply(mappingContext.RuleSet, mappingContext.MapperContext);

        public void Apply(MappingRuleSet ruleSet, MapperContext mapperContext)
        {
            var configInfo = new MappingConfigInfo(mapperContext)
                .ForRuleSet(ruleSet)
                .ForSourceType<TSource>()
                .ForTargetType<TTarget>();

            var configurator = _configuratorFactory.Invoke(configInfo);

            foreach (var configuration in _configurations)
            {
                configuration.Compile().Invoke(configurator);
            }
        }
    }

    #region Factory Class

    internal static class InlineConfigurationSet<TSource, TValue>
    {
        public static InlineConfigurationSet<TSource, TValue, IFullMappingInlineConfigurator<TSource, TValue>> Full(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TValue>>>[] configurations)
        {
            return new InlineConfigurationSet<TSource, TValue, IFullMappingInlineConfigurator<TSource, TValue>>(
                configurations,
                configInfo => new MappingConfigurator<TSource, TValue>(configInfo));
        }

        public static InlineConfigurationSet<TSource, TValue, ITargetDictionaryMappingInlineConfigurator<TSource, TValue>> Dictionary(
            Expression<Action<ITargetDictionaryMappingInlineConfigurator<TSource, TValue>>>[] configurations)
        {
            return new InlineConfigurationSet<TSource, TValue, ITargetDictionaryMappingInlineConfigurator<TSource, TValue>>(
                configurations,
                configInfo => new TargetDictionaryMappingConfigurator<TSource, TValue>(configInfo));
        }
    }

    #endregion
}