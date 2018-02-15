namespace AgileObjects.AgileMapper.Queryables.Api
{
    using Plans;

    /// <summary>
    /// Provides options to create and compile a query projection function from the source type being configured 
    /// to a specified target type.
    /// </summary>
    /// <typeparam name="TSourceElement">
    /// The type of element contained in the source IQueryable from which the projection function to be created will project.
    /// </typeparam>
    // ReSharper disable once UnusedTypeParameter
    public interface IProjectionPlanTargetSelector<TSourceElement>
    {
        /// <summary>
        /// Create and compile a query projection function from the source type being configured to the type specified 
        /// by the type argument.
        /// </summary>
        /// <typeparam name="TResult">The type of target object for which to create the query projection mapping plan.</typeparam>
        /// <returns>
        /// A <see cref="MappingPlan"/> object detailing the function to be executed during a query projection, using 
        /// the given source IQueryable. To see a string representation of the function assign the result to a string 
        /// variable, or call .ToString().
        /// </returns>
        MappingPlan To<TResult>();
    }
}