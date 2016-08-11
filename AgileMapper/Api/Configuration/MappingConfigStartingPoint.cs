namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;
    using AgileMapper.Configuration;
    using Extensions;
    using Members;
    using ReadableExpressions;

    public class MappingConfigStartingPoint
    {
        private readonly MapperContext _mapperContext;

        internal MappingConfigStartingPoint(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        #region Exception Handling

        public void SwallowAllExceptions() => PassExceptionsTo(ctx => { });

        public void PassExceptionsTo(Action<IMappingExceptionData> callback)
        {
            var exceptionCallback = new ExceptionCallback(
                MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(_mapperContext),
                Expression.Constant(callback));

            _mapperContext.UserConfigurations.Add(exceptionCallback);
        }

        #endregion

        #region Naming

        public void UseNamePrefix(string prefix) => UseNamePrefixes(prefix);

        public void UseNamePrefixes(params string[] prefixes)
            => UseNamePatterns(prefixes.Select(p => "^" + p + "(.+)$"));

        public void UseNameSuffix(string suffix) => UseNameSuffixes(suffix);

        public void UseNameSuffixes(params string[] suffixes)
            => UseNamePatterns(suffixes.Select(s => "^(.+)" + s + "$"));

        public void UseNamePattern(string pattern) => UseNamePatterns(pattern);

        private static readonly Regex _patternChecker =
            new Regex(@"^\^(?<Prefix>[^(]+){0,1}\(\.\+\)(?<Suffix>[^$]+){0,1}\$$");

        public void UseNamePatterns(params string[] patterns)
        {
            if (patterns.None())
            {
                throw new ArgumentException("No naming patterns supplied", nameof(patterns));
            }

            for (var i = 0; i < patterns.Length; i++)
            {
                var pattern = patterns[i];

                if (pattern == null)
                {
                    throw new ArgumentNullException(nameof(patterns), "Naming patterns cannot be null");
                }

                if (pattern.Contains(Environment.NewLine))
                {
                    throw CreateConfigurationException(pattern);
                }

                if (!pattern.StartsWith('^'))
                {
                    patterns[i] = pattern = "^" + pattern;
                }

                if (!pattern.EndsWith('$'))
                {
                    patterns[i] = pattern = pattern + "$";
                }

                ThrowIfPatternIsInvalid(pattern);
            }

            UseNamePatterns(patterns.AsEnumerable());
        }

        private static void ThrowIfPatternIsInvalid(string pattern)
        {
            var match = _patternChecker.Match(pattern);

            if (!match.Success)
            {
                throw CreateConfigurationException(pattern);
            }

            var prefix = match.Groups["Prefix"].Value;
            var suffix = match.Groups["Suffix"].Value;

            if (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix))
            {
                throw CreateConfigurationException(pattern);
            }
        }

        private static Exception CreateConfigurationException(string pattern)
        {
            return new MappingConfigurationException(
                "Name pattern '" + pattern + "' is not valid. " +
                "Please specify a regular expression pattern in the format '^{prefix}(.+){suffix}$'");
        }

        private void UseNamePatterns(IEnumerable<string> patterns)
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