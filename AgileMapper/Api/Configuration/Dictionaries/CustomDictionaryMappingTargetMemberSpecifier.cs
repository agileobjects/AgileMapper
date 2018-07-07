namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using System;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using AgileMapper.Configuration.Dictionaries;
    using DataSources;
#if DYNAMIC_SUPPORTED
    using Dynamics;
#endif
#if NET35
    using Extensions.Internal;
#endif

    /// <summary>
    /// Provides options for specifying a target member to which a dictionary configuration should apply.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of values stored in the dictionary to which the configuration will apply.
    /// </typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public class CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> :
        CustomDictionaryKeySpecifierBase<TValue, TTarget>
#if DYNAMIC_SUPPORTED
        ,
        ICustomDynamicMappingTargetMemberSpecifier<TTarget>
#endif
    {
        private readonly string _key;
        private readonly Action<DictionarySettings, CustomDictionaryKey> _dictionarySettingsAction;

        internal CustomDictionaryMappingTargetMemberSpecifier(
            MappingConfigInfo configInfo,
            string key,
            Action<DictionarySettings, CustomDictionaryKey> dictionarySettingsAction)
            : base(configInfo)
        {
            _key = key;
            _dictionarySettingsAction = dictionarySettingsAction;
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

#if DYNAMIC_SUPPORTED
        ISourceDynamicMappingConfigContinuation<TTarget> ICustomDynamicMappingTargetMemberSpecifier<TTarget>.To<TTargetValue>(
            Expression<Func<TTarget, TTargetValue>> targetMember)
            => RegisterCustomKey(targetMember);
#endif

        private DictionaryMappingConfigContinuation<TValue, TTarget> RegisterCustomKey(LambdaExpression targetMemberLambda)
        {
            var configuredKey = CustomDictionaryKey.ForTargetMember(
                _key,
#if NET35
                targetMemberLambda.ToDlrExpression(),
#else
                targetMemberLambda,
#endif
                ConfigInfo);

            UserConfigurations.ThrowIfConflictingIgnoredMemberExists(configuredKey);
            UserConfigurations.ThrowIfConflictingDataSourceExists(configuredKey, GetConflictMessage);

            return RegisterCustomKey(configuredKey, _dictionarySettingsAction);
        }

        private static string GetConflictMessage(
            CustomDictionaryKey key,
            ConfiguredDataSourceFactory conflictingDataSource)
        {
            return key.GetConflictMessage(conflictingDataSource);
        }
    }
}