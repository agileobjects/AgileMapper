namespace AgileObjects.AgileMapper.Caching
{
    using System;

    internal class CacheSet
    {
        private readonly ICache<Type, ICache> _cachesByType;

        public CacheSet()
        {
            _cachesByType = CreateNewWithHashCodes<Type, ICache>();
        }

        public TValue GetOrAdd<TKey, TValue>(
            TKey key,
            Func<TKey, TValue> valueFactory,
            KeyComparer<TKey> keyComparer = null)
        {
            return CreateScoped<TKey, TValue>(keyComparer).GetOrAdd(key, valueFactory);
        }

        public TValue GetOrAddWithHashCodes<TKey, TValue>(
            TKey key,
            Func<TKey, TValue> valueFactory)
        {
            return CreateScopedWithHashCodes<TKey, TValue>().GetOrAdd(key, valueFactory);
        }

        public ICache<TKey, TValue> CreateScoped<TKey, TValue>(KeyComparer<TKey> keyComparer = null)
            => CreateScoped<TKey, TValue>(_ => CreateNew<TKey, TValue>(keyComparer));

        public ICache<TKey, TValue> CreateScopedWithHashCodes<TKey, TValue>()
            => CreateScoped<TKey, TValue>(_ => CreateNewWithHashCodes<TKey, TValue>());

        private ICache<TKey, TValue> CreateScoped<TKey, TValue>(
            Func<Type, ICache> cacheFactory)
        {
            var cache = _cachesByType.GetOrAdd(typeof(ICache<TKey, TValue>), cacheFactory);
            return (ICache<TKey, TValue>)cache;
        }

        public ICache<TKey, TValue> CreateNew<TKey, TValue>(KeyComparer<TKey> keyComparer = null)
            => new DefaultArrayCache<TKey, TValue>(keyComparer);

        public ICache<TKey, TValue> CreateNewWithHashCodes<TKey, TValue>()
            => new HashCodeArrayCache<TKey, TValue>();

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