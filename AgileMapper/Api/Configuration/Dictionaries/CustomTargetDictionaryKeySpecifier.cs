namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using System;
    using AgileMapper.Configuration;
    using AgileMapper.Configuration.Dictionaries;
    using Members;

    /// <summary>
    /// Provides options for specifying custom target dictionary keys to which configured
    /// source members should be mapped.
    /// </summary>
    public class CustomTargetDictionaryKeySpecifier<TSource, TValue>
        : CustomDictionaryKeySpecifierBase<TSource, TValue>
    {
        private readonly QualifiedMember _sourceMember;

        internal CustomTargetDictionaryKeySpecifier(MappingConfigInfo configInfo, QualifiedMember sourceMember)
            : base(configInfo)
        {
            _sourceMember = sourceMember;
        }

        /// <summary>
        /// Configure a custom full dictionary key to use in place of the configured source member's name
        /// when constructing a target dictionary key. For example, calling 
        /// Map(address => address.Line1).ToFullKey("StreetName") will generate the key 'StreetName'
        /// when mapping an Address.Line1 property to a dictionary, instead of the default 'Address.Line1'.
        /// </summary>
        /// <param name="fullMemberNameKey">
        /// The dictionary key to which to map the value of the configured source member.
        /// </param>
        /// <returns>
        /// An ITargetDictionaryMappingConfigContinuation to enable further configuration of mappings between 
        /// the source and target dictionary types being configured.
        /// </returns>
        public ITargetDictionaryMappingConfigContinuation<TSource, TValue> ToFullKey(string fullMemberNameKey)
            => RegisterMemberKey(fullMemberNameKey, (settings, customKey) => settings.AddFullKey(customKey));

        /// <summary>
        /// Use the given <paramref name="memberNameKeyPart"/> in place of the configured source member's name
        /// when constructing a target dictionary key. For example, calling 
        /// Map(address => address.Line1).ToMemberKey("StreetName") will generate the key 'Address.StreetName'
        /// when mapping an Address.Line1 property to a dictionary, instead of the default 'Address.Line1'.
        /// </summary>
        /// <param name="memberNameKeyPart">
        /// The member key part to use in place of the configured source member's name.
        /// </param>
        /// <returns>
        /// An ITargetDictionaryMappingConfigContinuation to enable further configuration of mappings between 
        /// the source and target dictionary types being configured.
        /// </returns>
        public ITargetDictionaryMappingConfigContinuation<TSource, TValue> ToMemberNameKey(string memberNameKeyPart)
            => RegisterMemberKey(memberNameKeyPart, (settings, customKey) => settings.AddMemberKey(customKey));

        private ITargetDictionaryMappingConfigContinuation<TSource, TValue> RegisterMemberKey(
            string key,
            Action<DictionarySettings, CustomDictionaryKey> dictionarySettingsAction)
        {
            var configuredKey = CustomDictionaryKey.ForSourceMember(key, _sourceMember, ConfigInfo);

            return RegisterCustomKey(configuredKey, dictionarySettingsAction);
        }
    }
}