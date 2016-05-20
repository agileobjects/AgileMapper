namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    public class MappingConfigStartingPoint
    {
        private readonly MapperContext _mapperContext;

        internal MappingConfigStartingPoint(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public void PassExceptionsTo(Action<IUntypedMemberMappingExceptionContext> callback)
        {
            var callbackFactory = new ExceptionCallbackFactory(
                new MappingConfigInfo(_mapperContext).ForAllSourceTypes().ForAllRuleSets(),
                typeof(object),
                Expression.Constant(callback));

            _mapperContext.UserConfigurations.Add(callbackFactory);
        }

        public TargetTypeSpecifier<TSource> From<TSource>(TSource exampleInstance) => From<TSource>();

        public TargetTypeSpecifier<TSource> From<TSource>()
            => GetTargetTypeSpecifier<TSource>(ci => ci.ForSourceType<TSource>());

        private TargetTypeSpecifier<TSource> GetTargetTypeSpecifier<TSource>(
            Func<MappingConfigInfo, MappingConfigInfo> configInfoConfigurator)
        {
            var configInfo = configInfoConfigurator.Invoke(new MappingConfigInfo(_mapperContext));

            return new TargetTypeSpecifier<TSource>(configInfo);
        }

        public MappingConfigurator<object, TTarget> To<TTarget>() where TTarget : class
            => GetAllSourcesTargetTypeSpecifier(ci => ci.ForAllRuleSets()).To<TTarget>();

        public MappingConfigurator<object, TTarget> ToANew<TTarget>() where TTarget : class
            => GetAllSourcesTargetTypeSpecifier(ci => ci.ForRuleSet(Constants.CreateNew)).ToANew<TTarget>();

        public MappingConfigurator<object, TTarget> OnTo<TTarget>() where TTarget : class
            => GetAllSourcesTargetTypeSpecifier(ci => ci.ForRuleSet(Constants.Merge)).OnTo<TTarget>();

        public MappingConfigurator<object, TTarget> Over<TTarget>() where TTarget : class
            => GetAllSourcesTargetTypeSpecifier(ci => ci.ForRuleSet(Constants.Overwrite)).Over<TTarget>();

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
    }
}