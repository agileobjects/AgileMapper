namespace AgileObjects.AgileMapper.Api
{
    using Plans;

    /// <summary>
    /// Provides the option to create and compile mapping functions for mappings from the source type 
    /// being configured to a specified target type, for all mapping types (create new, merge, overwrite).
    /// </summary>
    public interface IPlanTargetTypeSelector<TSource>
    {
        /// <summary>
        /// Create and compile mapping functions from the source type being configured to the type specified 
        /// by the type argument, for all mapping types (create new, merge, overwrite).
        /// </summary>
        /// <typeparam name="TTarget">The type of object for which to create the mapping plans.</typeparam>
        /// <returns>
        /// A <see cref="MappingPlanSet{TSource,TTarget}"/> object detailing the functions to be executed 
        /// during a mapping. To see a string representation of the function assign the result to a string
        /// variable, or call .ToString().
        /// </returns>
        MappingPlanSet<TSource, TTarget> To<TTarget>();
    }
}