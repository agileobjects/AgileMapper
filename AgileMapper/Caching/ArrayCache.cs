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

        public ArrayCache(int capacity = DefaultCapacity)
        {
            _capacity = capacity;
            _length = 0;
            _keys = new TKey[capacity];
            _values = new TValue[capacity];
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

                if (ReferenceEquals(thisKey, key) || thisKey.Equals(key))
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

        //public void CloneTo(ICache<TKey, TValue> otherCache)
        //{
        //    for (var i = 0; i < _length; i++)
        //    {
        //        var value = _values[i];

        //        if (value is ICloneable cloneable)
        //        {
        //            value = (TValue)cloneable.Clone();
        //        }

        //        otherCache.GetOrAdd(_keys[i], m => value);
        //    }
        //}

        public void Empty()
        {
            for (var i = 0; i < _length; i++)
            {
                _keys[i] = default(TKey);
                _values[i] = default(TValue);
            }

            _length = 0;
        }

        //object ICloneable.Clone()
        //{
        //    var cache = new ArrayCache<TKey, TValue>(Math.Max(_length, DefaultCapacity));

        //    for (var i = 0; i < _length; i++)
        //    {
        //        cache._keys[i] = _keys[i];
        //        cache._values[i] = _values[i];
        //    }

        //    return cache;
        //}
    }
}