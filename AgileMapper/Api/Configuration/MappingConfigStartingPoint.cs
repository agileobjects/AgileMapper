namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using AgileMapper.Configuration;
    using Dictionaries;
    using Extensions;
    using Members;

    /// <summary>
    /// Provides options for configuring how a mapper performs a mapping.
    /// </summary>
    public class MappingConfigStartingPoint : IGlobalConfigSettings
    {
        private readonly MapperContext _mapperContext;

        internal MappingConfigStartingPoint(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        #region Global Settings

        #region Exception Handling

        /// <summary>
        /// Swallow exceptions thrown during a mapping, for all source and target types. Object mappings which 
        /// encounter an Exception will return null.
        /// </summary>
        /// <returns>
        /// An <see cref="IGlobalConfigSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        public IGlobalConfigSettings SwallowAllExceptions() => PassExceptionsTo(ctx => { });

        /// <summary>
        /// Pass Exceptions thrown during a mapping to the given <paramref name="callback"/> instead of throwing 
        /// them, for all source and target types.
        /// </summary>
        /// <param name="callback">
        /// The callback to which to pass thrown Exception information. If the thrown exception should not be 
        /// swallowed, it should be rethrown inside the callback.
        /// </param>
        /// <returns>
        /// An <see cref="IGlobalConfigSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        public IGlobalConfigSettings PassExceptionsTo(Action<IMappingExceptionData> callback)
        {
            var exceptionCallback = new ExceptionCallback(
                MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(_mapperContext),
                callback.ToConstantExpression());

            _mapperContext.UserConfigurations.Add(exceptionCallback);
            return this;
        }

        #endregion

        #region Naming

        /// <summary>
        /// Expect members of all source and target types to potentially have the given name <paramref name="prefix"/>.
        /// Source and target members will be matched as if the prefix is absent.
        /// </summary>
        /// <param name="prefix">The prefix to ignore when matching source and target members.</param>
        /// <returns>
        /// An <see cref="IGlobalConfigSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        public IGlobalConfigSettings UseNamePrefix(string prefix) => UseNamePrefixes(prefix);

        /// <summary>
        /// Expect members of all source and target types to potentially have any of the given name <paramref name="prefixes"/>.
        /// Source and target members will be matched as if the prefixes are absent.
        /// </summary>
        /// <param name="prefixes">The prefixes to ignore when matching source and target members.</param>
        /// <returns>
        /// An <see cref="IGlobalConfigSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        public IGlobalConfigSettings UseNamePrefixes(params string[] prefixes)
            => UseNamePatterns(prefixes.Select(p => "^" + p + "(.+)$"));

        /// <summary>
        /// Expect members of all source and target types to potentially have the given name <paramref name="suffix"/>.
        /// Source and target members will be matched as if the suffix is absent.
        /// </summary>
        /// <param name="suffix">The suffix to ignore when matching source and target members.</param>
        /// <returns>
        /// An <see cref="IGlobalConfigSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        public IGlobalConfigSettings UseNameSuffix(string suffix) => UseNameSuffixes(suffix);

        /// <summary>
        /// Expect members of all source and target types to potentially have any of the given name <paramref name="suffixes"/>.
        /// Source and target members will be matched as if the suffixes are absent.
        /// </summary>
        /// <param name="suffixes">The suffixes to ignore when matching source and target members.</param>
        /// <returns>
        /// An <see cref="IGlobalConfigSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        public IGlobalConfigSettings UseNameSuffixes(params string[] suffixes)
            => UseNamePatterns(suffixes.Select(s => "^(.+)" + s + "$"));

        /// <summary>
        /// Expect members of all source and target types to potentially match the given name <paramref name="pattern"/>.
        /// The pattern will be used to find the part of a name which should be used to match a source and target member.
        /// </summary>
        /// <param name="pattern">
        /// The Regex pattern to check against source and target member names. The pattern is expected to start with the 
        /// ^ character, end with the $ character and contain a single capturing group wrapped in parentheses, e.g. ^__(.+)__$
        /// </param>
        /// <returns>
        /// An <see cref="IGlobalConfigSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        public IGlobalConfigSettings UseNamePattern(string pattern) => UseNamePatterns(pattern);

        private static readonly Regex _patternChecker =
            new Regex(@"^\^(?<Prefix>[^(]+){0,1}\(\.\+\)(?<Suffix>[^$]+){0,1}\$$");

        /// <summary>
        /// Expect members of all source and target types to potentially match the given name <paramref name="patterns"/>.
        /// The patterns will be used to find the part of a name which should be used to match a source and target member.
        /// </summary>
        /// <param name="patterns">
        /// The Regex patterns to check against source and target member names. Each pattern is expected to start with the 
        /// ^ character, end with the $ character and contain a single capturing group wrapped in parentheses, e.g. ^__(.+)__$
        /// </param>
        /// <returns>
        /// An <see cref="IGlobalConfigSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        public IGlobalConfigSettings UseNamePatterns(params string[] patterns)
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

            return UseNamePatterns(patterns.AsEnumerable());
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

        private IGlobalConfigSettings UseNamePatterns(IEnumerable<string> patterns)
        {
            _mapperContext.NamingSettings.AddNameMatchers(patterns);
            return this;
        }

        #endregion

        /// <summary>
        /// Keep track of objects during mappings between all source and target types, in order to short-circuit 
        /// circular relationships and ensure a 1-to-1 relationship between source and mapped objects.
        /// </summary>
        public IGlobalConfigSettings TrackMappedObjects()
        {
            _mapperContext.UserConfigurations.Add(ObjectTrackingMode.TrackAll(_mapperContext));
            return this;
        }

        /// <summary>
        /// Map null source collections to null instead of an empty collection, for all source and target types.
        /// </summary>
        /// <returns>
        /// This <see cref="IGlobalConfigSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        public IGlobalConfigSettings MapNullCollectionsToNull()
        {
            _mapperContext.UserConfigurations.Add(NullCollectionsSetting.AlwaysMapToNull(_mapperContext));
            return this;
        }

        /// <summary>
        /// Configure this mapper to pair the given <paramref name="enumMember"/> with a member of another enum Type.
        /// This pairing will apply to mappings between all types and MappingRuleSets (create new, overwrite, etc).
        /// </summary>
        /// <typeparam name="TFirstEnum">The type of the first enum being paired.</typeparam>
        /// <param name="enumMember">The first enum member in the pair.</param>
        /// <returns>
        /// An EnumPairSpecifier with which to specify the enum member to which the given <paramref name="enumMember"/> 
        /// should be paired.
        /// </returns>
        public EnumPairSpecifier<TFirstEnum> PairEnum<TFirstEnum>(TFirstEnum enumMember) where TFirstEnum : struct
            => PairEnums(enumMember);

        /// <summary>
        /// Configure this mapper to pair the given <paramref name="enumMembers"/> with members of another enum Type.
        /// Pairings will apply to mappings between all types and MappingRuleSets (create new, overwrite, etc).
        /// </summary>
        /// <typeparam name="TFirstEnum">The type of the first set of enum members being paired.</typeparam>
        /// <param name="enumMembers">The first set of enum members to pair.</param>
        /// <returns>
        /// An EnumPairSpecifier with which to specify the set of enum members to which the given <paramref name="enumMembers"/> 
        /// should be paired.
        /// </returns>
        public EnumPairSpecifier<TFirstEnum> PairEnums<TFirstEnum>(params TFirstEnum[] enumMembers) where TFirstEnum : struct
            => EnumPairSpecifier<TFirstEnum>.For(_mapperContext, enumMembers);

        /// <summary>
        /// Scan the specified <paramref name="assemblies"/> when looking for types derived
        /// from a source or target type being mapped.
        /// </summary>
        /// <param name="assemblies">The assemblies in which to look for derived types.</param>
        /// <returns>
        /// This <see cref="IGlobalConfigSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        public IGlobalConfigSettings LookForDerivedTypesIn(params Assembly[] assemblies)
        {
            if (assemblies.None())
            {
                throw new MappingConfigurationException(
                    "One or more assemblies must be specified.",
                    new ArgumentException(nameof(assemblies)));
            }

            if (assemblies.Any(a => a == null))
            {
                throw new MappingConfigurationException(
                    "All supplied assemblies must be non-null.",
                    new ArgumentNullException(nameof(assemblies)));
            }

            _mapperContext.DerivedTypes.AddAssemblies(assemblies);
            return this;
        }

        #region Ignoring Members

        /// <summary>
        /// Ignore all target member(s) of the given <typeparamref name="TMember">Type</typeparamref>. Members will be
        /// ignored in mappings between all types and MappingRuleSets (create new, overwrite, etc).
        /// </summary>
        /// <typeparam name="TMember">The Type of target member to ignore.</typeparam>
        /// <returns>
        /// This <see cref="IGlobalConfigSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        public IGlobalConfigSettings IgnoreTargetMembersOfType<TMember>()
        {
            if (typeof(TMember) == typeof(object))
            {
                throw new MappingConfigurationException(
                    "Ignoring target members of type object would ignore everything!");
            }

            return IgnoreTargetMembersWhere(member => member.HasType<TMember>());
        }

        /// <summary>
        /// Ignore all target member(s) matching the given <paramref name="memberFilter"/>. Members will be
        /// ignored in mappings between all types and MappingRuleSets (create new, overwrite, etc).
        /// </summary>
        /// <param name="memberFilter">The matching function with which to select target members to ignore.</param>
        /// <returns>
        /// This <see cref="IGlobalConfigSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        public IGlobalConfigSettings IgnoreTargetMembersWhere(Expression<Func<TargetMemberSelector, bool>> memberFilter)
        {
            var configInfo = MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(_mapperContext);
            var configuredIgnoredMember = new ConfiguredIgnoredMember(configInfo, memberFilter);

            _mapperContext.UserConfigurations.Add(configuredIgnoredMember);
            return this;
        }

        #endregion

        MappingConfigStartingPoint IGlobalConfigSettings.AndWhenMapping => this;

        #endregion

        /// <summary>
        /// Configure how this mapper maps objects of the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TObject">The type of object to which the configuration will apply.</typeparam>
        /// <returns>An InstanceConfigurator with which to complete the configuration.</returns>
        public InstanceConfigurator<TObject> InstancesOf<TObject>() where TObject : class
            => new InstanceConfigurator<TObject>(_mapperContext);

        /// <summary>
        /// Configure how this mapper performs mappings from source Dictionary{string, T} instances.
        /// </summary>
        public DictionaryConfigurator<object> Dictionaries => DictionariesWithValueType<object>();

        /// <summary>
        /// Configure how this mapper performs mappings from source Dictionary{string, TValue} instances.
        /// </summary>
        /// <typeparam name="TValue">
        /// The type of values contained in the Dictionary to which the configuration will apply.
        /// </typeparam>
        /// <returns>A DictionaryConfigurator with which to continue the configuration.</returns>
        public DictionaryConfigurator<TValue> DictionariesWithValueType<TValue>()
            => new DictionaryConfigurator<TValue>(MappingConfigInfo.AllSourceTypes(_mapperContext));

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

        /// <summary>
        /// Configure how this mapper performs mappings from all source types and MappingRuleSets (create new, overwrite, 
        /// etc), to the target type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<object, TTarget> To<TTarget>() where TTarget : class
            => GetAllSourcesTargetTypeSpecifier(ci => ci.ForAllRuleSets()).To<TTarget>();

        /// <summary>
        /// Configure how this mapper performs object creation mappings from any source type to the target type 
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<object, TTarget> ToANew<TTarget>() where TTarget : class
            => GetAllSourcesTargetTypeSpecifier(ci => ci.ForRuleSet(Constants.CreateNew)).ToANew<TTarget>();

        /// <summary>
        /// Configure how this mapper performs OnTo (merge) mappings from any source type to the target type 
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<object, TTarget> OnTo<TTarget>() where TTarget : class
            => GetAllSourcesTargetTypeSpecifier(ci => ci.ForRuleSet(Constants.Merge)).OnTo<TTarget>();

        /// <summary>
        /// Configure how this mapper performs Over (overwrite) mappings from any source type to the target type 
        /// specified by the type argument.
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

        private TargetTypeSpecifier<TSource> GetTargetTypeSpecifier<TSource>(
            Func<MappingConfigInfo, MappingConfigInfo> configInfoConfigurator)
        {
            var configInfo = configInfoConfigurator.Invoke(new MappingConfigInfo(_mapperContext));

            return new TargetTypeSpecifier<TSource>(configInfo);
        }
    }
}