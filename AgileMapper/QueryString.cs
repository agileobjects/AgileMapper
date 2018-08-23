namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Extensions.Internal;

    /// <summary>
    /// Encapsulates a query string-formatted string.
    /// </summary>
    public sealed class QueryString : IDictionary<string, string>
    {
        private readonly Dictionary<string, string> _keyValuePairs;

        private QueryString(Dictionary<string, string> keyValuePairs)
        {
            _keyValuePairs = keyValuePairs;
        }

        /// <summary>
        /// Converts the <paramref name="queryString" /> to its string representation.
        /// </summary>
        /// <param name="queryString">The <see cref="QueryString"/> to convert.</param>
        /// <returns>The string representation of the <paramref name="queryString" />.</returns>
        public static explicit operator string(QueryString queryString) => queryString.ToString();

        /// <summary>
        /// Converts the <paramref name="queryString" /> to a <see cref="QueryString"/>.
        /// </summary>
        /// <param name="queryString">The query string-formatted string to convert.</param>
        /// <returns>
        /// A <see cref="QueryString"/> instance based on the given <paramref name="queryString"/>.
        /// </returns>
        public static explicit operator QueryString(string queryString) => Parse(queryString);

        /// <summary>
        /// Factory method for converting a query string-formatted string into a <see cref="QueryString"/>
        /// instance.
        /// </summary>
        /// <param name="queryString">The query string-formatted string to convert.</param>
        /// <returns>
        /// A <see cref="QueryString"/> instance based on the given <paramref name="queryString"/>.
        /// </returns>
        public static QueryString Parse(string queryString)
        {
            if (queryString.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Query string cannot be blank or null.", nameof(queryString));
            }

            var keyValuePairs = new Dictionary<string, string>();

            var previousAmpersandIndex = queryString[0] == '?' ? 1 : 0;

            while (true)
            {
                var nextAmpersandIndex = queryString.IndexOf('&', previousAmpersandIndex);

                if (nextAmpersandIndex == -1)
                {
                    nextAmpersandIndex = queryString.Length;
                }

                var equalsSignIndex = queryString.IndexOf('=', previousAmpersandIndex);

                int keyLength;
                string value;

                if ((equalsSignIndex == -1) || (equalsSignIndex > nextAmpersandIndex))
                {
                    keyLength = nextAmpersandIndex - previousAmpersandIndex;
                    value = null;
                }
                else
                {
                    keyLength = equalsSignIndex - previousAmpersandIndex;

                    var valueLength = nextAmpersandIndex - equalsSignIndex - 1;

                    value = (valueLength > 0)
                        ? Unescape(queryString, equalsSignIndex + 1, valueLength)
                        : null;
                }

                var key = Unescape(queryString, previousAmpersandIndex, keyLength);

                if (keyValuePairs.TryGetValue(key, out var existingValue))
                {
                    if (value != null)
                    {
                        if (existingValue != null)
                        {
                            value = existingValue + "," + value;
                        }

                        keyValuePairs[key] = value;
                    }
                }
                else
                {
                    keyValuePairs.Add(key, value);
                }

                previousAmpersandIndex = nextAmpersandIndex + 1;

                if ((previousAmpersandIndex == queryString.Length) || (nextAmpersandIndex == queryString.Length))
                {
                    return new QueryString(keyValuePairs);
                }
            }
        }

        #region Parse Helpers

        private static string Unescape(string queryString, int fromIndex, int length)
        {
            var substring = queryString.Substring(fromIndex, length);

            if (length < 2)
            {
                return substring;
            }

            substring = Uri.UnescapeDataString(substring);
#if NET35
            substring = substring.Replace("%21", "!");
#endif
            return substring.Replace("%2E", ".");
        }

        #endregion

        /// <summary>
        /// Returns the query string-formatted representation of the <see cref="QueryString"/>.
        /// </summary>
        /// <returns>The query string-formatted representation of the <see cref="QueryString"/>.</returns>
        public override string ToString()
        {
            var queryStringParts = new string[_keyValuePairs.Count * 4];

            var i = 0;

            foreach (var keyValuePair in _keyValuePairs)
            {
                queryStringParts[i++] = Uri.EscapeDataString(keyValuePair.Key);
                queryStringParts[i++] = "=";
                queryStringParts[i++] = Uri.EscapeDataString(keyValuePair.Value);
                queryStringParts[i++] = "&";
            }

            queryStringParts[queryStringParts.Length - 1] = string.Empty;

            var queryString = queryStringParts.Join(string.Empty);
#if NET35
            queryString = queryString.Replace("!", "%21");
#endif
            return queryString.Replace(".", "%2E");
        }

        #region IDictionary Members

        /// <summary>
        /// Gets the enumerator for the <see cref="QueryString"/>.
        /// </summary>
        /// <returns>The enumerator for the <see cref="QueryString"/>.</returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            => _keyValuePairs.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _keyValuePairs.GetEnumerator();

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
            => KvpCollection.Add(item);

        /// <summary>
        /// Empties the <see cref="QueryString"/>.
        /// </summary>
        public void Clear() => _keyValuePairs.Clear();

        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
            => KvpCollection.Contains(item);

        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
            => KvpCollection.CopyTo(array, arrayIndex);

        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
            => KvpCollection.Remove(item);

        private ICollection<KeyValuePair<string, string>> KvpCollection => _keyValuePairs;

        /// <summary>
        /// Gets the number of KeyValuePairs in the <see cref="QueryString"/>.
        /// </summary>
        public int Count => _keyValuePairs.Count;

        bool ICollection<KeyValuePair<string, string>>.IsReadOnly => false;

        /// <summary>
        /// Adds a new KeyValuePair to the <see cref="QueryString"/>.
        /// </summary>
        /// <param name="key">The key to use in the KeyValuePair.</param>
        /// <param name="value">The value to use in the KeyValuePair.</param>
        public void Add(string key, string value) => _keyValuePairs.Add(key, value);

        /// <summary>
        /// Determines whether the <see cref="QueryString"/> contains a KeyValuePair with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key for which to make the determination.</param>
        /// <returns>
        /// True if the <see cref="QueryString"/> contains a KeyValuePair with the given <paramref name="key"/>, otherwise false.
        /// </returns>
        public bool ContainsKey(string key) => _keyValuePairs.ContainsKey(key);

        /// <summary>
        /// Removes the query string KeyValuePair with the given <paramref name="key"/> from the <see cref="QueryString"/>.
        /// </summary>
        /// <param name="key">The key of the KeyValuePair to remove.</param>
        /// <returns>
        /// True if the KeyValuePair is successfully removed; otherwise, false. False is also returned if no KeyValuePair
        /// exists in the <see cref="QueryString"/> with the given <paramref name="key"/>.
        /// </returns>
        public bool Remove(string key) => _keyValuePairs.Remove(key);

        /// <summary>
        /// Gets the value associated with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key the value for which should be retrieved.</param>
        /// <param name="value">
        /// Populated with the value matching the given <paramref name="key"/> if one exists, otherwise set to null.
        /// </param>
        /// <returns>
        /// True if a value exists matching the given <paramref name="key"/>, otherwise false.
        /// </returns>
        public bool TryGetValue(string key, out string value)
            => _keyValuePairs.TryGetValue(key, out value);

        /// <summary>
        /// Gets or sets the query string value with the given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the query string value to get or set.</param>
        /// <returns>The query string value with the specified key.</returns>
        public string this[string key]
        {
            get => _keyValuePairs[key];
            set => _keyValuePairs[key] = value;
        }

        /// <summary>
        /// Gets the set of keys contained in the <see cref="QueryString"/>.
        /// </summary>
        public ICollection<string> Keys => _keyValuePairs.Keys;

        ICollection<string> IDictionary<string, string>.Values => _keyValuePairs.Values;

        #endregion
    }
}