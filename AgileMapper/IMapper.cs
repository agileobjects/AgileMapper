﻿namespace AgileObjects.AgileMapper
{
    using System;
    using Api;
    using Api.Configuration;

    /// <summary>
    /// Provides mapping and mapping configuration services.
    /// </summary>
    public interface IMapper : IDisposable
    {
        /// <summary>
        /// Creates a clone of this mapper including all user configurations.
        /// </summary>
        /// <returns>A cloned copy of this mapper.</returns>
        IMapper CloneSelf();

        /// <summary>
        /// Create and compile mapping functions for a particular type of mapping of the source type specified by 
        /// the given <paramref name="exampleInstance"/>. Use this overload for anonymous types.
        /// </summary>
        /// <typeparam name="TSource">The type of the given <paramref name="exampleInstance"/>.</typeparam>
        /// <param name="exampleInstance">
        /// An instance specifying the source type for which a mapping plan should be created.
        /// </param>
        /// <returns>
        /// An IPlanTargetTypeAndRuleSetSelector with which to specify the type of mapping the functions for which 
        /// should be cached.
        /// </returns>
        IPlanTargetTypeAndRuleSetSelector<TSource> GetPlanFor<TSource>(TSource exampleInstance);

        /// <summary>
        /// Create and compile mapping functions for a particular type of mapping of the source type
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TSource">The source type for which to create the mapping functions.</typeparam>
        /// <returns>
        /// An IPlanTargetTypeAndRuleSetSelector with which to specify the type of mapping the functions for which 
        /// should be cached.
        /// </returns>
        IPlanTargetTypeAndRuleSetSelector<TSource> GetPlanFor<TSource>();

        /// <summary>
        /// Create and compile mapping functions for mapping from the source type specified by the given 
        /// <paramref name="exampleInstance"/>, for all mapping types (create new, merge, overwrite). Use this 
        /// overload for anonymous types.
        /// </summary>
        /// <typeparam name="TSource">The source type for which to create the mapping functions.</typeparam>
        /// <param name="exampleInstance">
        /// An instance specifying the source type for which a mapping plan should be created.
        /// </param>
        /// <returns>
        /// An IPlanTargetTypeSelector with which to specify the target type the mapping functions for which 
        /// should be cached.
        /// </returns>
        IPlanTargetTypeSelector GetPlansFor<TSource>(TSource exampleInstance);

        /// <summary>
        /// Create and compile mapping functions for the source type specified by the type argument, for all
        /// mapping types (create new, merge, overwrite).
        /// </summary>
        /// <typeparam name="TSource">The source type for which to create the mapping functions.</typeparam>
        /// <returns>
        /// An IPlanTargetTypeSelector with which to specify the target type the mapping functions for which 
        /// should be cached.
        /// </returns>
        IPlanTargetTypeSelector GetPlansFor<TSource>();

        /// <summary>
        /// Returns mapping plans for all mapping functions currently cached by the <see cref="IMapper"/>.
        /// </summary>
        /// <returns>A string containing the currently-cached functions to be executed during mappings.</returns>
        string GetPlansInCache();

        /// <summary>
        /// Configure callbacks to be executed before a particular type of event occurs for all source
        /// and target types.
        /// </summary>
        PreEventConfigStartingPoint Before { get; }

        /// <summary>
        /// Configure callbacks to be executed after a particular type of event occurs for all source
        /// and target types.
        /// </summary>
        PostEventConfigStartingPoint After { get; }

        /// <summary>
        /// Configure how this mapper performs a mapping.
        /// </summary>
        MappingConfigStartingPoint WhenMapping { get; }

        /// <summary>
        /// Performs a deep clone of the given <paramref name="source"/> object and returns the result.
        /// </summary>
        /// <typeparam name="TSource">The type of object for which to perform a deep clone.</typeparam>
        /// <param name="source">The object to deep clone.</param>
        /// <returns>A deep clone of the given <paramref name="source"/> object.</returns>
        TSource Clone<TSource>(TSource source) where TSource : class;

        /// <summary>
        /// Flattens the given <paramref name="source"/> object so it has only value-type or string members
        /// and returns the result.
        /// </summary>
        /// <typeparam name="TSource">The type of object to flatten.</typeparam>
        /// <param name="source">The object to flatten.</param>
        /// <returns>
        /// A dynamic object containing flattened versions of the given <paramref name="source"/> object's 
        /// properties.
        /// </returns>
        dynamic Flatten<TSource>(TSource source) where TSource : class;

        /// <summary>
        /// Perform a mapping operation on the given <paramref name="source"/> object.
        /// </summary>
        /// <typeparam name="TSource">The type of source object on which to perform the mapping.</typeparam>
        /// <param name="source">The source object on which to perform the mapping.</param>
        /// <returns>A TargetTypeSelector with which to specify the type of mapping to perform.</returns>
        ITargetTypeSelector Map<TSource>(TSource source);
    }
}