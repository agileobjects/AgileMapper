namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using System;
    using AgileMapper.Configuration;

    /// <summary>
    /// Provides base dictionary key configuration functionality for customising mappings
    /// to or from dictionaries.
    /// </summary>
    /// <typeparam name="TFirst">The first type argument necessary in the dictionary configuration.</typeparam>
    /// <typeparam name="TSecond">The second type argument necessary in the dictionary configuration.</typeparam>
    public abstract class CustomDictionaryKeySpecifierBase<TFirst, TSecond>
    {
        private readonly Action<DictionarySettings, CustomDictionaryKey> _dictionarySettingsAction;

        internal CustomDictionaryKeySpecifierBase(
            MappingConfigInfo configInfo,
            Action<DictionarySettings, CustomDictionaryKey> dictionarySettingsAction)
        {
            _dictionarySettingsAction = dictionarySettingsAction;
            ConfigInfo = configInfo;
        }

        internal MappingConfigInfo ConfigInfo { get; }

        internal UserConfigurationSet UserConfigurations => ConfigInfo.MapperContext.UserConfigurations;

        internal DictionaryMappingConfigContinuation<TFirst, TSecond> RegisterCustomKey(
            CustomDictionaryKey configuredKey)
        {
            _dictionarySettingsAction.Invoke(UserConfigurations.Dictionaries, configuredKey);

            return new DictionaryMappingConfigContinuation<TFirst, TSecond>(ConfigInfo);
        }
    }
}