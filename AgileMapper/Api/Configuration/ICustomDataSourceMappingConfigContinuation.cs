namespace AgileObjects.AgileMapper.Api.Configuration
{
    using AgileMapper.Configuration;

    /// <summary>
    /// Enables chaining of configurations for the same source and target type, with options specific to custom
    /// data source configuration.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface ICustomDataSourceMappingConfigContinuation<TSource, TTarget>
        : IMappingConfigContinuation<TSource, TTarget>
    {
        /// <summary>
        /// Apply the reverse of the configured data source. For example, ProductDto.ProdId -> Product.Id will
        /// also apply Product.Id -> ProductDto.ProdId. Throws a <see cref="MappingConfigurationException"/> if
        /// reverse configured data sources have been enabled globally.
        /// </summary>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source 
        /// and target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> AndViceVersa();

        /// <summary>
        /// Do not apply the reverse of the configured data source. For example, ProductDto.ProdId -> Product.Id will
        /// not also apply Product.Id -> ProductDto.ProdId. Use this method to override this configuration instance if
        /// the globally-configured default has been changed; a <see cref="MappingConfigurationException"/> is thrown
        /// if the default behaviour has not been changed.
        /// </summary>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source 
        /// and target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> ButNotViceVersa();
    }
}