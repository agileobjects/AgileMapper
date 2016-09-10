namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    /// <summary>
    /// Provides options for configuring a condition which must evaluate to true for a configured callback 
    /// to execute during mappings from and to the source and target types being configured.
    /// </summary>
    /// <typeparam name="TSource">The source type for which the callback should execute.</typeparam>
    /// <typeparam name="TTarget">The target type for which the callback should execute.</typeparam>
    public interface IConditionalCallbackSpecifier<TSource, TTarget>
        : ICallbackSpecifier<TSource, TTarget>
    {
        /// <summary>
        /// Configure a condition which must evaluate to true for the callback to be executed. The condition
        /// expression is passed a context object containing the current mapping's source and target objects.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <returns>An ICallbackSpecifier with which to complete the configuration.</returns>
        ICallbackSpecifier<TSource, TTarget> If(
            Expression<Func<IMappingData<TSource, TTarget>, bool>> condition);

        /// <summary>
        /// Configure a condition which must evaluate to true for the callback to be executed. The condition
        /// expression is passed the current mapping's source and target objects.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <returns>An ICallbackSpecifier with which to complete the configuration.</returns>
        ICallbackSpecifier<TSource, TTarget> If(Expression<Func<TSource, TTarget, bool>> condition);

        /// <summary>
        /// Configure a condition which must evaluate to true for the callback to be executed. The condition
        /// expression is passed the current mapping's source and target objects and the current enumerable 
        /// index, if applicable.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <returns>An ICallbackSpecifier with which to complete the configuration.</returns>
        ICallbackSpecifier<TSource, TTarget> If(Expression<Func<TSource, TTarget, int?, bool>> condition);
    }
}