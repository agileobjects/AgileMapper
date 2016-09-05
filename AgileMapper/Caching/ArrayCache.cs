namespace AgileObjects.AgileMapper.Caching
{
    using System;
    using System.Collections.Generic;

    internal class ArrayCache<TKey, TValue> : ICache<TKey, TValue>
    {
        private const int DefaultCapacity = 10;
        private readonly object _keyLock = new object();

        private TKey[] _keys;
        private TValue[] _values;
        private int _capacity;
        private int _length;

        public ArrayCache()
        {
            _capacity = DefaultCapacity;
            _length = 0;
            _keys = new TKey[DefaultCapacity];
            _values = new TValue[DefaultCapacity];
        }

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
            TValue value;

            var currentLength = _length;

            if (TryGetValue(key, out value))
            {
                return value;
            }

            lock (_keyLock)
            {
                if (TryGetValue(key, currentLength, out value))
                {
                    return value;
                }

                EnsureCapacity();

                value = valueFactory.Invoke(key);

                _keys[_length] = key;
                _values[_length] = value;

                ++_length;
            }

            return value;
        }

        private bool TryGetValue(TKey key, out TValue value) => TryGetValue(key, 0, out value);

        private bool TryGetValue(TKey key, int startIndex, out TValue value)
        {
            for (var i = startIndex; i < _length; i++)
            {
                if (_keys[i].Equals(key))
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

        public void Remove(TKey key)
        {
            for (var i = 0; i < _length; i++)
            {
                if (_keys[i].Equals(key))
                {
                    --_length;

                    for (var j = i; j < _length; j++)
                    {
                        _keys[j] = _keys[j + 1];
                        _values[j] = _values[j + 1];
                    }

                    _keys[_length] = default(TKey);
                    _values[_length] = default(TValue);
                }
            }
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