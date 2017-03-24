namespace AgileObjects.AgileMapper.Api
{
    /// <summary>
    /// Provides options for specifying the type of mapping to perform.
    /// </summary>
    public interface ITargetTypeSelector
    {
        /// <summary>
        /// Perform a new object mapping.
        /// </summary>
        /// <typeparam name="TResult">The type of object to create from the specified source object.</typeparam>
        /// <returns>The result of the new object mapping.</returns>
        TResult ToANew<TResult>();

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
