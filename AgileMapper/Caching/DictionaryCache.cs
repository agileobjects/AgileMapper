namespace AgileObjects.AgileMapper.Caching
{
    using System;
    using System.Collections.Generic;

    internal class DictionaryCache : ICache
    {
        private static readonly object _itemsLock = new object();

        private readonly Dictionary<object, object> _items;

        public DictionaryCache()
        {
            _items = new Dictionary<object, object>();
        }

        public TValue GetOrAdd<TKey, TValue>(TKey key, Func<TKey, TValue> valueFactory)
        {
            object value;

            // ReSharper disable once InconsistentlySynchronizedField
            if (_items.TryGetValue(key, out value))
            {
                return (TValue)value;
            }

            lock (_itemsLock)
            {
                if (!_items.TryGetValue(key, out value))
                {
                    _items.Add(key, (value = valueFactory.Invoke(key)));
                }
            }

            return (TValue)value;
        }

        public void Empty()
        {
            _items.Clear();
        }
    }
}