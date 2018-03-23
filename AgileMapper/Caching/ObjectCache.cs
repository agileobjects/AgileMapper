namespace AgileObjects.AgileMapper.Caching
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Caches and returns instances of mapped target objects for given source objects.
    /// </summary>
    public class ObjectCache
    {
        private readonly ICache<object, List<object>> _cachedTargetObjectsBySource;

        internal ObjectCache()
        {
            _cachedTargetObjectsBySource = GlobalContext.Instance
                .Cache
                .CreateNew<object, List<object>>();
        }

        /// <summary>
        /// Registers the given <paramref name="target"/> instance as the result of mapping the given
        /// <paramref name="source"/> object.
        /// </summary>
        /// <typeparam name="TSource">The type of source object to register.</typeparam>
        /// <typeparam name="TTarget">The type of target object to register.</typeparam>
        /// <param name="source">The <typeparamref name="TSource"/> object to register.</param>
        /// <param name="target">The <typeparamref name="TTarget"/> object to register.</param>
        public void Register<TSource, TTarget>(TSource source, TTarget target)
        {
            _cachedTargetObjectsBySource
                .GetOrAdd(source, s => new List<object>())
                .Add(target);
        }

        /// <summary>
        /// Determines if the given <paramref name="source"/> has a mapped <typeparamref name="TTarget"/>
        /// instance in the cache, returning the cached object in <paramref name="target"/> if so.
        /// </summary>
        /// <typeparam name="TSource">The type of source object for which to make the determination.</typeparam>
        /// <typeparam name="TTarget">The type of target object to find.</typeparam>
        /// <param name="source">
        /// The <typeparamref name="TSource"/> object for which to make the determination.
        /// </param>
        /// <param name="target">The cached <typeparamref name="TTarget"/> object, if one exists.</param>
        /// <returns>
        /// True if the given <paramref name="source"/> object has already been mapped to a 
        /// <typeparamref name="TTarget"/> instance, otherwise false.
        /// </returns>
        public bool TryGet<TSource, TTarget>(TSource source, out TTarget target)
            where TTarget : class
        {
            if (_cachedTargetObjectsBySource.TryGet(source, out var mappedObjects))
            {
                target = mappedObjects.OfType<TTarget>().FirstOrDefault();
                return target != default(TTarget);
            }

            target = null;
            return false;

        }
    }
}
