namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using Members;

    /// <summary>
    /// Provides options for specifying a callback to be called before a particular type of event for mappings
    /// from and to the source and target types being configured.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    public interface IPreInstanceCreationCallbackSpecifier<TSource, TTarget>
    {
        /// <summary>
        /// Configure a callback to call in the configured conditions. The callback is passed a context 
        /// object containing the current mapping's source and target objects.
        /// </summary>
        /// <param name="callback">The callback to call.</param>
        /// <returns>
        /// A MappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        /// <returns></returns>
        IMappingConfigContinuation<TSource, TTarget> Call(Action<IMappingData<TSource, TTarget>> callback);

        /// <summary>
        /// Configure a callback to call in the configured conditions. The callback is passed the current 
        /// mapping's source and target objects.
        /// </summary>
        /// <param name="callback">The callback to call.</param>
        /// <returns>
        /// A MappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> Call(Action<TSource, TTarget> callback);

        /// <summary>
        /// Configure a callback to call in the configured conditions. The callback is passed the current 
        /// mapping's source and target objects and the current element index, if applicable.
        /// </summary>
        /// <param name="callback">The callback to call.</param>
        /// <returns>
        /// A MappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> Call(Action<TSource, TTarget, int?> callback);
    }
}