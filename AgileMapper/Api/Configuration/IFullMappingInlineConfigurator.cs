namespace AgileObjects.AgileMapper.Api.Configuration
{
    /// <summary>
    /// Provides options for configuring mappings from and to a given source and target type, inline.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface IFullMappingInlineConfigurator<TSource, TTarget> : IFullMappingConfigurator<TSource, TTarget>
    {
        /// <summary>
        /// Configure how this mapper performs a mapping, inline.
        /// </summary>
        MappingConfigStartingPoint WhenMapping { get; }
    }
}