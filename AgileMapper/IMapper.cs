namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Api;
    using Api.Configuration;
    using Queryables.Api;

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
        /// Create and compile a mapping function for a particular type of mapping of the source type specified by 
        /// the given <paramref name="exampleInstance"/>. Use this overload for anonymous types.
        /// </summary>
        /// <typeparam name="TSource">The type of the given <paramref name="exampleInstance"/>.</typeparam>
        /// <param name="exampleInstance">
        /// An instance specifying the source type for which a mapping plan should be created.
        /// </param>
        /// <returns>
        /// An IPlanTargetAndRuleSetSelector with which to specify the type of mapping the function for which 
        /// should be cached.
        /// </returns>
        IPlanTargetAndRuleSetSelector<TSource> GetPlanFor<TSource>(TSource exampleInstance);

        /// <summary>
        /// Create and compile a mapping function for a particular type of mapping of the source type
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TSource">The source type for which to create the mapping functions.</typeparam>
        /// <returns>
        /// An IPlanTargetAndRuleSetSelector with which to specify the type of mapping the function for which 
        /// should be cached.
        /// </returns>
        IPlanTargetAndRuleSetSelector<TSource> GetPlanFor<TSource>();

        /// <summary>
        /// Create and compile a query projection function from the source IQueryable Type specified by the given 
        /// <paramref name="exampleQueryable"/>.
        /// </summary>
        /// <typeparam name="TSourceElement">
        /// The type of element contained in the source IQueryable from which the projection function to be created will project.
        /// </typeparam>
        /// <param name="exampleQueryable">
        /// An IQueryable instance specifying the source IQueryable for which a query projection mapping plan should be created.
        /// </param>
        /// <returns>
        /// An IProjectionPlanTargetSelector with which to specify the target Type to which the query projection function to 
        /// be created should be cached.
        /// </returns>
        IProjectionPlanTargetSelector<TSourceElement> GetPlanForProjecting<TSourceElement>(IQueryable<TSourceElement> exampleQueryable);

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
        /// An IPlanTargetSelector with which to specify the target type the mapping functions for which 
        /// should be cached.
        /// </returns>
        IPlanTargetSelector<TSource> GetPlansFor<TSource>(TSource exampleInstance);

        /// <summary>
        /// Create and compile mapping functions for the source type specified by the type argument, for all
        /// mapping types (create new, merge, overwrite).
        /// </summary>
        /// <typeparam name="TSource">The source type for which to create the mapping functions.</typeparam>
        /// <returns>
        /// An IPlanTargetSelector with which to specify the target type the mapping functions for which 
        /// should be cached.
        /// </returns>
        IPlanTargetSelector<TSource> GetPlansFor<TSource>();

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
        /// Throw an exception upon execution of this statement if any cached mapping plans have any target members 
        /// which will not be mapped, or map from a source enum to a target enum which does not support all of its 
        /// values. Use calls to this method to validate a mapping plan; remove them in production code.
        /// </summary>
        void ThrowNowIfAnyMappingPlanIsIncomplete();

        /// <summary>
        /// Performs a deep clone of the given <paramref name="source"/> object and returns the result.
        /// </summary>
        /// <typeparam name="TSource">The type of object for which to perform a deep clone.</typeparam>
        /// <param name="source">The object to deep clone.</param>
        /// <returns>A deep clone of the given <paramref name="source"/> object.</returns>
        TSource DeepClone<TSource>(TSource source);

        /// <summary>
        /// Perform a deep clone of the given <paramref name="source"/> object using the given 
        /// <paramref name="configurations"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of object for which to perform a deep clone.</typeparam>
        /// <param name="configurations">
        /// One or more mapping configurations. The mapping will be configured by combining these inline 
        /// <paramref name="configurations"/> with any configuration already set up via the Mapper.WhenMapping API.
        /// </param>
        /// <param name="source">The object to deep clone.</param>
        /// <returns>A deep clone of the given <paramref name="source"/> object.</returns>
        TSource DeepClone<TSource>(
            TSource source,
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TSource>>>[] configurations);

        /// <summary>
        /// Flatten the given <paramref name="source"/> object so it has only value-type or string members.
        /// </summary>
        /// <typeparam name="TSource">The type of object to flatten.</typeparam>
        /// <param name="source">The object to flatten.</param>
        /// <returns>A FlatteningTypeSelector with which to select the type of flattening to perform.</returns>
        IFlatteningSelector<TSource> Flatten<TSource>(TSource source);

        /// <summary>
        /// Perform a mapping operation on the given <paramref name="source"/> object.
        /// </summary>
        /// <typeparam name="TSource">The type of source object on which to perform the mapping.</typeparam>
        /// <param name="source">The source object on which to perform the mapping.</param>
        /// <returns>A TargetSelector with which to specify the type of mapping to perform.</returns>
        ITargetSelector<TSource> Map<TSource>(TSource source);
    }
}