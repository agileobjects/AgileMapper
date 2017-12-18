namespace AgileObjects.AgileMapper.Api.Configuration.Dynamics
{
    using System;
    using AgileMapper.Configuration;
    using AgileMapper.Configuration.Dictionaries;
    using Dictionaries;
    using Members;

    /// <summary>
    /// Provides options for specifying custom target ExpandoObject member names to which 
    /// configured source members should be mapped.
    /// </summary>
    public class CustomTargetDynamicMemberNameSpecifier<TSource> :
        CustomDictionaryKeySpecifierBase<TSource, object>
    {
        internal CustomTargetDynamicMemberNameSpecifier(MappingConfigInfo configInfo, QualifiedMember sourceMember)
            : base(configInfo, sourceMember)
        {
        }

        /// <summary>
        /// Configure a custom full Dictionary key to use in place of the configured source member's name
        /// when constructing a target Dictionary key. For example, calling 
        /// Map(address => address.Line1).ToFullKey("StreetName") will generate the key 'StreetName'
        /// when mapping an Address.Line1 property to a Dictionary, instead of the default 'Address.Line1'.
        /// </summary>
        /// <param name="fullMemberNameKey">
        /// The Dictionary key to which to map the value of the configured source member.
        /// </param>
        /// <returns>
        /// An ITargetDictionaryMappingConfigContinuation to enable further configuration of mappings between 
        /// the source and target Dictionary types being configured.
        /// </returns>
        public ITargetDynamicMappingConfigContinuation<TSource> ToFullKey(string fullMemberNameKey)
            => RegisterMemberKey(fullMemberNameKey, (settings, customKey) => settings.AddFullKey(customKey));

        /// <summary>
        /// Use the given <paramref name="memberNameKeyPart"/> in place of the configured source member's name
        /// when constructing a target Dictionary key. For example, calling 
        /// Map(address => address.Line1).ToMemberKey("StreetName") will generate the key 'Address.StreetName'
        /// when mapping an Address.Line1 property to a Dictionary, instead of the default 'Address.Line1'.
        /// </summary>
        /// <param name="memberNameKeyPart">
        /// The member key part to use in place of the configured source member's name.
        /// </param>
        /// <returns>
        /// An ITargetDictionaryMappingConfigContinuation to enable further configuration of mappings between 
        /// the source and target Dictionary types being configured.
        /// </returns>
        public ITargetDynamicMappingConfigContinuation<TSource> ToMemberNameKey(string memberNameKeyPart)
            => RegisterMemberKey(memberNameKeyPart, (settings, customKey) => settings.AddMemberKey(customKey));

        private ITargetDynamicMappingConfigContinuation<TSource> RegisterMemberKey(
            string key,
            Action<DictionarySettings, CustomDictionaryKey> dictionarySettingsAction)
        {
            return RegisterCustomKey(key, dictionarySettingsAction);
        }
    }
}