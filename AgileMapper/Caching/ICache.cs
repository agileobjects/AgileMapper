namespace AgileObjects.AgileMapper.Caching
{
    using System;

    internal interface ICache
    {
        TValue GetOrAdd<TKey, TValue>(TKey key, Func<TKey, TValue> valueFactory);

        void Empty();
    }
}