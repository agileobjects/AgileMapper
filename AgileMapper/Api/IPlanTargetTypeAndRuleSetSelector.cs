namespace AgileObjects.AgileMapper.Api
{
    /// <summary>
    /// Provides options to create and compile mapping functions for a particular type of mapping from the 
    /// source type being configured to a specified target type.
    /// </summary>
    public interface IPlanTargetTypeAndRuleSetSelector
    {
        /// <summary>
        /// Create and compile mapping functions for a create new mapping from the source type being 
        /// configured to the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TResult">The type of object for which to create the mapping plan.</typeparam>
        /// <returns>A string mapping plan showing the functions to be executed during a mapping.</returns>
        string ToANew<TResult>();

        /// <summary>
        /// Create and compile mapping functions for an OnTo (merge) mapping from the source type being 
        /// configured to the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The type of object for which to create the mapping plan.</typeparam>
        /// <returns>A string mapping plan showing the functions to be executed during a mapping.</returns>
        string OnTo<TTarget>();

        /// <summary>
        /// Create and compile mapping functions for an Over (overwrite) mapping from the source type being 
        /// configured to the type specified by the type argument.
        /// </summary>
        /// <typeparam name="TTarget">The type of object for which to create the mapping plan.</typeparam>
        /// <returns>A string mapping plan showing the functions to be executed during a mapping.</returns>
        string Over<TTarget>();
    }
}