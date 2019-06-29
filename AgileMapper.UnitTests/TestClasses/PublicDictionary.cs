namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System.Collections;
    using System.Collections.Generic;

    internal class PublicDictionary : IDictionary<string, object>
    {
        private readonly Dictionary<string, object> _data;

        public PublicDictionary()
        {
            _data = new Dictionary<string, object>();
        }

        public static PublicDictionary Deserialize(string data)
            => new PublicDictionary();

        private ICollection<KeyValuePair<string, object>> DataAsCollection => _data;

        public object this[string key]
        {
            get => _data[key];
            set => _data[key] = value;
        }

        public ICollection<string> Keys => _data.Keys;

        public ICollection<object> Values => _data.Values;

        public int Count => _data.Count;

        public bool IsReadOnly => false;

        public void Add(string key, object value) => _data.Add(key, value);

        public void Add(KeyValuePair<string, object> item) => DataAsCollection.Add(item);

        public void Clear() => _data.Clear();

        public bool Contains(KeyValuePair<string, object> item)
            => DataAsCollection.Contains(item);

        public bool ContainsKey(string key) => _data.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            => DataAsCollection.CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            => DataAsCollection.GetEnumerator();

        public bool Remove(string key) => _data.Remove(key);

        public bool Remove(KeyValuePair<string, object> item)
            => DataAsCollection.Remove(item);

        public bool TryGetValue(string key, out object value)
            => _data.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator()
            => DataAsCollection.GetEnumerator();
    }
}