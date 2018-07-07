#if DYNAMIC_SUPPORTED
namespace AgileObjects.AgileMapper.Api.Configuration.Dynamics
{
    /// <summary>
    /// Provides options for configuring how this mapper will perform mappings from source ExpandoObjects.
    /// </summary>
    public interface ISourceDynamicSettings
    {
        /// <summary>
        /// Construct flattened member names for target Dynamic members. For example, an
        /// ExpandoObject.Address.StreetName member would be mapped to a Dynamic member with the 
        /// name 'AddressStreetName'.
        /// </summary>
        /// <returns>
        /// The <see cref="ISourceDynamicSettings"/> with which to configure other aspects of source 
        /// ExpandoObject mapping.
        /// </returns>
        ISourceDynamicSettings UseFlattenedTargetMemberNames();

        /// <summary>
        /// Use the given <paramref name="separator"/>  to construct expected source ExpandoObject 
        /// member names, and to separate member names when mapping to nested complex type members of 
        /// any target type - the default is '_'. For example, calling UseMemberNameSeparator("-") 
        /// will require a source ExpandoObject member with the name 'Address-Line1' to map to an 
        /// Address.Line1 member. Any string can be specified as a separator - even if it would create 
        /// illegal member names like 'Address-Line1' - because ExpandoObjects are mapped as 
        /// IDictionary{string, object}s.
        /// </summary>
        /// <param name="separator">
        /// The separator to use to separate member names when constructing expected source Dynamic 
        /// member names for nested members.
        /// </param>
        /// <returns>
        /// The <see cref="ISourceDynamicSettings"/> with which to configure other aspects of source 
        /// ExpandoObject mapping.
        /// </returns>
        ISourceDynamicSettings UseMemberNameSeparator(string separator);

        /// <summary>
        /// Use the given <paramref name="pattern"/> to create the part of an expected Dynamic member name 
        /// representing an enumerable element - the default is '_i'. The pattern must contain a single 'i' 
        /// character as a placeholder for the enumerable index. For example, calling UseElementKeyPattern("-i-") 
        /// and mapping from a Dynamic to a collection of ints will generate searches for member names '-0-', '-1-', 
        /// '-2-', etc. Any pattern can be specified as an element key - even if it would create illegal member 
        /// names like '-0-' - because ExpandoObjects are mapped as IDictionary{string, Object}s. 
        /// </summary>
        /// <param name="pattern">
        /// The pattern to use to create an expected source Dynamic member name part representing an enumerable 
        /// element.
        /// </param>
        /// <returns>
        /// The <see cref="ISourceDynamicSettings"/> with which to configure other aspects of source ExpandoObject 
        /// mapping.
        /// </returns>
        ISourceDynamicSettings UseElementKeyPattern(string pattern);

        /// <summary>
        /// Gets a link back to the full <see cref="ISourceDynamicTargetTypeSelector"/>, 
        /// for api fluency.
        /// </summary>
        ISourceDynamicTargetTypeSelector AndWhenMapping { get; }
    }
}
#endif