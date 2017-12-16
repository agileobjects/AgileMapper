namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using AgileMapper.Configuration;
    using Dynamics;

    internal class SourceDictionaryMappingConfigurator<TValue, TTarget> :
        DictionaryMappingConfiguratorBase<Dictionary<string, TValue>, TTarget>,
        ISourceDictionaryMappingConfigurator<TValue, TTarget>,
        ISourceDynamicMappingConfigurator<TTarget>
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
        {
            return CreateTargetMemberSpecifier(
                fullMemberNameKey,
                "keys",
                (settings, customKey) => settings.AddFullKey(customKey));
        }

        public CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> MapMemberNameKey(string memberNameKeyPart)
        {
            return CreateTargetMemberSpecifier(
                memberNameKeyPart,
                "member name",
                (settings, customKey) => settings.AddMemberKey(customKey));
        }

        public ICustomDynamicMappingTargetMemberSpecifier<TTarget> MapMember(string sourceMemberName)
        {
            return CreateTargetMemberSpecifier(
                sourceMemberName,
                "member name",
                (settings, customKey) => settings.AddFullKey(customKey));
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
