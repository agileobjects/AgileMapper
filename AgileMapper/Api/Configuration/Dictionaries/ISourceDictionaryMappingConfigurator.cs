namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides options for configuring mappings from a Dictionary{string, <typeparamref name="TValue"/>} 
    /// to a given <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of values stored in the dictionary to which the configurations will apply.
    /// </typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    public interface ISourceDictionaryMappingConfigurator<TValue, TTarget> :
        IFullMappingConfigurator<Dictionary<string, TValue>, TTarget>,
        ISourceDictionaryConfigSettings<TValue, TTarget>
    {
        /// <summary>
        /// Configure a custom full dictionary key for a particular target member when mapping from and to the dictionary 
        /// and target types being configured.
        /// </summary>
        /// <param name="fullMemberNameKey">
        /// The dictionary key with which to retrieve the value to map to the configured target member.
        /// </param>
        /// <returns>
        /// A CustomDictionaryMappingTargetMemberSpecifier with which to specify the target member for which the custom 
        /// key should be used.
        /// </returns>
        CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> MapFullKey(string fullMemberNameKey);

        /// <summary>
        /// Configure a custom member name to use in a key for a particular target member when mapping from and 
        /// to the dictionary and target types being configured. For example, to map the key "Address.HouseName"
        /// to a 'Line1' member of an 'Address' member, use MapMemberNameKey("HouseName").To(a => a.Line1).
        /// </summary>
        /// <param name="memberNameKeyPart">
        /// The custom member name to use in a key with which to retrieve the value to map to the configured target member.
        /// </param>
        /// <returns>
        /// A CustomDictionaryMappingTargetMemberSpecifier with which to specify the target member for which the custom 
        /// member name should be used.
        /// </returns>
        CustomDictionaryMappingTargetMemberSpecifier<TValue, TTarget> MapMemberNameKey(string memberNameKeyPart);
    }
}