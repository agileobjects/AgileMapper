namespace AgileObjects.AgileMapper.Caching.Dictionaries
{
    using System.Collections.Generic;

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
            Keys = ResizeToCapacity(Keys);
            Values = ResizeToCapacity(Values);
        }

        private T[] ResizeToCapacity<T>(IList<T> existingArray)
        {
            var biggerArray = new T[_capacity];

            for (var i = 0; i < Count; ++i)
            {
                biggerArray[i] = existingArray[i];
            }

            return biggerArray;
        }
    }
}