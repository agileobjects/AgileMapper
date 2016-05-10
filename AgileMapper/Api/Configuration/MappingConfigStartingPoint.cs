namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;

    public class MappingConfigStartingPoint
    {
        private readonly MapperContext _mapperContext;

        internal MappingConfigStartingPoint(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public MappingConfigurator<object, TTarget> To<TTarget>()
            where TTarget : class
        {
            return GetAllSourcesTargetTypeSpecifier(ci => ci.ForAllRuleSets()).To<TTarget>();
        }

        public MappingConfigurator<object, TTarget> ToANew<TTarget>()
            where TTarget : class
        {
            return GetAllSourcesTargetTypeSpecifier(ci => ci.ForRuleSet(Constants.CreateNew)).ToANew<TTarget>();
        }

        public MappingConfigurator<object, TTarget> OnTo<TTarget>()
            where TTarget : class
        {
            return GetAllSourcesTargetTypeSpecifier(ci => ci.ForRuleSet(Constants.Merge)).To<TTarget>();
        }

        private TargetTypeSpecifier<object> GetAllSourcesTargetTypeSpecifier(
            Func<MappingConfigInfo, MappingConfigInfo> configInfoConfigurator)
        {
            return GetTargetTypeSpecifier<object>(ci =>
            {
                ci.ForAllSourceTypes();
                configInfoConfigurator.Invoke(ci);
                return ci;
            });
        }

        public TargetTypeSpecifier<TSource> From<TSource>()
        {
            return GetTargetTypeSpecifier<TSource>(ci => ci.ForSourceType<TSource>());
        }

        private TargetTypeSpecifier<TSource> GetTargetTypeSpecifier<TSource>(
            Func<MappingConfigInfo, MappingConfigInfo> configInfoConfigurator)
        {
            var configInfo = configInfoConfigurator.Invoke(new MappingConfigInfo(_mapperContext));

            return new TargetTypeSpecifier<TSource>(configInfo);
        }
    }
}