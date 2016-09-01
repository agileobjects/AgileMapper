namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using ObjectPopulation;

    /// <summary>
    /// Provides options for specifying a callback to be called after a particular type of event for mappings
    /// from and to the source and target types being configured.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    /// <typeparam name="TObject">The type of created object to which the configuration should apply.</typeparam>
    public interface IPostInstanceCreationCallbackSpecifier<TSource, TTarget, out TObject>
    {
        /// <summary>
        /// Configure a callback to call in the configured conditions. The callback is passed a context 
        /// object containing the current mapping's source, target and created objects.
        /// </summary>
        /// <param name="callback">The callback to call.</param>
        /// <returns>
        /// A MappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        MappingConfigContinuation<TSource, TTarget> Call(
            Action<IObjectCreationMappingData<TSource, TTarget, TObject>> callback);

        /// <summary>
        /// Configure a callback to call in the configured conditions. The callback is passed the current 
        /// mapping's source and target objects.
        /// </summary>
        /// <param name="callback">The callback to call.</param>
        /// <returns>
        /// A MappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        MappingConfigContinuation<TSource, TTarget> Call(Action<TSource, TTarget> callback);

        /// <summary>
        /// Configure a callback to call in the configured conditions. The callback is passed the current 
        /// mapping's source and target objects and the current enumerable index, if applicable.
        /// </summary>
        /// <param name="callback">The callback to call.</param>
        /// <returns>
        /// A MappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        MappingConfigContinuation<TSource, TTarget> Call(Action<TSource, TTarget, TObject> callback);

        /// <summary>
        /// Configure a callback to call in the configured conditions. The callback is passed the current 
        /// mapping's source, target and created objects and the current enumerable index, if applicable.
        /// </summary>
        /// <param name="callback">The callback to call.</param>
        /// <returns>
        /// A MappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        MappingConfigContinuation<TSource, TTarget> Call(Action<TSource, TTarget, TObject, int?> callback);
    }
}