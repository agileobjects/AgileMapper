namespace AgileObjects.AgileMapper.Caching
{
    using System.Collections.Generic;
    using Extensions.Internal;

    internal class DefaultArrayCache<TKey, TValue> : ArrayCacheBase<TKey, TValue>
    {
        private readonly KeyComparer<TKey> _keyComparer;
        private TKey[] _keys;

        public DefaultArrayCache(KeyComparer<TKey> keyComparer)
        {
            _keyComparer = keyComparer ?? CompareKeys;
            _keys = new TKey[DefaultCapacity];
        }

        private static bool CompareKeys(TKey x, TKey y)
            => ReferenceEquals(x, y) || x?.Equals(y) == true;

        protected override bool TryGetValue(TKey key,
            IList<TValue> values,
            int startIndex,
            int length,
            out TValue value)
        {
            for (var i = startIndex; i < length; i++)
            {
                var thisKey = _keys[i];

                if (_keyComparer.Invoke(thisKey, key))
                {
                    value = values[i];
                    return true;
                }
            }

            return NotFound(out value);
        }

        protected override void EnsureKeyCapacity(int capacity)
            => _keys = _keys.EnlargeToArray(capacity);

        protected override void StoreKeyAt(int index, TKey key)
            => _keys[index] = key;

        protected override void ResetKeyAt(int index)
            => _keys[index] = default(TKey);
    }
}