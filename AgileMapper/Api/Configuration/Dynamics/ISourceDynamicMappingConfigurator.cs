#if DYNAMIC_SUPPORTED
namespace AgileObjects.AgileMapper.Api.Configuration.Dynamics
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides options for configuring mappings from an ExpandoObject to a given <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    // ReSharper disable once PossibleInterfaceMemberAmbiguity
    public interface ISourceDynamicMappingConfigurator<TTarget> :
        IFullMappingConfigurator<IDictionary<string, object>, TTarget>,
        ISourceDynamicConfigSettings<TTarget>
    {
        /// <summary>
        /// Configure a custom source member for a particular target member when mapping from an ExpandoObject 
        /// to the target type being configured.
        /// </summary>
        /// <param name="sourceMemberName">
        /// The name of the source member from which to retrieve the value to map to the configured target member.
        /// </param>
        /// <returns>
        /// An ICustomDynamicMappingTargetMemberSpecifier with which to specify the target member for which the 
        /// member with the given <paramref name="sourceMemberName"/> should be used.
        /// </returns>
        ICustomDynamicMappingTargetMemberSpecifier<TTarget> MapFullMemberName(string sourceMemberName);

        /// <summary>
        /// Configure a custom member name to use in a key for a particular target member when mapping from an 
        /// ExpandoObject to the target type being configured. For example, to map the member "Address.HouseName"
        /// to a 'Line1' member of an 'Address' member, use MapMemberName("HouseName").To(a => a.Line1).
        /// </summary>
        /// <param name="memberNamePart">
        /// The custom member name to use in a key with which to retrieve the value to map to the configured target member.
        /// </param>
        /// <returns>
        /// A CustomDictionaryMappingTargetMemberSpecifier with which to specify the target member for which the custom 
        /// member name should be used.
        /// </returns>
        ICustomDynamicMappingTargetMemberSpecifier<TTarget> MapMemberName(string memberNamePart);
    }
}
#endif