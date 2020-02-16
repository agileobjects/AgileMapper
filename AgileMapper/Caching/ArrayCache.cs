namespace AgileObjects.AgileMapper.Caching
{
    using System;
    using System.Collections.Generic;

    internal class ArrayCache<TKey, TValue> : ICache<TKey, TValue>
    {
        private const int DefaultCapacity = 10;
        private readonly object _keyLock = new object();
        private readonly IKeyComparer<TKey> _keyComparer;

        private int[] _hashCodes;
        private int[] _keyIndexes;
        private TKey[] _keys;
        private TValue[] _values;
        private int _capacity;
        private int _length;

        public ArrayCache(IKeyComparer<TKey> keyComparer)
        {
            _capacity = DefaultCapacity;
            _length = 0;
            _keyComparer = keyComparer ?? default(DefaultComparer<TKey>);
            _values = new TValue[DefaultCapacity];

            if (UseHashCodes)
            {
                _hashCodes = new int[DefaultCapacity];
                _keyIndexes = new int[DefaultCapacity];
            }
            else
            {
                _keys = new TKey[DefaultCapacity];
            }
        }

        int ICache<TKey, TValue>.Count => _length;

        private bool UseHashCodes => _keyComparer.UseHashCodes;

        public IEnumerable<TValue> Values
        {
            get
            {
                for (var i = 0; i < _length;)
                {
                    yield return _values[i++];
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
                if ((_length > currentLength) && TryGetValue(key, currentLength, out value))
                {
                    return value;
                }

                value = valueFactory.Invoke(key);

                EnsureCapacity();

                if (UseHashCodes)
                {
                    StoreHashCode(key);
                }
                else
                {
                    _keys[_length] = key;
                }

                _values[_length++] = value;
            }

            return value;
        }

        private bool TryGetValue(TKey key, int startIndex, out TValue value)
        {
            if (_length == 0)
            {
                return NotFound(out value);
            }

            if (UseHashCodes)
            {
                return TryGetValueFromHashCode(key, startIndex, out value);
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

            return NotFound(out value);
        }

        private static bool NotFound(out TValue value)
        {
            value = default(TValue);
            return false;
        }

        private bool TryGetValueFromHashCode(TKey key, int startIndex, out TValue value)
        {
            var hashCode = key.GetHashCode();

            if ((hashCode < _hashCodes[0]) && (hashCode > _hashCodes[_length - 1]))
            {
                return NotFound(out value);
            }

            var lowerBound = Math.Max(startIndex, 0);
            var upperBound = _length - 1;

            while (lowerBound <= upperBound)
            {
                var searchIndex = (lowerBound + upperBound) / 2;

                if (_hashCodes[searchIndex] == hashCode)
                {
                    value = _values[_keyIndexes[searchIndex]];
                    return true;
                }

                if (_hashCodes[searchIndex] > hashCode)
                {
                    upperBound = searchIndex - 1;
                }
                else
                {
                    lowerBound = searchIndex + 1;
                }
            }

            return NotFound(out value);
        }

        private void EnsureCapacity()
        {
            if (_length < _capacity)
            {
                return;
            }

            _capacity += DefaultCapacity;
            _values = ResizeToCapacity(_values);

            if (UseHashCodes)
            {
                _hashCodes = ResizeToCapacity(_hashCodes);
                _keyIndexes = ResizeToCapacity(_keyIndexes);
            }
            else
            {
                _keys = ResizeToCapacity(_keys);
            }
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

        private void StoreHashCode(TKey key)
        {
            var hashCode = key.GetHashCode();

            if (_length == 0)
            {
                InsertHashCodeAt(0, hashCode, unshift: false);
                return;
            }

            if (_hashCodes[0] > hashCode)
            {
                InsertHashCodeAt(0, hashCode);
                return;
            }

            if (_hashCodes[_length - 1] < hashCode)
            {
                InsertHashCodeAt(_length, hashCode, unshift: false);
                return;
            }

            var lowerBound = 1;
            var upperBound = _length - 2;

            while (true)
            {
                if ((upperBound - lowerBound) <= 1)
                {
                    while (lowerBound <= upperBound)
                    {
                        if (_hashCodes[lowerBound] < hashCode)
                        {
                            ++lowerBound;
                            continue;
                        }

                        break;
                    }

                    InsertHashCodeAt(lowerBound, hashCode);
                    return;
                }

                var searchIndex = (lowerBound + upperBound) / 2;

                if (_hashCodes[searchIndex] > hashCode)
                {
                    upperBound = searchIndex - 1;
                }
                else
                {
                    lowerBound = searchIndex + 1;
                }
            }
        }

        private void InsertHashCodeAt(int i, int hashCode, bool unshift = true)
        {
            if (unshift)
            {
                for (var j = _length - 1; j >= i; --j)
                {
                    _hashCodes[j + 1] = _hashCodes[j];
                    _keyIndexes[j + 1] = _keyIndexes[j];
                }
            }

            _hashCodes[i] = hashCode;
            _keyIndexes[i] = _length;
        }

        public void Empty()
        {
            for (var i = 0; i < _length; i++)
            {
                _values[i] = default(TValue);

                if (UseHashCodes)
                {
                    _hashCodes[i] = default(int);
                    _keyIndexes[i] = default(int);
                }
                else
                {
                    _keys[i] = default(TKey);
                }
            }

            _length = 0;
        }
    }
}