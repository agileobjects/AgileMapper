namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using System;
    using AgileMapper.Configuration;
    using Members;

    /// <summary>
    /// Provides options for specifying custom target dictionary keys to which configured
    /// source members should be mapped.
    /// </summary>
    public class CustomTargetDictionaryKeySpecifier<TSource, TValue>
        : CustomDictionaryKeySpecifierBase<TSource, TValue>
    {
        private readonly QualifiedMember _sourceMember;

        internal CustomTargetDictionaryKeySpecifier(
            MappingConfigInfo configInfo,
            QualifiedMember sourceMember,
            Action<DictionarySettings, CustomDictionaryKey> dictionarySettingsAction)
            : base(configInfo, dictionarySettingsAction)
        {
            _sourceMember = sourceMember;
        }

        /// <summary>
        /// Use the given <paramref name="memberNameKeyPart"/> in place of the configured source member's name
        /// when constructing a target dictionary key. For example, calling 
        /// Map(address => address.Line1).ToMemberKey("StreetName") will generate the key 'Address.StreetName'
        /// when mapping an Address property to a dictionary.
        /// </summary>
        /// <param name="memberNameKeyPart">
        /// The member key part to use in place of the configured source member's name.
        /// </param>
        /// <returns>
        /// An ITargetDictionaryMappingConfigContinuation to enable further configuration of mappings between 
        /// the source and target dictionary types being configured.
        /// </returns>
        public ITargetDictionaryMappingConfigContinuation<TSource, TValue> ToMemberNameKey(string memberNameKeyPart)
        {
            var configuredKey = CustomDictionaryKey.ForSourceMember(memberNameKeyPart, _sourceMember, ConfigInfo);

            return RegisterCustomKey(configuredKey);
        }
    }
}