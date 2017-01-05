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

        public ISourceDictionaryConfigSettings<TValue, TTarget> UseFlattenedMemberNames()
        {
            SetupFlattenedMemberNames();
            return this;
        }

        public ISourceDictionaryConfigSettings<TValue, TTarget> UseMemberNameSeparator(string separator)
        {
            SetupMemberNameSeparator(separator);
            return this;
        }

        public ISourceDictionaryConfigSettings<TValue, TTarget> UseElementKeyPattern(string pattern)
        {
            var keyPartFactory = ElementKeyPartFactory.For(pattern, ConfigInfo);

            ConfigInfo.MapperContext.UserConfigurations.Dictionaries.Add(keyPartFactory);
            return this;
        }

        ISourceDictionaryMappingConfigurator<TValue, TTarget> ISourceDictionaryConfigSettings<TValue, TTarget>.And
            => this;

        #endregion

        public CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> MapFullKey(string fullMemberNameKey)
            => CreateTargetMemberSpecifier("keys", fullMemberNameKey, (settings, customKey) => settings.AddFullKey(customKey));

        public CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> MapMemberNameKey(string memberNameKeyPart)
        {
            return CreateTargetMemberSpecifier(
                "member names",
                memberNameKeyPart,
                (settings, customKey) => settings.AddMemberKey(customKey));
        }

        private CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> CreateTargetMemberSpecifier(
            string keyName,
            string key,
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
