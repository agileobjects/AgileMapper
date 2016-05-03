namespace AgileObjects.AgileMapper.Caching
{
    using System;
    using System.Collections.Generic;

    internal class DictionaryCache : ICache
    {
        private readonly Dictionary<object, object> _items;

        public DictionaryCache()
        {
            _items = new Dictionary<object, object>();
        }

        public TValue GetOrAdd<TKey, TValue>(TKey key, Func<TKey, TValue> valueFactory)
        {
            object value;

            if (!_items.TryGetValue(key, out value))
            {
                lock (_items)
                {
                    if (!_items.TryGetValue(key, out value))
                    {
                        _items.Add(key, (value = valueFactory.Invoke(key)));
                    }
                }
            }

            return (TValue)value;
        }
    }
}