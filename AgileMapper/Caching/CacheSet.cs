namespace AgileObjects.AgileMapper.Caching
{
    using System;

    internal class CacheSet
    {
        private readonly ICache<Type, ICache> _cachesByType;

        public CacheSet()
        {
            _cachesByType = CreateCache<Type, ICache>();
        }

        public TValue GetOrAdd<TKey, TValue>(TKey key, Func<TKey, TValue> valueFactory)
            => Create<TKey, TValue>().GetOrAdd(key, valueFactory);

        public ICache<TKey, TValue> Create<TKey, TValue>()
        {
            var cache = _cachesByType.GetOrAdd(
                typeof(ICache<TKey, TValue>),
                t => CreateCache<TKey, TValue>());

            return (ICache<TKey, TValue>)cache;
        }

        private static ICache<TKey, TValue> CreateCache<TKey, TValue>() => new DictionaryCache<TKey, TValue>();

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