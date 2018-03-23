﻿namespace AgileObjects.AgileMapper.Caching
{
    using System;
    using System.Collections.Generic;

    internal interface ICache
    {
        void Empty();
    }

    internal interface ICache<TKey, TValue> : ICache
    {
        KeyValuePair<TKey, TValue> this[int index] { get; }

        int Count { get; }

        IEnumerable<TValue> Values { get; }

        bool TryGet(TKey key, out TValue value);

        TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory);
    }
}