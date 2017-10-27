namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using AgileMapper.Configuration;

    /// <summary>
    /// Provides options for configuring how a mapper performs mapping from or to Dictionary{string, TValue} 
    /// instances.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of values stored in the dictionary to which the configurations will apply.
    /// </typeparam>
    public class DictionaryConfigurator<TValue> : IGlobalDictionarySettings<TValue>
    {
        private readonly MappingConfigInfo _configInfo;

        internal DictionaryConfigurator(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo.ForSourceValueType<TValue>();
        }

        #region IGlobalDictionarySettings Members

        /// <summary>
        /// Construct dictionary keys for nested members using flattened member names. For example, a
        /// Person.Address.StreetName member would be populated using the dictionary entry with key 
        /// 'AddressStreetName' when mapping to a root Person object.
        /// </summary>
        public IGlobalDictionarySettings<TValue> UseFlattenedMemberNames()
        {
            var flattenedJoiningNameFactory = JoiningNameFactory.Flattened(GlobalConfigInfo);

            _configInfo.MapperContext.UserConfigurations.Dictionaries.Add(flattenedJoiningNameFactory);
            return this;
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
        public IGlobalDictionarySettings<TValue> UseMemberNameSeparator(string separator)
        {
            var joiningNameFactory = JoiningNameFactory.For(separator, GlobalConfigInfo);

            _configInfo.MapperContext.UserConfigurations.Dictionaries.Add(joiningNameFactory);
            return this;
        }

        /// <summary>
        /// Use the given <paramref name="pattern"/> to create the part of a dictionary key representing an 
        /// enumerable element. The pattern must contain a single 'i' character as a placeholder for the 
        /// enmerable index. For example, calling UseElementKeyPattern("(i)") and mapping from a dictionary
        /// to a collection of ints will generate searches for keys '(0)', '(1)', '(2)', etc.
        /// </summary>
        /// <param name="pattern">
        /// The pattern to use to create a dictionary key part representing an enumerable element.
        /// </param>
        public IGlobalDictionarySettings<TValue> UseElementKeyPattern(string pattern)
        {
            var keyPartFactory = ElementKeyPartFactory.For(pattern, GlobalConfigInfo);

            _configInfo.MapperContext.UserConfigurations.Dictionaries.Add(keyPartFactory);
            return this;
        }

        DictionaryConfigurator<TValue> IGlobalDictionarySettings<TValue>.AndWhenMapping => this;

        #endregion

        private MappingConfigInfo GlobalConfigInfo => _configInfo.Clone().ForAllRuleSets().ForAllTargetTypes();

        /// <summary>
        /// Configure how this mapper performs mappings from dictionaries in all MappingRuleSets 
        /// (create new, overwrite, etc), to the target type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An ISourceDictionaryMappingConfigurator with which to complete the configuration.</returns>
        public ISourceDictionaryMappingConfigurator<TValue, TTarget> To<TTarget>() where TTarget : class
            => CreateConfigurator<TTarget>(_configInfo.ForAllRuleSets());

        /// <summary>
        /// Configure how this mapper performs object creation mappings from dictionaries to the target type 
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An ISourceDictionaryMappingConfigurator with which to complete the configuration.</returns>
        public ISourceDictionaryMappingConfigurator<TValue, TTarget> ToANew<TTarget>() where TTarget : class
            => CreateConfigurator<TTarget>(Constants.CreateNew);

        /// <summary>
        /// Configure how this mapper performs OnTo (merge) mappings from dictionaries to the target type 
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An ISourceDictionaryMappingConfigurator with which to complete the configuration.</returns>
        public ISourceDictionaryMappingConfigurator<TValue, TTarget> OnTo<TTarget>() where TTarget : class
            => CreateConfigurator<TTarget>(Constants.Merge);

        /// <summary>
        /// Configure how this mapper performs Over (overwrite) mappings from dictionaries to the target type 
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The target type to which the configuration will apply.</typeparam>
        /// <returns>An ISourceDictionaryMappingConfigurator with which to complete the configuration.</returns>
        public ISourceDictionaryMappingConfigurator<TValue, TTarget> Over<TTarget>() where TTarget : class
            => CreateConfigurator<TTarget>(Constants.Overwrite);

        private ISourceDictionaryMappingConfigurator<TValue, TTarget> CreateConfigurator<TTarget>(string ruleSetName)
            => CreateConfigurator<TTarget>(_configInfo.ForRuleSet(ruleSetName));

        private static ISourceDictionaryMappingConfigurator<TValue, TTarget> CreateConfigurator<TTarget>(MappingConfigInfo configInfo)
            => new SourceDictionaryMappingConfigurator<TValue, TTarget>(configInfo);
    }
}
