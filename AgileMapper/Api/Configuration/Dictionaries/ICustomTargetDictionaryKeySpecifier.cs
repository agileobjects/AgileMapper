namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    /// <summary>
    /// Provides options for specifying custom target Dictionary keys to which configured
    /// source members should be mapped.
    /// </summary>
    public interface ICustomTargetDictionaryKeySpecifier<TSource, TValue>
    {
        /// <summary>
        /// Configure a custom full Dictionary key to use in place of the configured source member's name
        /// when constructing a target Dictionary key. For example, calling 
        /// Map(address => address.Line1).ToFullKey("StreetName") will generate the key 'StreetName'
        /// when mapping an Address.Line1 property to a Dictionary, instead of the default 'Address.Line1'.
        /// Using this method with an enumerable or complex type member in a flattening mapping will throw 
        /// a MappingConfigurationException as those members are always mapped as parts of their child 
        /// members. Use <see cref="ToMemberNameKey"/> instead.
        /// </summary>
        /// <param name="fullMemberNameKey">
        /// The Dictionary key to which to map the value of the configured source member.
        /// </param>
        /// <returns>
        /// An ITargetDictionaryMappingConfigContinuation to enable further configuration of mappings between 
        /// the source and target Dictionary types being configured.
        /// </returns>
        ITargetDictionaryMappingConfigContinuation<TSource, TValue> ToFullKey(string fullMemberNameKey);

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
        ITargetDictionaryMappingConfigContinuation<TSource, TValue> ToMemberNameKey(string memberNameKeyPart);
    }
}