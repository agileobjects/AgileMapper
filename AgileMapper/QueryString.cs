namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Extensions.Internal;

    /// <summary>
    /// Encapsulates a query string-formatted string.
    /// </summary>
    public class QueryString : IDictionary<string, string>
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

            var nextPairIndex = queryString[0] == '?' ? 1 : 0;

            while (true)
            {
                if (nextPairIndex == -1)
                {
                    return new QueryString(keyValuePairs);
                }

                var separatorIndex = queryString.IndexOf('=', nextPairIndex);

                if (separatorIndex == -1)
                {
                    nextPairIndex = queryString.IndexOf('&', ++nextPairIndex);
                    continue;
                }

                var keyLength = separatorIndex - nextPairIndex;
                var key = Unescape(queryString.Substring(nextPairIndex, keyLength));

                nextPairIndex = queryString.IndexOf('&', separatorIndex);

                if (nextPairIndex == -1)
                {
                    nextPairIndex = queryString.Length;
                }

                ++separatorIndex;
                var valueLength = nextPairIndex - separatorIndex;
                var value = Unescape(queryString.Substring(separatorIndex, valueLength));

                if (keyValuePairs.TryGetValue(key, out var existingValue))
                {
                    value = existingValue + "," + value;
                }

                keyValuePairs[key] = value;

                if (nextPairIndex == queryString.Length)
                {
                    nextPairIndex = -1;
                    continue;
                }

                ++nextPairIndex;
            }
        }

        private static string Unescape(string substring)
        {
            if (substring.Length < 2)
            {
                return substring;
            }

            substring = Uri.UnescapeDataString(substring);
#if NET35
            substring = substring.Replace("%21", "!");
#endif
            return substring.Replace("%2E", ".");
        }

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

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
            => _keyValuePairs.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _keyValuePairs.GetEnumerator();

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
            => KvpCollection.Add(item);

        void ICollection<KeyValuePair<string, string>>.Clear() => _keyValuePairs.Clear();

        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
            => KvpCollection.Contains(item);

        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
            => KvpCollection.CopyTo(array, arrayIndex);

        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
            => KvpCollection.Remove(item);

        private ICollection<KeyValuePair<string, string>> KvpCollection => _keyValuePairs;

        int ICollection<KeyValuePair<string, string>>.Count => _keyValuePairs.Count;

        bool ICollection<KeyValuePair<string, string>>.IsReadOnly => false;

        void IDictionary<string, string>.Add(string key, string value) => _keyValuePairs.Add(key, value);

        bool IDictionary<string, string>.ContainsKey(string key) => _keyValuePairs.ContainsKey(key);

        bool IDictionary<string, string>.Remove(string key) => _keyValuePairs.Remove(key);

        bool IDictionary<string, string>.TryGetValue(string key, out string value)
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

        ICollection<string> IDictionary<string, string>.Keys => _keyValuePairs.Keys;

        ICollection<string> IDictionary<string, string>.Values => _keyValuePairs.Values;

        #endregion
    }
}