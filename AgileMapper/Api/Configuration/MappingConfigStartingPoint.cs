namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;

    public class MappingConfigStartingPoint
    {
        private readonly MapperContext _mapperContext;

        internal MappingConfigStartingPoint(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        #region Exception Handling

        public void SwallowAllExceptions() => PassExceptionsTo(ctx => { });

        public void PassExceptionsTo(Action<IUntypedMemberMappingExceptionContext> callback)
        {
            var exceptionCallback = new ExceptionCallback(
                MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(_mapperContext),
                Expression.Constant(callback));

            _mapperContext.UserConfigurations.Add(exceptionCallback);
        }

        #endregion

        #region Naming

        public void ExpectNamePrefix(string prefix) => ExpectNamePrefixes(prefix);

        public void ExpectNamePrefixes(params string[] prefixes)
            => ExpectNamePatterns(prefixes.Select(p => "^" + p + "(.+)$"));

        public void ExpectNameSuffix(string suffix) => ExpectNameSuffixes(suffix);

        public void ExpectNameSuffixes(params string[] suffixes)
            => ExpectNamePatterns(suffixes.Select(s => "^(.+)" + s + "$"));

        public void ExpectNamePattern(string pattern) => ExpectNamePatterns(pattern);

        public void ExpectNamePatterns(params string[] patterns)
            => ExpectNamePatterns(patterns.AsEnumerable());

        private void ExpectNamePatterns(IEnumerable<string> patterns)
            => _mapperContext.NamingSettings.AddNameMatchers(patterns);

        #endregion

        public InstanceConfigurator<TObject> InstancesOf<TObject>() where TObject : class
            => new InstanceConfigurator<TObject>(_mapperContext);

        public TargetTypeSpecifier<TSource> From<TSource>(TSource exampleInstance) => From<TSource>();

        public TargetTypeSpecifier<TSource> From<TSource>()
            => GetTargetTypeSpecifier<TSource>(ci => ci.ForSourceType<TSource>());

        private TargetTypeSpecifier<TSource> GetTargetTypeSpecifier<TSource>(
            Func<MappingConfigInfo, MappingConfigInfo> configInfoConfigurator)
        {
            var configInfo = configInfoConfigurator.Invoke(new MappingConfigInfo(_mapperContext));

            return new TargetTypeSpecifier<TSource>(configInfo);
        }

        public IFullMappingConfigurator<object, TTarget> To<TTarget>() where TTarget : class
            => GetAllSourcesTargetTypeSpecifier(ci => ci.ForAllRuleSets()).To<TTarget>();

        public IFullMappingConfigurator<object, TTarget> ToANew<TTarget>() where TTarget : class
            => GetAllSourcesTargetTypeSpecifier(ci => ci.ForRuleSet(Constants.CreateNew)).ToANew<TTarget>();

        public IFullMappingConfigurator<object, TTarget> OnTo<TTarget>() where TTarget : class
            => GetAllSourcesTargetTypeSpecifier(ci => ci.ForRuleSet(Constants.Merge)).OnTo<TTarget>();

        public IFullMappingConfigurator<object, TTarget> Over<TTarget>() where TTarget : class
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