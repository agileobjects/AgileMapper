namespace AgileObjects.AgileMapper.Caching
{
    using System;
    using System.Collections.Generic;

    internal class DictionaryCache<TKey, TValue> : ICache<TKey, TValue>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly object _itemsLock = new object();

        private readonly Dictionary<TKey, TValue> _items;

        public DictionaryCache()
        {
            _items = new Dictionary<TKey, TValue>();
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue value;

            // ReSharper disable once InconsistentlySynchronizedField
            if (_items.TryGetValue(key, out value))
            {
                return value;
            }

            lock (_itemsLock)
            {
                if (!_items.TryGetValue(key, out value))
                {
                    _items.Add(key, (value = valueFactory.Invoke(key)));
                }
            }

            return value;
        }

        public void Empty()
        {
            _items.Clear();
        }
    }
}