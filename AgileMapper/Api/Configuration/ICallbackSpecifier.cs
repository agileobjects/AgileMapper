namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using Members;

    /// <summary>
    /// Provides options for specifying a callback to execute during mappings from and to the source and 
    /// target types being configured.
    /// </summary>
    /// <typeparam name="TSource">The source type for which the callback should execute.</typeparam>
    /// <typeparam name="TTarget">The target type for which the callback should execute.</typeparam>
    public interface ICallbackSpecifier<TSource, TTarget>
    {
        /// <summary>
        /// Specify a callback to be executed. The condition expression is passed a context object containing 
        /// the current mapping's source and target objects.
        /// </summary>
        /// <param name="callback">The callback to execute.</param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> Call(Action<IMappingData<TSource, TTarget>> callback);

        /// <summary>
        /// Specify a callback to be executed. The condition expression is passed the current mapping's source 
        /// and target objects.
        /// </summary>
        /// <param name="callback">The callback to execute.</param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> Call(Action<TSource, TTarget> callback);

        /// <summary>
        /// Specify a callback to be executed. The condition expression is passed the current mapping's source 
        /// and target objects and the current enumerable index, if applicable.
        /// </summary>
        /// <param name="callback">The callback to execute.</param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source and 
        /// target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> Call(Action<TSource, TTarget, int?> callback);
    }
}