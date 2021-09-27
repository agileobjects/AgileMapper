namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    /// <summary>
    /// Provides untyped services available at a particular point in a mapping.
    /// </summary>
    public interface IObjectMappingDataUntyped : IMappingData
    {
        /// <summary>
        /// Gets the <see cref="IObjectMappingDataUntyped"/> typed as an 
        /// <see cref="IObjectMappingData{TNewSource, TNewTarget}"/>.
        /// </summary>
        /// <typeparam name="TNewSource">The type of source object being mapped in the current context.</typeparam>
        /// <typeparam name="TNewTarget">The type of target object being mapped in the current context.</typeparam>
        /// <param name="isForDerivedTypeMapping">
        /// Whether the new, typed <see cref="IObjectMappingData{TNewSource, TNewTarget}"/> is needed for the creation
        /// of a derived type mapping.
        /// </param>
        /// <returns>
        /// The <see cref="IObjectMappingDataUntyped"/> typed as a 
        /// <see cref="IObjectMappingData{TNewSource, TNewTarget}"/>.
        /// </returns>
        IObjectMappingData<TNewSource, TNewTarget> As<TNewSource, TNewTarget>(bool isForDerivedTypeMapping)
            where TNewSource : class where TNewTarget : class;
    }
}