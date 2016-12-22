namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    /// <summary>
    /// Provides options for configuring how mappers will perform mappings from dictionaries.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of values stored in the dictionary to which the configurations will apply.
    /// </typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface IDictionaryConfigSettings<TValue, TTarget>
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
        /// A DictionaryMappingConfigContinuation to enable further configuration of mappings from dictionaries
        /// to the target type being configured.
        /// </returns>
        DictionaryMappingConfigContinuation<TValue, TTarget> UseMemberNameSeparator(string separator);
    }
}