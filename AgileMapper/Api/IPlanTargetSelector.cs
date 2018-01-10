namespace AgileObjects.AgileMapper.Api
{
    using System;
    using System.Linq.Expressions;
    using Configuration;
    using Plans;

    /// <summary>
    /// Provides the option to create and compile mapping functions for mappings from the source type 
    /// being configured to a specified target type, for all mapping types (create new, merge, overwrite).
    /// </summary>
    /// <typeparam name="TSource">
    /// The type of source object from which the mapping function to be created will be performed.
    /// </typeparam>
    public interface IPlanTargetSelector<TSource>
    {
        /// <summary>
        /// Create and compile mapping functions from the source type being configured to the type specified 
        /// by the type argument, for all mapping types (create new, merge, overwrite).
        /// </summary>
        /// <typeparam name="TTarget">The type of object for which to create the mapping plans.</typeparam>
        /// <returns>
        /// A <see cref="MappingPlanSet"/> detailing the functions to be executed during a mapping. To see 
        /// string representations of the functions, assign the result to an explicitly-typed string variable, 
        /// or call .ToString().
        /// </returns>
        MappingPlanSet To<TTarget>();

        /// <summary>
        /// Create and compile mapping functions from the source type being configured to the type specified 
        /// by the type argument, for all mapping types (create new, merge, overwrite).
        /// </summary>
        /// <typeparam name="TTarget">The type of object for which to create the mapping plans.</typeparam>
        /// <param name="configurations">
        /// One or more mapping configurations. The mapping functions will be configured by combining these inline 
        /// <paramref name="configurations"/> with any configuration already set up via the Mapper.WhenMapping API.
        /// </param>
        /// <returns>
        /// A <see cref="MappingPlanSet"/> detailing the functions to be executed during a mapping. To see 
        /// string representations of the functions, assign the result to an explicitly-typed string variable, 
        /// or call .ToString().
        /// </returns>
        MappingPlanSet To<TTarget>(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations);
    }
}