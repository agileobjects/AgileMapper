namespace AgileObjects.AgileMapper.Api.Configuration
{
    /// <summary>
    /// Provides options for configuring mappings from and to a given source and target type.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface IFullMappingConfigurator<TSource, TTarget> 
        : IFullMappingSettings<TSource, TTarget>,
            IFullMappingConfigStartingPoint<TSource, TTarget>,
            IFullMappingNamingSettings<IFullMappingConfigurator<TSource, TTarget>>
    {
    }
}