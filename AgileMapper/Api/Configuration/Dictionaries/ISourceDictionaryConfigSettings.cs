namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    /// <summary>
    /// Provides options for configuring how mappers will perform mappings from dictionaries.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of values stored in the dictionary to which the configurations will apply.
    /// </typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface ISourceDictionaryConfigSettings<TValue, TTarget>
    {
        /// <summary>
        /// Use the given <paramref name="separator"/> to separate member names when mapping to nested
        /// complex type members. For example, calling UseMemberName("-") will require a dictionary entry 
        /// with the key 'Address-Line1' to map to an Address.Line1 member.
        /// </summary>
        /// <param name="separator">
        /// The separator to use to separate member names when constructing dictionary keys for nested
        /// members.
        /// </param>
        /// <returns>
        /// An ISourceDictionaryConfigSettings to enable further configuration of mappings from dictionaries
        /// to the target type being configured.
        /// </returns>
        ISourceDictionaryConfigSettings<TValue, TTarget> UseMemberNameSeparator(string separator);

        /// <summary>
        /// Use the given <paramref name="pattern"/> to create the part of a dictionary key representing an 
        /// enumerable element. The pattern must contain a single 'i' character as a placeholder for the 
        /// enumerable index. For example, calling UseElementKeyPattern("(i)") and mapping from a dictionary
        /// to a collection of ints will generate searches for keys '(0)', '(1)', '(2)', etc.
        /// </summary>
        /// <param name="pattern">
        /// The pattern to use to create a dictionary key part representing an enumerable element.
        /// </param>
        /// <returns>
        /// An ISourceDictionaryConfigSettings to enable further configuration of mappings from dictionaries
        /// to the target type being configured.
        /// </returns>
        ISourceDictionaryConfigSettings<TValue, TTarget> UseElementKeyPattern(string pattern);

        /// <summary>
        /// Gets a link back to the full ISourceDictionaryMappingConfigurator, for api fluency.
        /// </summary>
        ISourceDictionaryMappingConfigurator<TValue, TTarget> And { get; }
    }
}