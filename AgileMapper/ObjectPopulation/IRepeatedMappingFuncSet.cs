namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Caching;

    /// <summary>
    /// Provides mapping operations for source to target type mappings which occur multiple
    /// times within the same mapping.
    /// </summary>
    public interface IRepeatedMappingFuncSet
    {
        /// <summary>
        /// Map the source of the given <paramref name="mappingData"/> to the given 
        /// <typeparamref name="TChildTarget"/> Type.
        /// </summary>
        /// <typeparam name="TChildSource">The Type of the source object to map.</typeparam>
        /// <typeparam name="TChildTarget">The Type of the target object to which to map.</typeparam>
        /// <param name="mappingData">The mapping data containing the mapping information.</param>
        /// <param name="mappedObjectsCache">A cache containing objects mapped within the mapping so far.</param>
        /// <returns></returns>
        TChildTarget Map<TChildSource, TChildTarget>(
            IObjectMappingData<TChildSource, TChildTarget> mappingData,
            ObjectCache mappedObjectsCache);
    }
}