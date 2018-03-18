namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    /// <summary>
    /// Provides a bridge to creating a typed mapping data object.
    /// </summary>
    public interface IObjectMappingDataUntyped : IMappingData
    {
        /// <summary>
        /// Gets the <see cref="IObjectMappingDataUntyped"/> typed as a 
        /// <see cref="IObjectMappingData{TNewSource, TNewTarget}"/>.
        /// </summary>
        /// <typeparam name="TNewSource">The type of source object being mapped in the current context.</typeparam>
        /// <typeparam name="TNewTarget">The type of target object being mapped in the current context.</typeparam>
        /// <returns>
        /// The <see cref="IObjectMappingDataUntyped"/> typed as a 
        /// <see cref="IObjectMappingData{TNewSource, TNewTarget}"/>.
        /// </returns>
        new IObjectMappingData<TNewSource, TNewTarget> As<TNewSource, TNewTarget>()
            where TNewSource : class where TNewTarget : class;
    }
}