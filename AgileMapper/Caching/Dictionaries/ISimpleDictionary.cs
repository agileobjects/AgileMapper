namespace AgileObjects.AgileMapper.Caching.Dictionaries
{
    using System.Collections.Generic;

    internal interface ISimpleDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        int Count { get; }

        bool None { get; }

        bool HasOne { get; }

        IList<TKey> Keys { get; }

        IList<TValue> Values { get; }

        IEqualityComparer<TKey> Comparer { get; }

        TValue this[TKey key] { get; }

        ISimpleDictionary<TKey, TValue> Add(TKey key, TValue value);

        bool ContainsKey(TKey key);

        bool TryGetValue(TKey key, out TValue value);
    }
}