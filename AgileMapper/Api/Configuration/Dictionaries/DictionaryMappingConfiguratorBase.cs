namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using System;
    using AgileMapper.Configuration;
    using AgileMapper.Configuration.Dictionaries;

    internal abstract class DictionaryMappingConfiguratorBase<TSource, TTarget>
        : MappingConfigurator<TSource, TTarget>
    {
        protected DictionaryMappingConfiguratorBase(MappingConfigInfo configInfo)
            : base(configInfo)
        {
        }

        protected void SetupFlattenedTargetMemberNames(MappingConfigInfo configInfo = null)
        {
            var flattenedJoiningNameFactory = JoiningNameFactory.Flattened(configInfo ?? ConfigInfo);

            ConfigInfo.MapperContext.UserConfigurations.Dictionaries.Add(flattenedJoiningNameFactory);
        }

        protected void SetupMemberNameSeparator(string separator, MappingConfigInfo configInfo = null)
        {
            var joiningNameFactory = JoiningNameFactory.For(separator, configInfo ?? ConfigInfo);

            ConfigInfo.MapperContext.UserConfigurations.Dictionaries.Add(joiningNameFactory);
        }

        protected void SetupElementKeyPattern(string pattern, MappingConfigInfo configInfo = null)
        {
            var keyPartFactory = ElementKeyPartFactory.For(pattern, configInfo ?? ConfigInfo);

            ConfigInfo.MapperContext.UserConfigurations.Dictionaries.Add(keyPartFactory);
        }

        protected CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> MapFullKey<TValue>(string fullMemberNameKey)
        {
            return CreateTargetMemberSpecifier<TValue>(
                fullMemberNameKey,
                "keys",
                (settings, customKey) => settings.AddFullKey(customKey));
        }

        protected CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> MapMemberNameKey<TValue>(string memberNameKeyPart)
        {
            return CreateTargetMemberSpecifier<TValue>(
                memberNameKeyPart,
                "member name",
                (settings, customKey) => settings.AddMemberKey(customKey));
        }

        protected CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> CreateTargetMemberSpecifier<TValue>(
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