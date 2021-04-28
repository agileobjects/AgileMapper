namespace AgileObjects.AgileMapper.Caching
{
    using System;
    using System.Collections.Generic;
    using Extensions.Internal;

    internal class HashCodeArrayCache<TKey, TValue> : ArrayCacheBase<TKey, TValue>
    {
        private int[] _hashCodes;
        private int[] _valueIndexes;

        public HashCodeArrayCache()
        {
            _hashCodes = new int[DefaultCapacity];
            _valueIndexes = new int[DefaultCapacity];
        }

        protected override bool TryGetValue(
            TKey key,
            IList<TValue> values,
            int startIndex,
            int length,
            out TValue value)
        {
            var hashCode = key.GetHashCode();

            if ((hashCode < _hashCodes[0]) && (hashCode > _hashCodes[length - 1]))
            {
                return NotFound(out value);
            }

            var lowerBound = Math.Max(startIndex, 0);
            var upperBound = length - 1;

            while (lowerBound <= upperBound)
            {
                var searchIndex = (lowerBound + upperBound) / 2;

                if (_hashCodes[searchIndex] == hashCode)
                {
                    var valueIndex = _valueIndexes[searchIndex];
                    value = values[valueIndex];
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

        protected override void EnsureKeyCapacity(int capacity)
        {
            _hashCodes = _hashCodes.EnlargeToArray(capacity);
            _valueIndexes = _valueIndexes.EnlargeToArray(capacity);
        }

        protected override void StoreKeyAt(int length, TKey key)
        {
            var hashCode = key.GetHashCode();

            if (length == 0)
            {
                InsertHashCodeAt(0, length, hashCode, unshift: false);
                return;
            }

            if (_hashCodes[0] > hashCode)
            {
                InsertHashCodeAt(0, length, hashCode, unshift: true);
                return;
            }

            if (_hashCodes[length - 1] < hashCode)
            {
                InsertHashCodeAt(length, length, hashCode, unshift: false);
                return;
            }

            var lowerBound = 1;
            var upperBound = length - 2;

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

                    InsertHashCodeAt(lowerBound, length, hashCode, unshift: true);
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

        protected override void ResetKeyAt(int index)
        {
            _hashCodes[index] = default(int);
            _valueIndexes[index] = default(int);
        }

        private void InsertHashCodeAt(
            int index,
            int length,
            int hashCode,
            bool unshift)
        {
            if (unshift)
            {
                for (var j = length - 1; j >= index; --j)
                {
                    _hashCodes[j + 1] = _hashCodes[j];
                    _valueIndexes[j + 1] = _valueIndexes[j];
                }
            }

            _hashCodes[index] = hashCode;
            _valueIndexes[index] = length;
        }
    }
}