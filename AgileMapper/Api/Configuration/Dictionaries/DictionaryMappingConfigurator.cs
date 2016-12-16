namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using AgileMapper.Configuration;

    internal class DictionaryMappingConfigurator<TValue, TTarget> :
        MappingConfigurator<Dictionary<string, TValue>, TTarget>,
        IDictionaryMappingConfigurator<TValue, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;

        public DictionaryMappingConfigurator(MappingConfigInfo configInfo)
            : base(configInfo)
        {
            _configInfo = configInfo;
        }

        public CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> MapKey(string key)
            => CreateTargetMemberSpecifier(key, (settings, customKey) => settings.AddFullKey(customKey));

        public CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> MapMemberName(string memberName)
            => CreateTargetMemberSpecifier(memberName, (settings, customKey) => settings.AddMemberKey(customKey));

        private CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> CreateTargetMemberSpecifier(
            string key,
            Action<DictionarySettings, CustomDictionaryKey> dictionarySettingsAction)
        {
            return new CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget>(
                _configInfo,
                key,
                dictionarySettingsAction);
        }
    }
}