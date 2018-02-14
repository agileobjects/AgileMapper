namespace AgileObjects.AgileMapper.Api.Configuration.Projection
{
    using System;
    using AgileMapper.Configuration;

    /// <summary>
    /// Provides options for configuring how a mapper performs a query projection.
    /// </summary>
    public interface IProjectionConfigStartingPoint
    {
        /// <summary>
        /// Configure a formatting string to use when projecting from the given <typeparamref name="TSourceValue"/>
        /// to strings, for all source and result Types. The configured formatting string will have to be supported
        /// by the QueryProvider, and may be ignored if it is not.
        /// </summary>
        /// <typeparam name="TSourceValue">The source value type to which to apply a formatting string.</typeparam>
        /// <param name="formatSelector">An action which supplies the formatting string.</param>
        /// <returns>
        /// An <see cref="IGlobalProjectionSettings"/> with which to globally configure other query projection 
        /// aspects.
        /// </returns>
        IGlobalProjectionSettings StringsFrom<TSourceValue>(Action<StringFormatSpecifier> formatSelector);

        /// <summary>
        /// Configure how this mapper performs projections from the <typeparamref name="TSource"/> Type.
        /// </summary>
        /// <typeparam name="TSource">The source Type to which the configuration will apply.</typeparam>
        /// <returns>
        /// An IProjectionResultSelector with which to specify to which result Type the configuration 
        /// will apply.
        /// </returns>
        IProjectionResultSelector<TSource> From<TSource>() where TSource : class;

        /// <summary>
        /// Configure how this mapper performs query projection mappings from any source type to the
        /// <typeparamref name="TResult"/> Type.
        /// </summary>
        /// <typeparam name="TResult">The result Type to which the configuration will apply.</typeparam>
        /// <returns>An IFullProjectionConfigurator with which to complete the configuration.</returns>
        IFullProjectionConfigurator<object, TResult> ProjectionsTo<TResult>();
    }
}