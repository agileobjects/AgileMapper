namespace AgileObjects.AgileMapper.Configuration.Inline
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Api.Configuration;

    internal static class InlineMappingConfigurator<TSource, TTarget>
    {
        public static MapperContext ConfigureInlineMapperContext(
            IEnumerable<Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>> configurations,
            IMappingContext mappingContext)
        {
            var inlineMapperContext = mappingContext.MapperContext.Clone();

            return ConfigureMapperContext(configurations, mappingContext.RuleSet, inlineMapperContext);
        }

        public static MapperContext ConfigureMapperContext(
            IEnumerable<Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>> configurations,
            IMappingContext mappingContext)
        {
            return ConfigureMapperContext(configurations, mappingContext.RuleSet, mappingContext.MapperContext);
        }

        private static MapperContext ConfigureMapperContext(
            IEnumerable<Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>> configurations,
            MappingRuleSet ruleSet,
            MapperContext mapperContext)
        {
            var configInfo = new MappingConfigInfo(mapperContext)
                .ForRuleSet(ruleSet)
                .ForSourceType<TSource>()
                .ForTargetType<TTarget>();

            var configurator = new MappingConfigurator<TSource, TTarget>(configInfo);

            foreach (var configuration in configurations)
            {
                configuration.Compile().Invoke(configurator);
            }

            return mapperContext;
        }
    }
}