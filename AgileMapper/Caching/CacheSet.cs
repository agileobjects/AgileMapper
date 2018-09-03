namespace AgileObjects.AgileMapper.Caching
{
    using System;

    internal class CacheSet
    {
        private readonly ICache<Type, ICache> _cachesByType;

        public CacheSet()
        {
            _cachesByType = CreateNew<Type, ICache>(default(HashCodeComparer<Type>));
        }

        public TValue GetOrAdd<TKey, TValue>(
            TKey key,
            Func<TKey, TValue> valueFactory,
            IKeyComparer<TKey> keyComparer = null)
        {
            return CreateScoped<TKey, TValue>(keyComparer).GetOrAdd(key, valueFactory);
        }

        public ICache<TKey, TValue> CreateScoped<TKey, TValue>(IKeyComparer<TKey> keyComparer = null)
        {
            var cache = _cachesByType.GetOrAdd(
                typeof(ICache<TKey, TValue>),
                t => CreateNew<TKey, TValue>(keyComparer));

            return (ICache<TKey, TValue>)cache;
        }

        public ICache<TKey, TValue> CreateNew<TKey, TValue>(IKeyComparer<TKey> keyComparer = null)
            => new ArrayCache<TKey, TValue>(keyComparer);

        public void Empty()
        {
            foreach (var cache in _cachesByType.Values)
            {
                cache.Empty();
            }

            _cachesByType.Empty();
        }
    }
}