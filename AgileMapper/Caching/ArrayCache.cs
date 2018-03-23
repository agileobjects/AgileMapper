namespace AgileObjects.AgileMapper.Caching
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A lightweight, array-based cache.
    /// </summary>
    /// <typeparam name="TKey">The Type of the cache's key objects.</typeparam>
    /// <typeparam name="TValue">The Type of the cache's value objects.</typeparam>
    internal class ArrayCache<TKey, TValue> : ICache<TKey, TValue>
    {
        private const int DefaultCapacity = 10;
        private readonly object _keyLock = new object();

        private TKey[] _keys;
        private TValue[] _values;
        private int _capacity;

        public ArrayCache()
        {
            _capacity = DefaultCapacity;
            _keys = new TKey[_capacity];
            _values = new TValue[_capacity];
        }

        public KeyValuePair<TKey, TValue> this[int index]
            => new KeyValuePair<TKey, TValue>(_keys[index], _values[index]);

        public int Count { get; private set; }

        public IEnumerable<TValue> Values
        {
            get
            {
                for (var i = 0; i < Count; i++)
                {
                    yield return _values[i];
                }
            }
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            var currentLength = Count;

            if (TryGetValue(key, 0, out var value))
            {
                return value;
            }

            lock (_keyLock)
            {
                if (TryGetValue(key, currentLength, out value))
                {
                    return value;
                }

                value = Add(key, valueFactory.Invoke(key));
            }

            return value;
        }

        private TValue Add(TKey key, TValue value)
        {
            if (Count == _capacity)
            {
                IncreaseCapacity();
            }

            _keys[Count] = key;
            _values[Count] = value;

            ++Count;

            return value;
        }

        public bool TryGet(TKey source, out TValue target) => TryGetValue(source, 0, out target);

        private bool TryGetValue(TKey key, int startIndex, out TValue value)
        {
            if (Count == 0)
            {
                value = default(TValue);
                return false;
            }

            for (var i = startIndex; i < Count; i++)
            {
                var thisKey = _keys[i];

                if (ReferenceEquals(thisKey, key) || thisKey.Equals(key))
                {
                    value = _values[i];
                    return true;
                }
            }

            value = default(TValue);
            return false;
        }

        private void IncreaseCapacity()
        {
            _capacity += DefaultCapacity;
            _keys = ResizeToCapacity(_keys);
            _values = ResizeToCapacity(_values);
        }

        private T[] ResizeToCapacity<T>(IList<T> existingArray)
        {
            var biggerArray = new T[_capacity];

            for (var i = 0; i < Count; i++)
            {
                biggerArray[i] = existingArray[i];
            }

            return biggerArray;
        }

        public void Empty()
        {
            for (var i = 0; i < Count; i++)
            {
                _keys[i] = default(TKey);
                _values[i] = default(TValue);
            }

            Count = 0;
        }
    }
}