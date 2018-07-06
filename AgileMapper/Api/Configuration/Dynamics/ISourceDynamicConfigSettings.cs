#if DYNAMIC_SUPPORTED
namespace AgileObjects.AgileMapper.Api.Configuration.Dynamics
{
    /// <summary>
    /// Provides options for configuring how mappers will perform mappings from Dynamics to the given 
    /// <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface ISourceDynamicConfigSettings<TTarget>
    {
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
        /// The <see cref="ISourceDynamicConfigSettings{TTarget}"/> with which to configure other 
        /// aspects of source ExpandoObject mapping.
        /// </returns>
        ISourceDynamicConfigSettings<TTarget> UseMemberNameSeparator(string separator);

        /// <summary>
        /// Use the given <paramref name="pattern"/> to create the part of an expected Dynamic member name 
        /// representing an enumerable element - the default is '_i'. The pattern must contain a single 'i' 
        /// character as a placeholder for the enumerable index. Any pattern can be specified as an element 
        /// key - even if it would create illegal member names like '0-OrderItemId' - because ExpandoObjects 
        /// are mapped as IDictionary{string, Object}s. For example, calling UseElementKeyPattern("-i-") and 
        /// mapping from a Dynamic to a collection of ints will generate searches for member names '-0-', '-1-', 
        /// '-2-', etc.
        /// </summary>
        /// <param name="pattern">
        /// The pattern to use to create an expected source Dynamic member name part representing an enumerable 
        /// element.
        /// </param>
        /// <returns>
        /// The <see cref="ISourceDynamicConfigSettings{TTarget}"/> with which to configure other 
        /// aspects of source ExpandoObject mapping.
        /// </returns>
        ISourceDynamicConfigSettings<TTarget> UseElementKeyPattern(string pattern);

        /// <summary>
        /// Gets a link back to the full ISourceDynamicMappingConfigurator, for api fluency.
        /// </summary>
        ISourceDynamicMappingConfigurator<TTarget> And { get; }
    }
}
#endif