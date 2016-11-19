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

    /// <summary>
    /// Provides options for configuring how a mapper performs a mapping.
    /// </summary>
    public class MappingConfigStartingPoint
    {
        private readonly MapperContext _mapperContext;

        internal MappingConfigStartingPoint(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        #region Exception Handling

        /// <summary>
        /// Swallow exceptions thrown during a mapping irrespective of source and target type. Object mappings 
        /// which encounter an Exception will return null.
        /// </summary>
        public void SwallowAllExceptions() => PassExceptionsTo(ctx => { });

        /// <summary>
        /// Pass Exceptions thrown during a mapping to the given <paramref name="callback"/> instead of throwing 
        /// them, irrespective of source and target type.
        /// </summary>
        /// <param name="callback">
        /// The callback to which to pass thrown Exception information. If the thrown exception should not be 
        /// swallowed, it should be rethrown inside the callback.
        /// </param>
        public void PassExceptionsTo(Action<IMappingExceptionData> callback)
        {
            var exceptionCallback = new ExceptionCallback(
                MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(_mapperContext),
                Expression.Constant(callback));

            _mapperContext.UserConfigurations.Add(exceptionCallback);
        }

        #endregion

        #region Naming

        /// <summary>
        /// Expect members of any source and target type to potentially have the given name <paramref name="prefix"/>.
        /// Source and target members will be matched as if the prefix is absent.
        /// </summary>
        /// <param name="prefix">The prefix to ignore when matching source and target members.</param>
        public void UseNamePrefix(string prefix) => UseNamePrefixes(prefix);

        /// <summary>
        /// Expect members of any source and target type to potentially have any of the given name <paramref name="prefixes"/>.
        /// Source and target members will be matched as if the prefixes are absent.
        /// </summary>
        /// <param name="prefixes">The prefixes to ignore when matching source and target members.</param>
        public void UseNamePrefixes(params string[] prefixes)
            => UseNamePatterns(prefixes.Select(p => "^" + p + "(.+)$"));

        /// <summary>
        /// Expect members of any source and target type to potentially have the given name <paramref name="suffix"/>.
        /// Source and target members will be matched as if the suffix is absent.
        /// </summary>
        /// <param name="suffix">The suffix to ignore when matching source and target members.</param>
        public void UseNameSuffix(string suffix) => UseNameSuffixes(suffix);

        /// <summary>
        /// Expect members of any source and target type to potentially have any of the given name <paramref name="suffixes"/>.
        /// Source and target members will be matched as if the suffixes are absent.
        /// </summary>
        /// <param name="suffixes">The suffixes to ignore when matching source and target members.</param>
        public void UseNameSuffixes(params string[] suffixes)
            => UseNamePatterns(suffixes.Select(s => "^(.+)" + s + "$"));

        /// <summary>
        /// Expect members of any source and target type to potentially match the given name <paramref name="pattern"/>.
        /// The pattern will be used to find the part of a name which should be used to match a source and target member.
        /// </summary>
        /// <param name="pattern">
        /// The Regex pattern to check against source and target member names. The pattern is expected to start with the 
        /// ^ character, end with the $ character and contain a single capturing group wrapped in parentheses, e.g. ^__(.+)__$
        /// </param>
        public void UseNamePattern(string pattern) => UseNamePatterns(pattern);

        private static readonly Regex _patternChecker =
            new Regex(@"^\^(?<Prefix>[^(]+){0,1}\(\.\+\)(?<Suffix>[^$]+){0,1}\$$");

        /// <summary>
        /// Expect members of any source and target type to potentially match the given name <paramref name="patterns"/>.
        /// The patterns will be used to find the part of a name which should be used to match a source and target member.
        /// </summary>
        /// <param name="patterns">
        /// The Regex patterns to check against source and target member names. Each pattern is expected to start with the 
        /// ^ character, end with the $ character and contain a single capturing group wrapped in parentheses, e.g. ^__(.+)__$
        /// </param>
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

        #region Object Equality

        /// <summary>
        /// Configure this mapper to keep track of objects during a mapping in order to short-circuit 
        /// circular relationships and ensure a 1-to-1 relationship between source and mapped objects.
        /// </summary>
        public void TrackMappedObjects()
            => _mapperContext.UserConfigurations.Add(ObjectTrackingMode.TrackAll(_mapperContext));

        #endregion

        /// <summary>
        /// Configure how this mapper maps objects of the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TObject">The type of object to which the configuration will apply.</typeparam>
        /// <returns>An InstanceConfigurator with which to complete the configuration.</returns>
        public InstanceConfigurator<TObject> InstancesOf<TObject>() where TObject : class
            => new InstanceConfigurator<TObject>(_mapperContext);

        /// <summary>
        /// Configure how this mapper performs mappings from the source type specified by the given 
        /// <paramref name="exampleInstance"/>. Use this overload for anonymous types.
        /// </summary>
        /// <typeparam name="TSource">The type of the given <paramref name="exampleInstance"/>.</typeparam>
        /// <param name="exampleInstance">The instance specifying to which source type the configuration will apply.</param>
        /// <returns>A TargetTypeSpecifier with which to specify to which target type the configuration will apply.</returns>
        public TargetTypeSpecifier<TSource> From<TSource>(TSource exampleInstance) => From<TSource>();

        /// <summary>
        /// Configure how this mapper performs mappings from the source type specified by the type argument.
        /// </summary>
        /// <typeparam name="TSource">The source type to which the configuration will apply.</typeparam>
        /// <returns>A TargetTypeSpecifier with which to specify to which target type the configuration will apply.</returns>
        public TargetTypeSpecifier<TSource> From<TSource>()
            => GetTargetTypeSpecifier<TSource>(ci => ci.ForSourceType<TSource>());

        private TargetTypeSpecifier<TSource> GetTargetTypeSpecifier<TSource>(
            Func<MappingConfigInfo, MappingConfigInfo> configInfoConfigurator)
        {
            var configInfo = configInfoConfigurator.Invoke(new MappingConfigInfo(_mapperContext));

            return new TargetTypeSpecifier<TSource>(configInfo);
        }

        /// <summary>
        /// Configure how this mapper performs mappings from any source type to the target type specified by 
        /// the type argument, irrespective of the MappingRuleSet used.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<object, TTarget> To<TTarget>() where TTarget : class
            => GetAllSourcesTargetTypeSpecifier(ci => ci.ForAllRuleSets()).To<TTarget>();

        /// <summary>
        /// Configure how this mapper performs mappings from any source type to the target type specified by 
        /// the type argument when mapping to new objects.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<object, TTarget> ToANew<TTarget>() where TTarget : class
            => GetAllSourcesTargetTypeSpecifier(ci => ci.ForRuleSet(Constants.CreateNew)).ToANew<TTarget>();

        /// <summary>
        /// Configure how this mapper performs mappings from any source type to the target type specified by 
        /// the type argument when performing OnTo (merge) mappings.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<object, TTarget> OnTo<TTarget>() where TTarget : class
            => GetAllSourcesTargetTypeSpecifier(ci => ci.ForRuleSet(Constants.Merge)).OnTo<TTarget>();

        /// <summary>
        /// Configure how this mapper performs mappings from any source type to the target type specified by 
        /// the type argument when performing Over (overwrite) mappings.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
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