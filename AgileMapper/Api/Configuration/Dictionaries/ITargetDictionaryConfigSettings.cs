namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    /// <summary>
    /// Provides options for configuring how mappers will perform mappings to dictionaries.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TValue">
    /// The type of values stored in the dictionary to which the configurations will apply.
    /// </typeparam>
    public interface ITargetDictionaryConfigSettings<TSource, TValue>
    {
        /// <summary>
        /// Construct dictionary keys for nested members using flattened member names. For example, a
        /// Person.Address.StreetName member would be mapped to a dictionary entry with key 
        /// 'AddressStreetName' when mapping from a root Person object.
        /// </summary>
        /// <returns>
        /// An ITargetDictionaryConfigSettings to enable further configuration of mappings from the source type
        /// being configured to dictionaries.
        /// </returns>
        ITargetDictionaryConfigSettings<TSource, TValue> UseFlattenedMemberNames();

        /// <summary>
        /// Use the given <paramref name="separator"/> to separate member names when mapping from nested complex 
        /// type members to dictionaries. For example, calling UseMemberName("_") will create a dictionary entry 
        /// with the key 'Address_Line1' when mapped from an Address.Line1 member.
        /// </summary>
        /// <param name="separator">
        /// The separator to use to separate member names when constructing dictionary keys for nested
        /// members.
        /// </param>
        /// <returns>
        /// An ITargetDictionaryConfigSettings to enable further configuration of mappings from the source type
        /// being configured to dictionaries.
        /// </returns>
        ITargetDictionaryConfigSettings<TSource, TValue> UseMemberNameSeparator(string separator);

        /// <summary>
        /// Use the given <paramref name="pattern"/> to create the part of a dictionary key representing an 
        /// enumerable element. The pattern must contain a single 'i' character as a placeholder for the 
        /// enumerable index. For example, calling UseElementKeyPattern("(i)") and mapping from a collection
        /// of ints to a dictionary will generate keys '(0)', '(1)', '(2)', etc.
        /// </summary>
        /// <param name="pattern">
        /// The pattern to use to create a dictionary key part representing an enumerable element.
        /// </param>
        /// <returns>
        /// An ITargetDictionaryConfigSettings to enable further configuration of mappings from the source 
        /// type being configured to dictionaries.
        /// </returns>
        ITargetDictionaryConfigSettings<TSource, TValue> UseElementKeyPattern(string pattern);

        /// <summary>
        /// Gets a link back to the full ITargetDictionaryMappingConfigurator, for api fluency.
        /// </summary>
        ITargetDictionaryMappingConfigurator<TSource, TValue> And { get; }
    }
}