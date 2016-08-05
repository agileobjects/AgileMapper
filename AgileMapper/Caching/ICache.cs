namespace AgileObjects.AgileMapper.Caching
{
    using System;

    internal interface ICache<TKey, TValue>
    {
        TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory);

        void Empty();
    }
}