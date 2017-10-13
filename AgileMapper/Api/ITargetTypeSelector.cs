﻿namespace AgileObjects.AgileMapper.Api
{
    using System;
    using Configuration;

    /// <summary>
    /// Provides options for specifying the type of mapping to perform.
    /// </summary>
    /// <typeparam name="TSource">The type of source object from which mapping is being performed.</typeparam>
    public interface ITargetTypeSelector<TSource>
    {
        /// <summary>
        /// Perform a new object mapping.
        /// </summary>
        /// <typeparam name="TResult">The type of object to create from the specified source object.</typeparam>
        /// <returns>The result of the new object mapping.</returns>
        TResult ToANew<TResult>();

        /// <summary>
        /// Perform a new object mapping using the given <paramref name="configuration"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of object to create from the specified source object.</typeparam>
        /// <param name="configuration">
        /// A mapping configuration, if required. If supplied, the mapping will be configured by the combination of
        /// this inline <paramref name="configuration"/> and any configuration set up via the Mapper.WhenMapping API.
        /// </param>
        /// <returns>The result of the new object mapping.</returns>
        TResult ToANew<TResult>(Action<IFullMappingConfigurator<TSource, TResult>> configuration);

        /// <summary>
        /// Perform an OnTo (merge) mapping.
        /// </summary>
        /// <typeparam name="TTarget">The type of object on which to perform the mapping.</typeparam>
        /// <param name="existing">The object on which to perform the mapping.</param>
        /// <returns>The mapped object.</returns>
        TTarget OnTo<TTarget>(TTarget existing);

        /// <summary>
        /// Perform an Over (overwrite) mapping.
        /// </summary>
        /// <typeparam name="TTarget">The type of object on which to perform the mapping.</typeparam>
        /// <param name="existing">The object on which to perform the mapping.</param>
        /// <returns>The mapped object.</returns>
        TTarget Over<TTarget>(TTarget existing);
    }
}
