namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using ObjectPopulation;

    /// <summary>
    /// Provides options to configure the execution of a callback after a particular type of event for mappings
    /// from and to the source and target types being configured.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    /// <typeparam name="TObject">The type of created object to which the configuration should apply.</typeparam>
    public interface IConditionalPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>
        : IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject>
    {
        /// <summary>
        /// Configure a condition which must evaluate to true for the configuration to apply. The condition
        /// expression is passed a context object containing the current mapping's source, target and created 
        /// objects.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <returns>An IPostInstanceCreationCallbackSpecifier with which to complete the configuration.</returns>
        IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<IObjectCreationMappingData<TSource, TTarget, TObject>, bool>> condition);

        /// <summary>
        /// Configure a condition which must evaluate to true for the configuration to apply. The condition
        /// expression is passed the current mapping's source and target objects.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <returns>An IPostInstanceCreationCallbackSpecifier with which to complete the configuration.</returns>
        IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<TSource, TTarget, bool>> condition);

        /// <summary>
        /// Configure a condition which must evaluate to true for the configuration to apply. The condition
        /// expression is passed the current mapping's source, target and created objects.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <returns>An IPostInstanceCreationCallbackSpecifier with which to complete the configuration.</returns>
        IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<TSource, TTarget, TObject, bool>> condition);

        /// <summary>
        /// Configure a condition which must evaluate to true for the configuration to apply. The condition
        /// expression is passed the current mapping's source, target and created objects and the current 
        /// enumerable index, if applicable.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <returns>An IPostInstanceCreationCallbackSpecifier with which to complete the configuration.</returns>
        IPostInstanceCreationCallbackSpecifier<TSource, TTarget, TObject> If(
            Expression<Func<TSource, TTarget, TObject, int?, bool>> condition);
    }
}