namespace AgileObjects.AgileMapper.Api
{
    using System;
    using System.Linq.Expressions;
    using Configuration;
    using Plans;

    /// <summary>
    /// Provides options to create and compile mapping functions for a particular type of mapping from the 
    /// source type being configured to a specified target type.
    /// </summary>
    /// <typeparam name="TSource">
    /// The type of source object from which the mapping function to be created will be performed.
    /// </typeparam>
    public interface IPlanTargetAndRuleSetSelector<TSource>
    {
        /// <summary>
        /// Create and compile mapping functions for a create new mapping from the source type being 
        /// configured to the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TResult">The type of object for which to create the mapping plan.</typeparam>
        /// <param name="configurations">
        /// Zero or more mapping configurations. If supplied, the mapping functions will be configured by 
        /// combining these inline <paramref name="configurations"/> with any configuration already set up 
        /// via the Mapper.WhenMapping API.
        /// </param>
        /// <returns>
        /// A <see cref="MappingPlan"/> object detailing the function to be executed during a mapping. To see 
        /// a string representation of the function assign the result to a string variable, or call .ToString().
        /// </returns>
        MappingPlan ToANew<TResult>(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>[] configurations);

        /// <summary>
        /// Create and compile mapping functions for an OnTo (merge) mapping from the source type being 
        /// configured to the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The type of object for which to create the mapping plan.</typeparam>
        /// <param name="configurations">
        /// Zero or more mapping configurations. If supplied, the mapping functions will be configured by 
        /// combining these inline <paramref name="configurations"/> with any configuration already set up 
        /// via the Mapper.WhenMapping API.
        /// </param>
        /// <returns>
        /// A <see cref="MappingPlan"/> object detailing the function to be executed during a mapping. To see 
        /// a string representation of the function assign the result to a string variable, or call .ToString().
        /// </returns>
        MappingPlan OnTo<TTarget>(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations);

        /// <summary>
        /// Create and compile mapping functions for an Over (overwrite) mapping from the source type being 
        /// configured to the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The type of object for which to create the mapping plan.</typeparam>
        /// <param name="configurations">
        /// Zero or more mapping configurations. If supplied, the mapping functions will be configured by 
        /// combining these inline <paramref name="configurations"/> with any configuration already set up 
        /// via the Mapper.WhenMapping API.
        /// </param>
        /// <returns>
        /// A <see cref="MappingPlan"/> object detailing the function to be executed during a mapping. To see 
        /// a string representation of the function assign the result to a string variable, or call .ToString().
        /// </returns>
        MappingPlan Over<TTarget>(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations);
    }
}