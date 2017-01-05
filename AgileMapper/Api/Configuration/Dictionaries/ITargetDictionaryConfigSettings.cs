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
        ITargetDictionaryConfigSettings<TSource, TValue> UseFlattenedMemberNames();

        /// <summary>
        /// Gets a link back to the full ITargetDictionaryMappingConfigurator, for api fluency.
        /// </summary>
        ITargetDictionaryMappingConfigurator<TSource, TValue> And { get; }
    }
}