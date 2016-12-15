namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using System;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;

    /// <summary>
    /// Provides options for specifying a target member to which a dictionary configuration should apply.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of values stored in the dictionary to which the configuration will apply.
    /// </typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public class CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly Expression _keyValue;

        internal CustomDictionaryMappingTargetMemberSpecifier(
            MappingConfigInfo configInfo,
            Expression keyValue)
        {
            _configInfo = configInfo.ForTargetType<TTarget>();
            _keyValue = keyValue;
        }

        /// <summary>
        /// Apply the configuration to the given <paramref name="targetMember"/>.
        /// </summary>
        /// <typeparam name="TTargetValue">The target member's type.</typeparam>
        /// <param name="targetMember">The target member to which to apply the configuration.</param>
        /// <returns>
        /// A DictionaryMappingConfigContinuation to enable further configuration of mappings from dictionaries
        /// to the target type being configured.
        /// </returns>
        public DictionaryMappingConfigContinuation<TValue, TTarget> To<TTargetValue>(
            Expression<Func<TTarget, TTargetValue>> targetMember)
        {
            var configuredKey = new CustomDictionaryKey(_keyValue, targetMember, _configInfo);

            _configInfo.MapperContext.UserConfigurations.Dictionaries.Add(configuredKey);

            return new DictionaryMappingConfigContinuation<TValue, TTarget>(_configInfo);
        }
    }
}