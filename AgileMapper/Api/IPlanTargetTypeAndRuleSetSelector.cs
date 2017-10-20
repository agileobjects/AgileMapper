namespace AgileObjects.AgileMapper.Api
{
    using Plans;

    /// <summary>
    /// Provides options to create and compile mapping functions for a particular type of mapping from the 
    /// source type being configured to a specified target type.
    /// </summary>
    public interface IPlanTargetTypeAndRuleSetSelector<TSource>
    {
        /// <summary>
        /// Create and compile mapping functions for a create new mapping from the source type being 
        /// configured to the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TResult">The type of object for which to create the mapping plan.</typeparam>
        /// <returns>
        /// A <see cref="MappingPlan{TSource,TTarget}"/> object detailing the function to be executed 
        /// during a mapping. To see a string representation of the function assign the result to a string
        /// variable, or call .ToString().
        /// </returns>
        MappingPlan<TSource, TResult> ToANew<TResult>();

        /// <summary>
        /// Create and compile mapping functions for an OnTo (merge) mapping from the source type being 
        /// configured to the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The type of object for which to create the mapping plan.</typeparam>
        /// <returns>
        /// A <see cref="MappingPlan{TSource,TTarget}"/> detailing the function to be executed during a mapping. 
        /// To see a string representation of the function, assign the result to an explitly-typed string variable, 
        /// or call .ToString().
        /// </returns>
        MappingPlan<TSource, TTarget> OnTo<TTarget>();

        /// <summary>
        /// Create and compile mapping functions for an Over (overwrite) mapping from the source type being 
        /// configured to the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The type of object for which to create the mapping plan.</typeparam>
        /// <returns>
        /// A <see cref="MappingPlan{TSource,TTarget}"/> detailing the function to be executed during a mapping. 
        /// To see a string representation of the function, assign the result to an explitly-typed string variable, 
        /// or call .ToString().
        /// </returns>
        MappingPlan<TSource, TTarget> Over<TTarget>();
    }
}