namespace AgileObjects.AgileMapper.Api.Configuration
{
    using AgileMapper.Configuration;
    using AgileMapper.Configuration.Dictionaries;
    using Dictionaries;
    using Dynamics;

    /// <summary>
    /// Provides options for specifying the target type and mapping rule set to which the configuration should
    /// apply.
    /// </summary>
    /// <typeparam name="TSource">The source type being configured.</typeparam>
    public class TargetSpecifier<TSource>
    {
        private readonly MappingConfigInfo _configInfo;

        internal TargetSpecifier(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured in all MappingRuleSets 
        /// (create new, overwrite, etc), to the target type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<TSource, TTarget> To<TTarget>()
            => new MappingConfigurator<TSource, TTarget>(_configInfo.ForAllRuleSets());

        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured to the target 
        /// type specified by the type argument when mapping to new objects.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<TSource, TTarget> ToANew<TTarget>()
            => UsingRuleSet<TTarget>(Constants.CreateNew);

        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured to the target 
        /// type specified by the type argument when performing OnTo (merge) mappings.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<TSource, TTarget> OnTo<TTarget>()
            => UsingRuleSet<TTarget>(Constants.Merge);

        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured to the target 
        /// type specified by the type argument when performing Over (overwrite) mappings.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<TSource, TTarget> Over<TTarget>()
            => UsingRuleSet<TTarget>(Constants.Overwrite);

        private MappingConfigurator<TSource, TTarget> UsingRuleSet<TTarget>(string name)
            => new MappingConfigurator<TSource, TTarget>(_configInfo.ForRuleSet(name));

        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured in all MappingRuleSets 
        /// (create new, overwrite, etc), to target Dictionaries.
        /// </summary>
        public ITargetDictionaryMappingConfigurator<TSource, object> ToDictionaries => ToDictionariesWithValueType<object>();

        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured in all MappingRuleSets 
        /// (create new, overwrite, etc), to target Dictionary{string, <typeparamref name="TValue"/>} instances.
        /// </summary>
        /// <typeparam name="TValue">
        /// The type of values contained in the Dictionary to which the configuration will apply.
        /// </typeparam>
        /// <returns>An ITargetDictionaryConfigSettings with which to continue the configuration.</returns>
        public ITargetDictionaryMappingConfigurator<TSource, TValue> ToDictionariesWithValueType<TValue>()
            => new TargetDictionaryMappingConfigurator<TSource, TValue>(_configInfo.ForAllRuleSets());

        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured in all MappingRuleSets 
        /// (create new, overwrite, etc), to target ExpandoObjects.
        /// </summary>
        public ITargetDynamicMappingConfigurator<TSource> ToDynamics
            => new TargetDynamicMappingConfigurator<TSource>(_configInfo);
    }
}