namespace AgileObjects.AgileMapper.Caching
{
    using System;

    internal class CacheSet
    {
        private readonly ICache<Type, ICache> _cachesByType;

        public CacheSet()
        {
            _cachesByType = CreateNew<Type, ICache>();
        }

        public TValue GetOrAdd<TKey, TValue>(TKey key, Func<TKey, TValue> valueFactory)
            => CreateScoped<TKey, TValue>().GetOrAdd(key, valueFactory);

        public ICache<TKey, TValue> CreateScoped<TKey, TValue>()
        {
            var cache = _cachesByType.GetOrAdd(
                typeof(ICache<TKey, TValue>),
                t => CreateNew<TKey, TValue>());

            return (ICache<TKey, TValue>)cache;
        }

        public ICache<TKey, TValue> CreateNew<TKey, TValue>() => new ArrayCache<TKey, TValue>();

        //public void CloneTo(CacheSet cacheSet) => _cachesByType.CloneTo(cacheSet._cachesByType);

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