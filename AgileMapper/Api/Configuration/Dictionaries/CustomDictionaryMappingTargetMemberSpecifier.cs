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
        : CustomDictionaryKeySpecifierBase<TValue, TTarget>
    {
        private readonly string _key;

        internal CustomDictionaryMappingTargetMemberSpecifier(
            MappingConfigInfo configInfo,
            string key,
            Action<DictionarySettings, CustomDictionaryKey> dictionarySettingsAction)
            : base(configInfo, dictionarySettingsAction)
        {
            _key = key;
        }

        /// <summary>
        /// Apply the configuration to the given <paramref name="targetMember"/>.
        /// </summary>
        /// <typeparam name="TTargetValue">The target member's type.</typeparam>
        /// <param name="targetMember">The target member to which to apply the configuration.</param>
        /// <returns>
        /// An ISourceDictionaryMappingConfigContinuation to enable further configuration of mappings from 
        /// dictionaries to the target type being configured.
        /// </returns>
        public ISourceDictionaryMappingConfigContinuation<TValue, TTarget> To<TTargetValue>(
            Expression<Func<TTarget, TTargetValue>> targetMember)
            => RegisterCustomKey(targetMember);

        /// <summary>
        /// Apply the configuration to the given <paramref name="targetSetMethod"/>.
        /// </summary>
        /// <typeparam name="TTargetValue">The type of the target set method's argument.</typeparam>
        /// <param name="targetSetMethod">The target set method to which to apply the configuration.</param>
        /// <returns>
        /// A ISourceDictionaryMappingConfigContinuation to enable further configuration of mappings from 
        /// dictionaries to the target type being configured.
        /// </returns>
        public ISourceDictionaryMappingConfigContinuation<TValue, TTarget> To<TTargetValue>(
            Expression<Func<TTarget, Action<TTargetValue>>> targetSetMethod)
            => RegisterCustomKey(targetSetMethod);

        private DictionaryMappingConfigContinuation<TValue, TTarget> RegisterCustomKey(LambdaExpression targetMemberLambda)
        {
            var configuredKey = CustomDictionaryKey.ForTargetMember(_key, targetMemberLambda, ConfigInfo);

            UserConfigurations.ThrowIfConflictingIgnoredMemberExists(configuredKey);
            UserConfigurations.ThrowIfConflictingDataSourceExists(configuredKey, GetConflictDescription);

            return RegisterCustomKey(configuredKey);
        }

        private static string GetConflictDescription(CustomDictionaryKey key)
            => $"Configured dictionary key member {key.TargetMember.GetPath()} has a configured data source";
    }
}