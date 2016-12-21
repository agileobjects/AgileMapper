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

        #region IDictionaryConfigSettings Members

        void IDictionaryConfigSettings.UseMemberNameSeparator(string separator)
        {
            var joiningNameFactory = JoiningNameFactory.For(separator, _configInfo);

            _configInfo.MapperContext.UserConfigurations.Dictionaries.Add(joiningNameFactory);
        }

        #endregion

        public CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> MapKey(string key)
            => CreateTargetMemberSpecifier("keys", key, (settings, customKey) => settings.AddFullKey(customKey));

        public CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> MapMemberName(string memberName)
        {
            return CreateTargetMemberSpecifier(
                "member names",
                memberName,
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
                _configInfo,
                key,
                dictionarySettingsAction);
        }
    }
}