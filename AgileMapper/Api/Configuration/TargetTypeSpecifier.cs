namespace AgileObjects.AgileMapper.Api.Configuration
{
    using AgileMapper.Configuration;

    /// <summary>
    /// Provides options for specifying the target type and mapping rule set to which the configuration should
    /// apply.
    /// </summary>
    /// <typeparam name="TSource">The source type being configured.</typeparam>
    public class TargetTypeSpecifier<TSource>
    {
        private readonly MappingConfigInfo _configInfo;

        internal TargetTypeSpecifier(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
        }

        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured to the target 
        /// type specified by the type argument, irrespective of the MappingRuleSet used.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<TSource, TTarget> To<TTarget>() where TTarget : class
            => new MappingConfigurator<TSource, TTarget>(_configInfo.ForAllRuleSets());

        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured to the target 
        /// type specified by the type argument when mapping to new objects.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<TSource, TTarget> ToANew<TTarget>() where TTarget : class
            => UsingRuleSet<TTarget>(Constants.CreateNew);

        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured to the target 
        /// type specified by the type argument when performing OnTo (merge) mappings.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<TSource, TTarget> OnTo<TTarget>() where TTarget : class
            => UsingRuleSet<TTarget>(Constants.Merge);

        /// <summary>
        /// Configure how this mapper performs mappings from the source type being configured to the target 
        /// type specified by the type argument when performing Over (overwrite) mappings.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IFullMappingConfigurator with which to complete the configuration.</returns>
        public IFullMappingConfigurator<TSource, TTarget> Over<TTarget>() where TTarget : class
            => UsingRuleSet<TTarget>(Constants.Overwrite);

        private MappingConfigurator<TSource, TTarget> UsingRuleSet<TTarget>(string name) where TTarget : class
            => new MappingConfigurator<TSource, TTarget>(_configInfo.ForRuleSet(name));
    }
}