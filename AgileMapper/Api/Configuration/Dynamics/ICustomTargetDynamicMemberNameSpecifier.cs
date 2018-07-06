#if DYNAMIC_SUPPORTED
namespace AgileObjects.AgileMapper.Api.Configuration.Dynamics
{
    /// <summary>
    /// Provides options for specifying custom target ExpandoObject member names to which configured
    /// source members should be mapped.
    /// </summary>
    public interface ICustomTargetDynamicMemberNameSpecifier<TSource>
    {
        /// <summary>
        /// Configure a custom full ExpandoObject member name to use in place of the configured source 
        /// member's name when constructing a target ExpandoObject member name. For example, calling 
        /// Map(address => address.Line1).ToFullMemberName("StreetName") will generate the key 'StreetName'
        /// when mapping an Address.Line1 property to an ExpandoObject, instead of the default 'Address_Line1'.
        /// </summary>
        /// <param name="fullMemberName">
        /// The member name to which to map the value of the configured source member.
        /// </param>
        /// <returns>
        /// An ITargetDynamicMappingConfigContinuation to enable further configuration of mappings between the 
        /// source and target types being configured.
        /// </returns>
        ITargetDynamicMappingConfigContinuation<TSource> ToFullMemberName(string fullMemberName);

        /// <summary>
        /// Use the given <paramref name="memberName"/> in place of the configured source member's name
        /// when constructing a target ExpandoObject member name. For example, calling 
        /// Map(address => address.Line1).ToMemberName("StreetName") will generate the member name 
        /// 'Address_StreetName' when mapping an Address.Line1 property to an ExpandoObject, instead of 
        /// the default 'Address_Line1'.
        /// </summary>
        /// <param name="memberName">
        /// The member name to use in place of the configured source member's name.
        /// </param>
        /// <returns>
        /// An ITargetDynamicMappingConfigContinuation to enable further configuration of mappings between the 
        /// source and target types being configured.
        /// </returns>
        ITargetDynamicMappingConfigContinuation<TSource> ToMemberName(string memberName);
    }
}
#endif