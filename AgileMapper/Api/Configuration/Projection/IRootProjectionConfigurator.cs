namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    /// <summary>
    /// Provides options for configuring projections from and to a given source and result type.
    /// </summary>
    /// <typeparam name="TSourceElement">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TResultElement">The result type to which the configuration should apply.</typeparam>
    public interface IRootProjectionConfigurator<TSourceElement, TResultElement>
    {
        /// <summary>
        /// Configure a constant value for a particular target member when projecting from and to the source and 
        /// result types being configured.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of the custom constant value being configured.</typeparam>
        /// <param name="value">The constant value to map to the configured result member.</param>
        /// <returns>
        /// A CustomDataSourceTargetMemberSpecifier with which to specify the result member to which the custom 
        /// constant value should be applied.
        /// </returns>
        CustomDataSourceTargetMemberSpecifier<TSourceElement, TResultElement> Map<TSourceValue>(TSourceValue value);
    }
}