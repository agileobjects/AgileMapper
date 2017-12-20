namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
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
            => MapFullKey<TValue>(fullMemberNameKey);

        public CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> MapMemberNameKey(string memberNameKeyPart)
            => MapMemberNameKey<TValue>(memberNameKeyPart);
    }
}
