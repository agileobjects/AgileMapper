#if DYNAMIC_SUPPORTED
namespace AgileObjects.AgileMapper.Api.Configuration.Dynamics
{
    /// <summary>
    /// Provides options for configuring how mappers will perform mappings to dictionaries.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    public interface ITargetDynamicConfigSettings<TSource>
    {
        /// <summary>
        /// Construct ExpandoObject member names for nested members using flattened member names - the default is 
        /// to separate member names with '_'. For example, a Person.Address.StreetName member would be mapped to 
        /// an ExpandoObject member with name 'AddressStreetName' when mapping from a root Person object.
        /// </summary>
        /// <returns>
        /// An ITargetDynamicConfigSettings to enable further configuration of mappings from the source type
        /// being configured to ExpandoObjects.
        /// </returns>
        ITargetDynamicConfigSettings<TSource> UseFlattenedMemberNames();

        /// <summary>
        /// Use the given <paramref name="separator"/> to separate member names when mapping from nested complex 
        /// type members to ExpandoObjects - the default is '_'. For example, calling UseMemberNameSeparator("-") 
        /// will create an ExpandoObject member with the name 'Address-Line1' when mapping from an Address.Line1 
        /// member. Any string can be specified as a separator - even if it would create illegal member names like 
        /// 'Address-Line1' - because ExpandoObjects are mapped as IDictionary{string, object}s.
        /// </summary>
        /// <param name="separator">
        /// The separator to use to separate member names when constructing ExpandoObject member names for nested 
        /// members.
        /// </param>
        /// <returns>
        /// An ITargetDynamicConfigSettings to enable further configuration of mappings from the source type
        /// being configured to ExpandoObjects.
        /// </returns>
        ITargetDynamicConfigSettings<TSource> UseMemberNameSeparator(string separator);

        /// <summary>
        /// Use the given <paramref name="pattern"/> to create the part of an ExpandoObject member name 
        /// representing an enumerable element - the default is '_i. The pattern must contain a single 'i' 
        /// character as a placeholder for the enumerable index. For example, calling UseElementKeyPattern("(i)") 
        /// and mapping from a collection of ints to a Dictionary will generate keys '(0)', '(1)', '(2)', 
        /// etc. Any pattern can be specified as an element key - even if it would create illegal member names 
        /// like '(0)' - because ExpandoObjects are mapped as IDictionary{string, Object}s.
        /// </summary>
        /// <param name="pattern">
        /// The pattern to use to create a Dictionary key part representing an enumerable element.
        /// </param>
        /// <returns>
        /// An ITargetDynamicConfigSettings to enable further configuration of mappings from the source type
        /// being configured to ExpandoObjects.
        /// </returns>
        ITargetDynamicConfigSettings<TSource> UseElementKeyPattern(string pattern);

        /// <summary>
        /// Gets a link back to the full ITargetDictionaryMappingConfigurator, for api fluency.
        /// </summary>
        ITargetDynamicMappingConfigurator<TSource> And { get; }
    }
}
#endif