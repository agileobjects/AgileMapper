namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using AgileMapper.Configuration;

    /// <summary>
    /// Provides options for configuring how a mapper performs mapping from Dictionary{string, TValue} instances.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of values stored in the dictionary to which the configurations will apply.
    /// </typeparam>
    public class DictionaryConfigurator<TValue>
    {
        private readonly MappingConfigInfo _configInfo;

        internal DictionaryConfigurator(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo.ForSourceValueType<TValue>();
        }

        /// <summary>
        /// Configure how this mapper performs mappings from dictionaries in all MappingRuleSets 
        /// (create new, overwrite, etc), to the target type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IDictionaryMappingConfigurator with which to complete the configuration.</returns>
        public IDictionaryMappingConfigurator<TValue, TTarget> To<TTarget>() where TTarget : class
            => new DictionaryMappingConfigurator<TValue, TTarget>(_configInfo.ForAllRuleSets());
    }
}