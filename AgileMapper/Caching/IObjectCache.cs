namespace AgileObjects.AgileMapper.Caching
{
    /// <summary>
    /// Caches and returns instances of mapped target objects for given source objects.
    /// </summary>
    /// <typeparam name="TSource">The type of source object to be cached.</typeparam>
    /// <typeparam name="TTarget">The type of target object to be cached.</typeparam>
    public interface IObjectCache<in TSource, TTarget>
    {
        /// <summary>
        /// Registers the given <paramref name="target"/> instance as the result of mapping the given
        /// <paramref name="source"/> object.
        /// </summary>
        /// <param name="source">The <typeparamref name="TSource"/> object to register.</param>
        /// <param name="target">The <typeparamref name="TTarget"/> object to register.</param>
        void Register(TSource source, TTarget target);

        /// <summary>
        /// Determines if the given <paramref name="source"/> has a mapped <typeparamref name="TTarget"/>
        /// instance in the cache, returning the cached object in <paramref name="target"/> if so.
        /// </summary>
        /// <param name="source">
        /// The <typeparamref name="TSource"/> object for which to make the determination.
        /// </param>
        /// <param name="target">The cached <typeparamref name="TTarget"/> object, if one exists.</param>
        /// <returns>
        /// True if the given <paramref name="source"/> object has already been mapped to a 
        /// <typeparamref name="TTarget"/> instance, otherwise false.
        /// </returns>
        bool TryGet(TSource source, out TTarget target);
    }
}
