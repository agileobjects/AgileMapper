namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    /// <summary>
    /// Provides options for configuring mappings from a <typeparamref name="TSource"/> to a 
    /// Dictionary{string, <typeparamref name="TValue"/>}, inline.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TValue">
    /// The type of values stored in the Dictionary to which the configurations will apply.
    /// </typeparam>
    public interface ITargetDictionaryMappingInlineConfigurator<TSource, TValue> :
        ITargetDictionaryMappingConfigurator<TSource, TValue>
    {
    }
}