namespace AgileObjects.AgileMapper.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Extensions.Internal;
    using Queryables.Converters;

    internal abstract class ArrayCacheBase<TKey, TValue> : ICache<TKey, TValue>
    {
        protected const int DefaultCapacity = 10;
        private readonly ReaderWriterLockSlim _lock;

        private TValue[] _values;
        private int _capacity;
        private int _length;

        protected ArrayCacheBase()
        {
            _capacity = DefaultCapacity;
            _values = new TValue[DefaultCapacity];
            _lock = new ReaderWriterLockSlim();
        }

        int ICache<TKey, TValue>.Count => _length;

        public IEnumerable<TValue> Values
        {
            get
            {
                for (var i = 0; i < _length; ++i)
                {
                    yield return _values[i];
                }
            }
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            _lock.EnterReadLock();

            var currentLength = _length;

            TValue value;

            try
            {
                if (TryGetValue(key, 0, currentLength, out value))
                {
                    return value;
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            _lock.EnterUpgradeableReadLock();

            try
            {
                if ((_length > currentLength) && TryGetValue(key, currentLength, _length, out value))
                {
                    return value;
                }

                _lock.EnterWriteLock();

                try
                {
                    value = valueFactory.Invoke(key);

                    EnsureCapacity();
                    StoreKeyAt(_length, key);

                    _values[_length] = value;
                    _length++;

                    return value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        private bool TryGetValue(TKey key, int startIndex, int length, out TValue value)
        {
            if (length == 0)
            {
                return NotFound(out value);
            }

            return TryGetValue(key, _values, startIndex, length, out value);
        }

        protected static bool NotFound(out TValue value)
        {
            value = default(TValue);
            return false;
        }

        protected abstract bool TryGetValue(
            TKey key,
            IList<TValue> values,
            int startIndex,
            int length,
            out TValue value);

        private void EnsureCapacity()
        {
            if (_length < _capacity)
            {
                return;
            }

            _capacity += DefaultCapacity;
            _values = _values.EnlargeToArray(_capacity);
            EnsureKeyCapacity(_capacity);
        }

        protected abstract void EnsureKeyCapacity(int capacity);

        protected abstract void StoreKeyAt(int index, TKey key);

        public void Empty()
        {
            for (var i = 0; i < _length; i++)
            {
                _values[i] = default(TValue);
                ResetKeyAt(i);
            }

            _length = 0;
        }

        protected abstract void ResetKeyAt(int index);
    }
}