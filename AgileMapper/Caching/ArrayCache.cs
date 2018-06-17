namespace AgileObjects.AgileMapper.Caching
{
    using System;
    using System.Collections.Generic;

    internal class ArrayCache<TKey, TValue> : ICache<TKey, TValue>
    {
        private const int DefaultCapacity = 10;
        private readonly object _keyLock = new object();
        private readonly IEqualityComparer<TKey> _keyComparer;

        private TKey[] _keys;
        private TValue[] _values;
        private int _capacity;
        private int _length;

        public ArrayCache(IEqualityComparer<TKey> keyComparer)
        {
            _capacity = DefaultCapacity;
            _length = 0;
            _keys = new TKey[DefaultCapacity];
            _values = new TValue[DefaultCapacity];
            _keyComparer = keyComparer ?? default(DefaultComparer<TKey>);
        }

        KeyValuePair<TKey, TValue> ICache<TKey, TValue>.this[int index]
            => new KeyValuePair<TKey, TValue>(_keys[index], _values[index]);

        int ICache<TKey, TValue>.Count => _length;

        public IEnumerable<TValue> Values
        {
            get
            {
                for (var i = 0; i < _length; i++)
                {
                    yield return _values[i];
                }
            }
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
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

                value = valueFactory.Invoke(key);

                EnsureCapacity();

                _keys[_length] = key;
                _values[_length] = value;

                ++_length;
            }

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

                if (_keyComparer.Equals(thisKey, key))
                {
                    value = _values[i];
                    return true;
                }
            }

            value = default(TValue);
            return false;
        }

        private void EnsureCapacity()
        {
            if (_length < _capacity)
            {
                return;
            }

            _capacity += DefaultCapacity;
            _keys = ResizeToCapacity(_keys);
            _values = ResizeToCapacity(_values);
        }

        private T[] ResizeToCapacity<T>(T[] existingArray)
        {
            var biggerArray = new T[_capacity];

            for (var i = 0; i < _length; i++)
            {
                biggerArray[i] = existingArray[i];
            }

            return biggerArray;
        }

        public void Empty()
        {
            for (var i = 0; i < _length; i++)
            {
                _keys[i] = default(TKey);
                _values[i] = default(TValue);
            }

            _length = 0;
        }
    }
}