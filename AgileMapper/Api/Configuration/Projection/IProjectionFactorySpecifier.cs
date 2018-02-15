namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides an option for configuring a custom factory Expression with which to create instances of the 
    /// <typeparamref name="TObject"/> Type, when projecting from and to the source and result types being
    /// configured.
    /// </summary>
    /// <typeparam name="TSourceElement">The source Type to which the configuration should apply.</typeparam>
    /// <typeparam name="TResultElement">The result Type to which the configuration should apply.</typeparam>
    /// <typeparam name="TObject">The Type of object which will be created by the configured factory.</typeparam>
    public interface IProjectionFactorySpecifier<TSourceElement, TResultElement, TObject>
    {
        /// <summary>
        /// Use the given <paramref name="factory"/> expression to create instances of the object type being 
        /// configured. The factory expression is passed the source element being projected, and must be 
        /// translatable by the QueryProvider being used.
        /// </summary>
        /// <param name="factory">
        /// The factory expression to use to create instances of the type being configured.
        /// </param>
        /// <returns>
        /// An IProjectionConfigContinuation to enable further configuration of projections from and to the 
        /// source and result Type being configured.
        /// </returns>
        IProjectionConfigContinuation<TSourceElement, TResultElement> Using(Expression<Func<TSourceElement, TObject>> factory);
    }
}