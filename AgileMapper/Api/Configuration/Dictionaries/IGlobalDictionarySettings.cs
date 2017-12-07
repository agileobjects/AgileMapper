namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    /// <summary>
    /// Provides options for globally configuring how all mappers will perform mappings from dictionaries.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of values stored in the dictionary to which the configurations will apply.
    /// </typeparam>
    public interface IGlobalDictionarySettings<TValue>
    {
        /// <summary>
        /// Construct keys for target dictionary members using flattened member names. For example, a
        /// Person.Address.StreetName member would be mapped to a dictionary entry with the key 
        /// 'AddressStreetName'.
        /// </summary>
        /// <returns>
        /// An <see cref="IGlobalDictionarySettings{TValue}"/> with which to globally configure other 
        /// dictionary mapping aspects.
        /// </returns>
        IGlobalDictionarySettings<TValue> UseFlattenedTargetMemberNames();

        /// <summary>
        /// Use the given <paramref name="separator"/> to separate member names when mapping to nested
        /// complex type members of any target type. For example, calling UseMemberName("_") will require 
        /// a dictionary entry with the key 'Address_Line1' to map to an Address.Line1 member.
        /// </summary>
        /// <param name="separator">
        /// The separator to use to separate member names when constructing dictionary keys for nested
        /// members.
        /// </param>
        /// <returns>
        /// An <see cref="IGlobalDictionarySettings{TValue}"/> with which to globally configure other 
        /// dictionary mapping aspects.
        /// </returns>
        IGlobalDictionarySettings<TValue> UseMemberNameSeparator(string separator);

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
        /// An <see cref="IGlobalDictionarySettings{TValue}"/> with which to globally configure other 
        /// dictionary mapping aspects.
        /// </returns>
        IGlobalDictionarySettings<TValue> UseElementKeyPattern(string pattern);

        /// <summary>
        /// Gets a link back to the full <see cref="DictionaryConfigurator{TValue}"/>, for api fluency.
        /// </summary>
        DictionaryConfigurator<TValue> AndWhenMapping { get; }
    }
}