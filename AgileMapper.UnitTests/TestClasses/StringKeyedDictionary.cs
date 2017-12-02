namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System.Collections;
    using System.Collections.Generic;

    public class StringKeyedDictionary<TValue> : IDictionary<string, TValue>
    {
        private readonly IDictionary<string, TValue> _dictionary;

        public StringKeyedDictionary()
        {
            _dictionary = new Dictionary<string, TValue>();
        }

        public int Count => _dictionary.Count;

        public bool IsReadOnly => _dictionary.IsReadOnly;

        public ICollection<string> Keys => _dictionary.Keys;

        public ICollection<TValue> Values => _dictionary.Values;

        public TValue this[string key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        public void Add(KeyValuePair<string, TValue> item) => _dictionary.Add(item);

        public void Add(string key, TValue value) => _dictionary.Add(key, value);

        public bool TryGetValue(string key, out TValue value) => _dictionary.TryGetValue(key, out value);

        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public bool ContainsKey(string key) => _dictionary.ContainsKey(key);

        public bool Contains(KeyValuePair<string, TValue> item) => _dictionary.Contains(item);

        public bool Remove(KeyValuePair<string, TValue> item) => _dictionary.Remove(item);

        public bool Remove(string key) => _dictionary.Remove(key);

        public void Clear() => _dictionary.Clear();

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
