namespace AgileObjects.AgileMapper.Caching
{
    using System;
    using System.Collections.Generic;

    internal class CacheSet
    {
        private readonly ICache<Type, ICache> _cachesByType;

        public CacheSet()
        {
            _cachesByType = CreateNew<Type, ICache>(default(ReferenceEqualsComparer<Type>));
        }

        public TValue GetOrAdd<TKey, TValue>(TKey key, Func<TKey, TValue> valueFactory)
            => CreateScoped<TKey, TValue>().GetOrAdd(key, valueFactory);

        public ICache<TKey, TValue> CreateScoped<TKey, TValue>(IEqualityComparer<TKey> keyComparer = null)
        {
            var cache = _cachesByType.GetOrAdd(
                typeof(ICache<TKey, TValue>),
                t => CreateNew<TKey, TValue>(keyComparer));

            return (ICache<TKey, TValue>)cache;
        }

        public ICache<TKey, TValue> CreateNew<TKey, TValue>(IEqualityComparer<TKey> keyComparer = null)
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