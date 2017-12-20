namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    /// <summary>
    /// Provides options for configuring how mappers will perform mappings to Dictionaries.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TValue">
    /// The type of values stored in the Dictionary to which the configurations will apply.
    /// </typeparam>
    public interface ITargetDictionaryConfigSettings<TSource, TValue>
    {
        /// <summary>
        /// Construct Dictionary keys for nested members using flattened member names - the default is to 
        /// separate member names with '.'. For example, a Person.Address.StreetName member would be mapped to 
        /// a Dictionary entry with key 'AddressStreetName' when mapping from a root Person object.
        /// </summary>
        /// <returns>
        /// An ITargetDictionaryConfigSettings to enable further configuration of mappings from the source type
        /// being configured to Dictionaries.
        /// </returns>
        ITargetDictionaryConfigSettings<TSource, TValue> UseFlattenedMemberNames();

        /// <summary>
        /// Use the given <paramref name="separator"/> to separate member names when mapping from nested complex 
        /// type members to Dictionaries - the default is '.'. For example, calling UseMemberNameSeparator("_") 
        /// will create a Dictionary entry with the key 'Address_Line1' when mapping from an Address.Line1 member.
        /// </summary>
        /// <param name="separator">
        /// The separator to use to separate member names when constructing Dictionary keys for nested members.
        /// </param>
        /// <returns>
        /// An ITargetDictionaryConfigSettings to enable further configuration of mappings from the source type
        /// being configured to Dictionaries.
        /// </returns>
        ITargetDictionaryConfigSettings<TSource, TValue> UseMemberNameSeparator(string separator);

        /// <summary>
        /// Use the given <paramref name="pattern"/> to create the part of a Dictionary key representing an 
        /// enumerable element - the default is '[i]. The pattern must contain a single 'i' character as a 
        /// placeholder for the enumerable index. For example, calling UseElementKeyPattern("(i)") and mapping 
        /// from a collection of ints to a Dictionary will generate keys '(0)', '(1)', '(2)', etc.
        /// </summary>
        /// <param name="pattern">
        /// The pattern to use to create a Dictionary key part representing an enumerable element.
        /// </param>
        /// <returns>
        /// An ITargetDictionaryConfigSettings to enable further configuration of mappings from the source 
        /// type being configured to Dictionaries.
        /// </returns>
        ITargetDictionaryConfigSettings<TSource, TValue> UseElementKeyPattern(string pattern);

        /// <summary>
        /// Gets a link back to the full ITargetDictionaryMappingConfigurator, for api fluency.
        /// </summary>
        ITargetDictionaryMappingConfigurator<TSource, TValue> And { get; }
    }
}