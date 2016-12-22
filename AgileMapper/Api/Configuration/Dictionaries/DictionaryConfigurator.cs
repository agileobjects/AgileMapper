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
        /// Construct dictionary keys for nested members using flattened member names. For example, a
        /// Person.Address.StreetName member would be populated using the dictionary entry with key 
        /// 'AddressStreetName' when mapping to a root Person object.
        /// </summary>
        public void UseFlattenedMemberNames()
        {
            var flattenedJoiningNameFactory = JoiningNameFactory.Flattened(GlobalConfigInfo);

            _configInfo.MapperContext.UserConfigurations.Dictionaries.Add(flattenedJoiningNameFactory);
        }

        /// <summary>
        /// Use the given <paramref name="separator"/> to separate member names when mapping to nested
        /// complex type members of any target type. For example, calling UseMemberName("_") will require 
        /// a dictionary entry with the key 'Address_Line1' to map to an Address.Line1 member.
        /// </summary>
        /// <param name="separator">
        /// The separator to use to separate member names when constructing dictionary keys for nested
        /// members.
        /// </param>
        public void UseMemberNameSeparator(string separator)
        {
            var joiningNameFactory = JoiningNameFactory.For(separator, GlobalConfigInfo);

            _configInfo.MapperContext.UserConfigurations.Dictionaries.Add(joiningNameFactory);
        }

        private MappingConfigInfo GlobalConfigInfo => _configInfo.ForAllRuleSets().ForAllTargetTypes();

        /// <summary>
        /// Configure how this mapper performs mappings from dictionaries in all MappingRuleSets 
        /// (create new, overwrite, etc), to the target type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IDictionaryMappingConfigurator with which to complete the configuration.</returns>
        public IDictionaryMappingConfigurator<TValue, TTarget> To<TTarget>() where TTarget : class
            => CreateConfigurator<TTarget>(_configInfo.ForAllRuleSets());

        /// <summary>
        /// Configure how this mapper performs object creation mappings from dictionaries to the target type 
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IDictionaryMappingConfigurator with which to complete the configuration.</returns>
        public IDictionaryMappingConfigurator<TValue, TTarget> ToANew<TTarget>() where TTarget : class
            => CreateConfigurator<TTarget>(Constants.CreateNew);

        /// <summary>
        /// Configure how this mapper performs OnTo (merge) mappings from dictionaries to the target type 
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IDictionaryMappingConfigurator with which to complete the configuration.</returns>
        public IDictionaryMappingConfigurator<TValue, TTarget> OnTo<TTarget>() where TTarget : class
            => CreateConfigurator<TTarget>(Constants.Merge);

        /// <summary>
        /// Configure how this mapper performs Over (overwrite) mappings from dictionaries to the target type 
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An IDictionaryMappingConfigurator with which to complete the configuration.</returns>
        public IDictionaryMappingConfigurator<TValue, TTarget> Over<TTarget>() where TTarget : class
            => CreateConfigurator<TTarget>(Constants.Overwrite);

        private IDictionaryMappingConfigurator<TValue, TTarget> CreateConfigurator<TTarget>(string ruleSetName)
            => CreateConfigurator<TTarget>(_configInfo.ForRuleSet(ruleSetName));

        private static IDictionaryMappingConfigurator<TValue, TTarget> CreateConfigurator<TTarget>(MappingConfigInfo configInfo)
            => new DictionaryMappingConfigurator<TValue, TTarget>(configInfo.ForTargetType<TTarget>());
    }
}
