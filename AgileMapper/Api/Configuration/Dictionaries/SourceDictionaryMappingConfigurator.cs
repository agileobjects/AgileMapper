namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using AgileMapper.Configuration;

    internal class SourceDictionaryMappingConfigurator<TValue, TTarget> :
        DictionaryMappingConfiguratorBase<Dictionary<string, TValue>, TTarget>,
        ISourceDictionaryMappingConfigurator<TValue, TTarget>
    {
        public SourceDictionaryMappingConfigurator(MappingConfigInfo configInfo)
            : base(configInfo)
        {
        }

        #region ISourceDictionaryConfigSettings Members

        public ISourceDictionaryConfigSettings<TValue, TTarget> UseMemberNameSeparator(string separator)
        {
            SetupMemberNameSeparator(separator);
            return this;
        }

        public ISourceDictionaryConfigSettings<TValue, TTarget> UseElementKeyPattern(string pattern)
        {
            SetupElementKeyPattern(pattern);
            return this;
        }

        ISourceDictionaryMappingConfigurator<TValue, TTarget> ISourceDictionaryConfigSettings<TValue, TTarget>.And
            => this;

        #endregion

        public CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> MapFullKey(string fullMemberNameKey)
            => CreateTargetMemberSpecifier(fullMemberNameKey, "keys", (settings, customKey) => settings.AddFullKey(customKey));

        public CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> MapMemberNameKey(string memberNameKeyPart)
        {
            return CreateTargetMemberSpecifier(
                memberNameKeyPart,
                "member names",
                (settings, customKey) => settings.AddMemberKey(customKey));
        }

        private CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> CreateTargetMemberSpecifier(
            string key,
            string keyName,
            Action<DictionarySettings, CustomDictionaryKey> dictionarySettingsAction)
        {
            if (key == null)
            {
                throw new MappingConfigurationException(keyName + " cannot be null");
            }

            return new CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget>(
                ConfigInfo,
                key,
                dictionarySettingsAction);
        }
    }
}
