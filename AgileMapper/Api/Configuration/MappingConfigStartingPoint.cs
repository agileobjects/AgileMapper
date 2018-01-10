namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AgileMapper.Configuration;
    using AgileMapper.Configuration.Dictionaries;
    using Dictionaries;
    using Dynamics;
    using Extensions.Internal;
    using Members;
    using static Constants;
    using static AgileMapper.Configuration.Dictionaries.DictionaryType;

    /// <summary>
    /// Provides options for configuring how a mapper performs a mapping.
    /// </summary>
    public class MappingConfigStartingPoint : IGlobalConfigSettings
    {
        private readonly MappingConfigInfo _configInfo;

        internal MappingConfigStartingPoint(MapperContext mapperContext)
        {
            _configInfo = new MappingConfigInfo(mapperContext);
        }

        private MapperContext MapperContext => _configInfo.MapperContext;

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
            var exceptionCallback = new ExceptionCallback(GlobalConfigInfo, callback.ToConstantExpression());

            MapperContext.UserConfigurations.Add(exceptionCallback);
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
        {
            MapperContext.Naming.AddNamePrefixes(prefixes);
            return this;
        }

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
        {
            MapperContext.Naming.AddNameSuffixes(suffixes);
            return this;
        }

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
            MapperContext.Naming.AddNameMatchers(patterns);
            return this;
        }

        #endregion

        /// <summary>
        /// Ensure 1-to-1 relationships between source and mapped objects by tracking and reusing mapped objects if 
        /// they appear more than once in a source object tree. Mapped objects are automatically tracked in object 
        /// trees with circular relationships - unless <see cref="DisableObjectTracking"/> is called - so configuring 
        /// this option is not necessary when mapping circular relationships.
        /// </summary>
        /// <returns>
        /// An <see cref="IGlobalConfigSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        public IGlobalConfigSettings MaintainIdentityIntegrity()
        {
            MapperContext.UserConfigurations.Add(MappedObjectCachingSettings.CacheAll);
            return this;
        }

        /// <summary>
        /// Disable tracking of objects during circular relationship mapping between all source and target types. 
        /// Mapped objects are tracked by default when mapping circular relationships to prevent stack overflows 
        /// if two objects in a source object tree hold references to each other, and to ensure 1-to-1 relationships 
        /// between source and mapped objects. If you are confident that each object in a source object tree appears 
        /// only once, disabling object tracking will increase mapping performance.
        /// </summary>
        /// <returns>
        /// An <see cref="IGlobalConfigSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        public IGlobalConfigSettings DisableObjectTracking()
        {
            MapperContext.UserConfigurations.Add(MappedObjectCachingSettings.CacheNone);
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
            MapperContext.UserConfigurations.Add(new NullCollectionsSetting(GlobalConfigInfo));
            return this;
        }

        /// <summary>
        /// Throw an exception upon creation of a mapper if the mapping plan has any target members which will not be mapped, 
        /// or maps from a source enum to a target enum which does not support all of its values. Call this method to validate 
        /// mapping plans during development; remove it in production code.
        /// </summary>
        /// <returns>
        /// This <see cref="IGlobalConfigSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        public IGlobalConfigSettings ThrowIfAnyMappingPlanIsIncomplete()
        {
            MapperContext.UserConfigurations.ValidateMappingPlans = true;
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
        public EnumPairSpecifier<object, object, TFirstEnum> PairEnum<TFirstEnum>(TFirstEnum enumMember) where TFirstEnum : struct
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
        public EnumPairSpecifier<object, object, TFirstEnum> PairEnums<TFirstEnum>(params TFirstEnum[] enumMembers)
            where TFirstEnum : struct
        {
            return EnumPairSpecifier<object, object, TFirstEnum>.For(GlobalConfigInfo, enumMembers);
        }

        /// <summary>
        /// Scan the given <paramref name="assemblies"/> when looking for types derived from any source or 
        /// target type being mapped.
        /// </summary>
        /// <param name="assemblies">The assemblies in which to look for derived types.</param>
        /// <returns>
        /// This <see cref="IGlobalConfigSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        public IGlobalConfigSettings LookForDerivedTypesIn(params Assembly[] assemblies)
        {
            SetDerivedTypeAssemblies(assemblies);
            return this;
        }

        internal static void SetDerivedTypeAssemblies(Assembly[] assemblies)
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

            GlobalContext.Instance.DerivedTypes.AddAssemblies(assemblies);
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
            var configuredIgnoredMember = new ConfiguredIgnoredMember(GlobalConfigInfo, memberFilter);

            MapperContext.UserConfigurations.Add(configuredIgnoredMember);
            return this;
        }

        #endregion

        /// <summary>
        /// Configure a formatting string to use when mapping from the given <typeparamref name="TSourceValue"/>
        /// to strings, for all source and target types.
        /// </summary>
        /// <typeparam name="TSourceValue">The source value type to which to apply a formatting string.</typeparam>
        /// <param name="formatSelector">An action which supplies the formatting string.</param>
        /// <returns>
        /// This <see cref="IGlobalConfigSettings"/> with which to globally configure other mapping aspects.
        /// </returns>
        public IGlobalConfigSettings StringsFrom<TSourceValue>(Action<StringFormatSpecifier> formatSelector)
        {
            var formatSpecifier = new StringFormatSpecifier(MapperContext, typeof(TSourceValue));

            formatSelector.Invoke(formatSpecifier);

            formatSpecifier.ErrorIfInvalid();

            return this;
        }

        MappingConfigStartingPoint IGlobalConfigSettings.AndWhenMapping => this;

        private MappingConfigInfo GlobalConfigInfo =>
            _configInfo.ForAllRuleSets().ForAllSourceTypes().ForAllTargetTypes();

        #endregion

        /// <summary>
        /// Configure how this mapper maps objects of the type specified by the given <paramref name="exampleInstance"/>. 
        /// Use this overload for anonymous types.
        /// </summary>
        /// <typeparam name="TObject">The type of object to which the configuration will apply.</typeparam>
        /// <param name="exampleInstance">
        /// An instance specifying the source type for which mapping will be configured.
        /// </param>
        /// <returns>An InstanceConfigurator with which to complete the configuration.</returns>
        public InstanceConfigurator<TObject> InstancesOf<TObject>(TObject exampleInstance) where TObject : class
            => InstancesOf<TObject>();

        /// <summary>
        /// Configure how this mapper maps objects of the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TObject">The type of object to which the configuration will apply.</typeparam>
        /// <returns>An InstanceConfigurator with which to complete the configuration.</returns>
        public InstanceConfigurator<TObject> InstancesOf<TObject>() where TObject : class
            => new InstanceConfigurator<TObject>(GlobalConfigInfo);

        #region Dictionaries

        /// <summary>
        /// Configure how this mapper performs mappings from or to source Dictionary instances
        /// with any Dictionary value type.
        /// </summary>
        public IGlobalDictionarySettings<object> Dictionaries
            => CreateDictionaryConfigurator<object>(Dictionary, sourceValueType: AllTypes);

        /// <summary>
        /// Configure how this mapper performs mappings from or to source Dictionary{string, TValue} instances.
        /// </summary>
        /// <typeparam name="TValue">
        /// The type of values contained in the Dictionary to which the configuration will apply.
        /// </typeparam>
        /// <returns>
        /// An IGlobalDictionarySettings with which to continue other global aspects of Dictionary mapping.
        /// </returns>
        public IGlobalDictionarySettings<TValue> DictionariesWithValueType<TValue>()
            => CreateDictionaryConfigurator<TValue>(Dictionary);

        /// <summary>
        /// Configure how this mapper performs mappings from source Dictionary instances with 
        /// any Dictionary value type.
        /// </summary>
        public ISourceDictionaryTargetTypeSelector<object> FromDictionaries
            => CreateDictionaryConfigurator<object>(Dictionary, sourceValueType: AllTypes);

        /// <summary>
        /// Configure how this mapper performs mappings from source Dictionary{string, TValue} instances.
        /// </summary>
        /// <typeparam name="TValue">
        /// The type of values contained in the Dictionary to which the configuration will apply.
        /// </typeparam>
        /// <returns>
        /// An ISourceDictionaryTargetTypeSelector with which to specify to which target type the 
        /// configuration will apply.
        /// </returns>
        public ISourceDictionaryTargetTypeSelector<TValue> FromDictionariesWithValueType<TValue>()
            => CreateDictionaryConfigurator<TValue>(Dictionary);

        /// <summary>
        /// Configure how this mapper performs mappings from or to ExpandoObject instances.
        /// </summary>
        public IGlobalDynamicSettings Dynamics
            => CreateDictionaryConfigurator<object>(Expando, sourceValueType: AllTypes);

        /// <summary>
        /// Configure how this mapper performs mappings from source ExpandoObject instances.
        /// </summary>
        public ISourceDynamicTargetTypeSelector FromDynamics
            => CreateDictionaryConfigurator<object>(Expando, typeof(ExpandoObject), sourceValueType: AllTypes);

        private DictionaryMappingConfigurator<TValue> CreateDictionaryConfigurator<TValue>(
            DictionaryType dictionaryType,
            Type sourceType = null,
            Type sourceValueType = null)
        {
            var configInfo = _configInfo
                .ForSourceType(sourceType ?? AllTypes)
                .ForSourceValueType(sourceValueType ?? typeof(TValue))
                .Set(dictionaryType);

            return new DictionaryMappingConfigurator<TValue>(configInfo);
        }

        #endregion

        /// <summary>
        /// Configure how this mapper performs mappings from the source type specified by the given 
        /// <paramref name="exampleInstance"/>. Use this overload for anonymous types.
        /// </summary>
        /// <typeparam name="TSource">The type of the given <paramref name="exampleInstance"/>.</typeparam>
        /// <param name="exampleInstance">The instance specifying to which source type the configuration will apply.</param>
        /// <returns>A TargetSpecifier with which to specify to which target type the configuration will apply.</returns>
        public TargetSpecifier<TSource> From<TSource>(TSource exampleInstance) => From<TSource>();

        /// <summary>
        /// Configure how this mapper performs mappings from the source type specified by the type argument.
        /// </summary>
        /// <typeparam name="TSource">The source type to which the configuration will apply.</typeparam>
        /// <returns>A TargetSpecifier with which to specify to which target type the configuration will apply.</returns>
        public TargetSpecifier<TSource> From<TSource>()
            => GetTargetTypeSpecifier<TSource>(ci => ci.ForSourceType<TSource>());

        /// <summary>
        /// Configure how this mapper performs mappings from all source types and MappingRuleSets (create new, overwrite, 
        /// etc), to the target type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<object, TTarget> To<TTarget>()
            => GetAllSourcesTargetTypeSpecifier(ci => ci.ForAllRuleSets()).To<TTarget>();

        /// <summary>
        /// Configure how this mapper performs object creation mappings from any source type to the target type 
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<object, TTarget> ToANew<TTarget>()
            => GetAllSourcesTargetTypeSpecifier(ci => ci.ForRuleSet(CreateNew)).ToANew<TTarget>();

        /// <summary>
        /// Configure how this mapper performs OnTo (merge) mappings from any source type to the target type 
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<object, TTarget> OnTo<TTarget>()
            => GetAllSourcesTargetTypeSpecifier(ci => ci.ForRuleSet(Merge)).OnTo<TTarget>();

        /// <summary>
        /// Configure how this mapper performs Over (overwrite) mappings from any source type to the target type 
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<object, TTarget> Over<TTarget>()
            => GetAllSourcesTargetTypeSpecifier(ci => ci.ForRuleSet(Overwrite)).Over<TTarget>();

        private TargetSpecifier<object> GetAllSourcesTargetTypeSpecifier(
            Func<MappingConfigInfo, MappingConfigInfo> configInfoConfigurator)
        {
            return GetTargetTypeSpecifier<object>(ci =>
            {
                ci.ForAllSourceTypes();
                configInfoConfigurator.Invoke(ci);
                return ci;
            });
        }

        private TargetSpecifier<TSource> GetTargetTypeSpecifier<TSource>(
            Func<MappingConfigInfo, MappingConfigInfo> configInfoConfigurator)
        {
            var configInfo = configInfoConfigurator.Invoke(_configInfo);

            return new TargetSpecifier<TSource>(configInfo);
        }
    }
}