namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    /// <summary>
    /// Provides options for configuring how this mapper will perform mappings from and to dictionaries.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of values stored in the dictionary to which the configurations will apply.
    /// </typeparam>
    public interface IGlobalDictionarySettings<TValue>
    {
        /// <summary>
        /// Construct keys for target Dictionary members using flattened member names. For example, 
        /// a Person.Address.StreetName member would be mapped to a Dictionary entry with the key 
        /// 'AddressStreetName'.
        /// </summary>
        /// <returns>
        /// An <see cref="IGlobalDictionarySettings{TValue}"/> with which to globally configure other 
        /// Dictionary mapping aspects.
        /// </returns>
        IGlobalDictionarySettings<TValue> UseFlattenedTargetMemberNames();

        /// <summary>
        /// Use the given <paramref name="separator"/> to construct expected source and target Dictionary 
        /// keys, and to separate member names when mapping to nested complex type members of any target 
        /// type - the default is '.'. For example, calling UseMemberNameSeparator("_") will require a 
        /// Dictionary entry with the key 'Address_Line1' to map to an Address.Line1 member.
        /// </summary>
        /// <param name="separator">
        /// The separator to use to separate member names when constructing expected Dictionary keys for 
        /// nested members.
        /// </param>
        /// <returns>
        /// An <see cref="IGlobalDictionarySettings{TValue}"/> with which to globally configure other 
        /// Dictionary mapping aspects.
        /// </returns>
        IGlobalDictionarySettings<TValue> UseMemberNameSeparator(string separator);

        /// <summary>
        /// Use the given <paramref name="pattern"/> to create the part of an expected Dictionary key 
        /// representing an enumerable element - the default is '[i]'. The pattern must contain a single 
        /// 'i' character as a placeholder for the enumerable index. For example, calling 
        /// UseElementKeyPattern("(i)") and mapping from a Dictionary to a collection of ints will generate 
        /// searches for keys '(0)', '(1)', '(2)', etc.
        /// </summary>
        /// <param name="pattern">
        /// The pattern to use to create an expected Dictionary key part representing an enumerable element.
        /// </param>
        /// <returns>
        /// An <see cref="IGlobalDictionarySettings{TValue}"/> with which to globally configure other 
        /// Dictionary mapping aspects.
        /// </returns>
        IGlobalDictionarySettings<TValue> UseElementKeyPattern(string pattern);

        /// <summary>
        /// Gets a link back to the full <see cref="MappingConfigStartingPoint"/>, for api fluency.
        /// </summary>
        MappingConfigStartingPoint AndWhenMapping { get; }
    }
}