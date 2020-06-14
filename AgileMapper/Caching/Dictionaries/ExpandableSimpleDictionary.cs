namespace AgileObjects.AgileMapper.Caching.Dictionaries
{
    using System.Collections.Generic;
    using Extensions.Internal;

    internal class ExpandableSimpleDictionary<TKey, TValue> : FixedSizeSimpleDictionary<TKey, TValue>
    {
        private readonly int _initialCapacity;
        private int _capacity;

        public ExpandableSimpleDictionary(int capacity, IEqualityComparer<TKey> keyComparer)
            : base(capacity, keyComparer)
        {
            _initialCapacity = _capacity = capacity;
        }

        public override ISimpleDictionary<TKey, TValue> Add(TKey key, TValue value)
        {
            EnsureCapacity();
            return base.Add(key, value);
        }

        private void EnsureCapacity()
        {
            if (Count < _capacity)
            {
                return;
            }

            _capacity += _initialCapacity;
            Keys = Keys.EnlargeToArray(_capacity);
            Values = Values.EnlargeToArray(_capacity);
        }
    }
}