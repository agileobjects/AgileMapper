namespace AgileObjects.AgileMapper.Api
{
    /// <summary>
    /// Provides the option to create and compile mapping functions for mappings from the source type 
    /// being configured to a specified target type, for all mapping types (create new, merge, overwrite).
    /// </summary>
    public interface IPlanTargetTypeSelector
    {
        /// <summary>
        /// Create and compile mapping functions from the source type being configured to the type specified 
        /// by the type argument, for all mapping types (create new, merge, overwrite).
        /// </summary>
        /// <typeparam name="TTarget">The type of object for which to create the mapping plans.</typeparam>
        /// <returns>A set of string mapping plans showing the functions to be executed during a mapping.</returns>
        string To<TTarget>();
    }
}