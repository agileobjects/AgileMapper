namespace AgileObjects.AgileMapper.Caching
{
    using System;
    using System.Collections.Generic;

    internal interface ICache
    {
        void Empty();
    }

    internal interface ICache<TKey, TValue> : ICache
    {
        IEnumerable<TValue> Values { get; }

        TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory);

        //void CloneTo(ICache<TKey, TValue> otherCache);
    }
}