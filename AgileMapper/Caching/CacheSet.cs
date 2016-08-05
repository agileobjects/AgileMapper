namespace AgileObjects.AgileMapper.Caching
{
    using System;

    internal class CacheSet
    {
        private readonly GlobalContext _globalContext;
        private readonly ICache<Type, object> _cachesByType;

        public CacheSet(GlobalContext globalContext)
        {
            _globalContext = globalContext;
            _cachesByType = _globalContext.CreateCache<Type, object>();
        }

        public TValue GetOrAdd<TKey, TValue>(TKey key, Func<TKey, TValue> valueFactory)
            => Get<TKey, TValue>().GetOrAdd(key, valueFactory);

        private ICache<TKey, TValue> Get<TKey, TValue>()
        {
            var cache = _cachesByType.GetOrAdd(
                typeof(ICache<TKey, TValue>),
                t => _globalContext.CreateCache<TKey, TValue>());

            return (ICache<TKey, TValue>)cache;
        }

        public void Empty()
        {
            _cachesByType.Empty();
        }
    }
}