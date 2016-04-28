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

        public MappingConfigurator<TTarget, TTarget> To<TTarget>()
            where TTarget : class
        {
            return GetTargetTypeSpecifier<TTarget>(ci => ci.ForAllSourceTypes().ForAllRuleSets()).To<TTarget>();
        }

        public MappingConfigurator<TTarget, TTarget> OnTo<TTarget>()
            where TTarget : class
        {
            return GetTargetTypeSpecifier<TTarget>(ci => ci.ForAllSourceTypes().ForRuleSet(Constants.Merge)).To<TTarget>();
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