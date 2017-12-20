namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    /// <summary>
    /// Provides options for configuring how mappers will perform mappings from Dictionaries to the 
    /// given <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of values stored in the Dictionary to which the configurations will apply.
    /// </typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface ISourceDictionaryConfigSettings<TValue, TTarget>
    {
        /// <summary>
        /// Use the given <paramref name="separator"/>  to construct expected source Dictionary keys, 
        /// and to separate member names when mapping to nested complex type members of any target type - 
        /// the default is '.'. For example, calling UseMemberNameSeparator("_") will require a source 
        /// Dictionary entry with the key 'Address_Line1' to map to an Address.Line1 member.
        /// </summary>
        /// <param name="separator">
        /// The separator to use to separate member names when constructing expected source Dictionary 
        /// keys for nested members.
        /// </param>
        /// <returns>
        /// The <see cref="ISourceDictionaryConfigSettings{TValue, TTarget}"/> with which to configure 
        /// other aspects of source Dictionary mapping.
        /// </returns>
        ISourceDictionaryConfigSettings<TValue, TTarget> UseMemberNameSeparator(string separator);

        /// <summary>
        /// Use the given <paramref name="pattern"/> to create the part of an expected source Dictionary 
        /// key representing an enumerable element - the default is '[i]'. The pattern must contain a 
        /// single 'i' character as a placeholder for the enumerable index. For example, calling 
        /// UseElementKeyPattern("(i)") and mapping from a Dictionary to a collection of ints will generate 
        /// searches for keys '(0)', '(1)', '(2)', etc.
        /// </summary>
        /// <param name="pattern">
        /// The pattern to use to create an expected source Dictionary key part representing an enumerable 
        /// element.
        /// </param>
        /// <returns>
        /// The <see cref="ISourceDictionaryConfigSettings{TValue, TTarget}"/> with which to configure 
        /// other aspects of source Dictionary mapping.
        /// </returns>
        ISourceDictionaryConfigSettings<TValue, TTarget> UseElementKeyPattern(string pattern);

        /// <summary>
        /// Gets a link back to the full <see cref="ISourceDictionaryMappingConfigurator{TValue, TTarget}"/>, 
        /// for api fluency.
        /// </summary>
        ISourceDictionaryMappingConfigurator<TValue, TTarget> And { get; }
    }
}