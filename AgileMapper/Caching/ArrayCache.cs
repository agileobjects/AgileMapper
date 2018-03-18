namespace AgileObjects.AgileMapper.Caching
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A lightweight, array-based cache.
    /// </summary>
    /// <typeparam name="TKey">The Type of the cache's key objects.</typeparam>
    /// <typeparam name="TValue">The Type of the cache's value objects.</typeparam>
    public class ArrayCache<TKey, TValue> : ICache<TKey, TValue>, IObjectCache<TKey, TValue>
    {
        private const int DefaultCapacity = 10;
        private readonly object _keyLock = new object();

        private TKey[] _keys;
        private TValue[] _values;
        private int _capacity;
        private int _length;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayCache{TKey, TValue}"/> class.
        /// </summary>
        public ArrayCache()
        {
            _capacity = DefaultCapacity;
            _keys = new TKey[_capacity];
            _values = new TValue[_capacity];
        }

        KeyValuePair<TKey, TValue> ICache<TKey, TValue>.this[int index]
            => new KeyValuePair<TKey, TValue>(_keys[index], _values[index]);

        int ICache<TKey, TValue>.Count => _length;

        IEnumerable<TValue> ICache<TKey, TValue>.Values
        {
            get
            {
                for (var i = 0; i < _length; i++)
                {
                    yield return _values[i];
                }
            }
        }

        TValue ICache<TKey, TValue>.GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            var currentLength = _length;

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
            if (_length == _capacity)
            {
                IncreaseCapacity();
            }

            _keys[_length] = key;
            _values[_length] = value;

            ++_length;

            return value;
        }

        private bool TryGetValue(TKey key, int startIndex, out TValue value)
        {
            if (_length == 0)
            {
                value = default(TValue);
                return false;
            }

            for (var i = startIndex; i < _length; i++)
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

            for (var i = 0; i < _length; i++)
            {
                biggerArray[i] = existingArray[i];
            }

            return biggerArray;
        }

        void ICache.Empty()
        {
            for (var i = 0; i < _length; i++)
            {
                _keys[i] = default(TKey);
                _values[i] = default(TValue);
            }

            _length = 0;
        }

        #region IObjectCache Members

        void IObjectCache<TKey, TValue>.Register(TKey source, TValue target)
            => Add(source, target);

        bool IObjectCache<TKey, TValue>.TryGet(TKey source, out TValue target)
            => TryGetValue(source, 0, out target);

        #endregion
    }
}