namespace AgileObjects.AgileMapper.Caching.Dictionaries
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class FixedSizeSimpleDictionary<TKey, TValue> : ISimpleDictionary<TKey, TValue>
    {
        public FixedSizeSimpleDictionary(int capacity, IEqualityComparer<TKey> keyComparer = null)
        {
            Keys = new TKey[capacity];
            Values = new TValue[capacity];
            Comparer = keyComparer ?? EqualityComparer<TKey>.Default;
        }

        public int Count { get; private set; }

        public bool None => Count == 0;

        public bool HasOne => Count == 1;

        public IList<TKey> Keys { get; protected set; }

        public IList<TValue> Values { get; protected set; }

        public IEqualityComparer<TKey> Comparer { get; }

        public TValue this[TKey key]
        {
            get
            {
                if (HasOne)
                {
                    return Values[0];
                }

                for (var i = 0; i < Count; i++)
                {
                    var thisKey = Keys[i];

                    if (Comparer.Equals(thisKey, key))
                    {
                        return Values[i];
                    }
                }

                throw new InvalidOperationException("Couldn't find a value for that key");
            }
        }

        public virtual ISimpleDictionary<TKey, TValue> Add(TKey key, TValue value)
        {
            Keys[Count] = key;
            Values[Count] = value;
            ++Count;
            return this;
        }

        public bool ContainsKey(TKey key)
        {
            switch (Count)
            {
                case 0:
                    return false;

                case 1:
                    return Comparer.Equals(Keys[0], key);

                default:
                    for (var i = 0; i < Count; i++)
                    {
                        var thisKey = Keys[i];

                        if (Comparer.Equals(thisKey, key))
                        {
                            return true;
                        }
                    }

                    return false;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            switch (Count)
            {
                case 0:
                    break;

                case 1:
                    if (Comparer.Equals(Keys[0], key))
                    {
                        value = Values[0];
                        return true;
                    }

                    break;

                default:
                    for (var i = 0; i < Count; i++)
                    {
                        var thisKey = Keys[i];

                        if (Comparer.Equals(thisKey, key))
                        {
                            value = Values[i];
                            return true;
                        }
                    }

                    break;
            }

            value = default;
            return false;
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
            {
                yield return new KeyValuePair<TKey, TValue>(Keys[i], Values[i]);
            }
        }

        #endregion
    }
}
