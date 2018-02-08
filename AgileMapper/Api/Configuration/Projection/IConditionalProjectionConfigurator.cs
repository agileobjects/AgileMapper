namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides options for configuring a condition which must evaluate to true for the configuration to apply
    /// to mappings from and to the source and result types being configured.
    /// </summary>
    /// <typeparam name="TSourceElement">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TResultElement">The result type to which the configuration should apply.</typeparam>
    public interface IConditionalProjectionConfigurator<TSourceElement, TResultElement>
        : IRootProjectionConfigurator<TSourceElement, TResultElement>
    {
        /// <summary>
        /// Configure a condition which must evaluate to true for the configuration to apply. The condition
        /// expression is passed the source element being projected.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <returns>An IConditionalRootProjectionConfigurator with which to complete the configuration.</returns>
        IConditionalRootProjectionConfigurator<TSourceElement, TResultElement> If(
            Expression<Func<TSourceElement, bool>> condition);
    }
}