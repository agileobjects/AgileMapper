namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    /// <summary>
    /// Provides options for configuring custom factory objects with which to create instances of the 
    /// <typeparamref name="TObject"/> Type, when mapping from and to the given source and target types.
    /// </summary>
    /// <typeparam name="TSource">The source type to which the configuration should apply.</typeparam>
    /// <typeparam name="TTarget">The target type to which the configuration should apply.</typeparam>
    /// <typeparam name="TObject">The type of object which will be created by the configured factories.</typeparam>
    public interface IMappingFactorySpecifier<TSource, TTarget, TObject>
    {
        /// <summary>
        /// Use the given <paramref name="factory"/> expression to create instances of the object type being 
        /// configured. The factory expression is passed a context object containing the current mapping's source 
        /// and target objects.
        /// </summary>
        /// <param name="factory">
        /// The factory expression to use to create instances of the type being configured.
        /// </param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source 
        /// and target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> Using(Expression<Func<IMappingData<TSource, TTarget>, TObject>> factory);

        /// <summary>
        /// Use the given <paramref name="factory"/> function to create instances of the object type being 
        /// configured. The following factory function signatures are supported:
        /// <para>
        /// Func&lt;TObject&gt; - parameterless.
        /// </para>
        /// <para>
        /// Func&lt;IMappingData&lt;TSource, TTarget&gt;, TObject&gt; - taking a context object containing the 
        /// current mapping's source and target objects.
        /// </para>
        /// <para>
        /// Func&lt;TSource, TTarget, TObject&gt; - taking the source and target objects.
        /// </para>
        /// <para>
        /// Func&lt;TSource, TTarget, int?, TObject&gt; - taking the source and target objects and the current 
        /// enumerable index, if applicable.
        /// </para>
        /// </summary>
        /// <param name="factory">
        /// The factory function to use to create instances of the type being configured.
        /// </param>
        /// <returns>
        /// An IMappingConfigContinuation to enable further configuration of mappings from and to the source 
        /// and target type being configured.
        /// </returns>
        IMappingConfigContinuation<TSource, TTarget> Using<TFactory>(TFactory factory)
            where TFactory : class;
    }
}