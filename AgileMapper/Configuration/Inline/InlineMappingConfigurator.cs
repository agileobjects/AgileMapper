namespace AgileObjects.AgileMapper.Configuration.Inline
{
    using System;
    using System.Collections.Generic;
    using Api.Configuration;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class InlineMappingConfigurator<TSource, TTarget>
    {
        public static MapperContext ConfigureInlineMapperContext<TConfigurator>(
            IEnumerable<Expression<Action<TConfigurator>>> configurations,
            Func<MappingConfigInfo, TConfigurator> configuratorFactory,
            IMappingContext mappingContext)
        {
            var inlineMapperContext = mappingContext.MapperContext.Clone();

            return ConfigureMapperContext(
                configurations,
                configuratorFactory,
                mappingContext.RuleSet,
                inlineMapperContext);
        }

        public static MapperContext ConfigureMapperContext(
            IEnumerable<Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>> configurations,
            IMappingContext mappingContext)
        {
            return ConfigureMapperContext(
                configurations,
                configInfo => new MappingConfigurator<TSource, TTarget>(configInfo),
                mappingContext.RuleSet,
                mappingContext.MapperContext);
        }

        private static MapperContext ConfigureMapperContext<TConfigurator>(
            IEnumerable<Expression<Action<TConfigurator>>> configurations,
            Func<MappingConfigInfo, TConfigurator> configuratorFactory,
            MappingRuleSet ruleSet,
            MapperContext mapperContext)
        {
            var configInfo = new MappingConfigInfo(mapperContext)
                .ForRuleSet(ruleSet)
                .ForSourceType<TSource>()
                .ForTargetType<TTarget>();

            var configurator = configuratorFactory.Invoke(configInfo);

            foreach (var configuration in configurations)
            {
                configuration.Compile().Invoke(configurator);
            }

            return mapperContext;
        }
    }
}