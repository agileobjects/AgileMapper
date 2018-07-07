namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides extension methods used by compiled mapping functions.
    /// </summary>
    public static class PublicStringExtensions
    {
        /// <summary>
        /// Gets the string value of the first character of the <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value from which to get the first character.</param>
        /// <returns>
        /// The first character of the value if it has a length of greater than one, otherwise returns
        /// <paramref name="value"/>.
        /// </returns>
        public static string FirstOrDefault(this string value)
        {
            if (string.IsNullOrEmpty(value) || (value.Length == 1))
            {
                return value;
            }

            return value[0].ToString();
        }

        /// <summary>
        /// Determines if the <paramref name="subjectKey"/> matches the given <paramref name="queryKey"/>,
        /// given the given <paramref name="separator"/> and <paramref name="elementKeyPartMatcher"/>.
        /// </summary>
        /// <param name="subjectKey">The subject key for which to make the determination.</param>
        /// <param name="queryKey">The query key for which to make the determination.</param>
        /// <param name="separator">The separator to use to separate key parts while making the determination.</param>
        /// <param name="elementKeyPartMatcher">
        /// A Regex with which to match element key parts while making the determination.
        /// </param>
        /// <returns>
        /// True if the <paramref name="subjectKey"/> matches the given <paramref name="queryKey"/>, otherwise false.
        /// </returns>
        public static bool MatchesKey(
            this string subjectKey,
            string queryKey,
            string separator,
            Regex elementKeyPartMatcher)
        {
            if (queryKey == null)
            {
                // This can happen when mapping to types with multiple, nested
                // recursive relationships, e.g:
                // Dictionary<,> -> Order -> OrderItems -> Order -> OrderItems
                // ...it's basically not supported
                return false;
            }

            if (subjectKey.EqualsIgnoreCase(queryKey))
            {
                return true;
            }

            var elementKeyParts = elementKeyPartMatcher.Matches(queryKey);

            var searchEndIndex = queryKey.Length;

            for (var i = elementKeyParts.Count; i > 0; --i)
            {
                var elementKeyPart = elementKeyParts[i - 1];
                var matchStartIndex = elementKeyPart.Index;
                var matchEndIndex = matchStartIndex + elementKeyPart.Length;

                ReplaceSeparatorsInSubstring(matchStartIndex, matchEndIndex, ref queryKey, separator, ref searchEndIndex);
            }

            ReplaceSeparatorsInSubstring(searchEndIndex, 0, ref queryKey, separator, ref searchEndIndex);

            return subjectKey.EqualsIgnoreCase(queryKey);
        }

        private static void ReplaceSeparatorsInSubstring(
            int matchStartIndex,
            int matchEndIndex,
            ref string queryKey,
            string separator,
            ref int searchEndIndex)
        {
            var querySubstring = queryKey.Substring(matchEndIndex, searchEndIndex - matchEndIndex);

            if (querySubstring.IndexOf(separator, StringComparison.Ordinal) == -1)
            {
                searchEndIndex = matchStartIndex;
                return;
            }

            var flattenedQuerySubstring = querySubstring.Replace(separator, null);

            queryKey = queryKey
                .Remove(matchEndIndex, searchEndIndex - matchEndIndex)
                .Insert(matchEndIndex, flattenedQuerySubstring);

            searchEndIndex = matchStartIndex;
        }

        /// <summary>
        /// Determines if the <paramref name="subjectKey"/> matches the given <paramref name="queryKey"/>, using
        /// the given <paramref name="separator"/> and the default element key part pattern.
        /// </summary>
        /// <param name="subjectKey">The subject key for which to make the determination.</param>
        /// <param name="queryKey">The query key for which to make the determination.</param>
        /// <param name="separator">The separator to use to separate key parts while making the determination.</param>
        /// <returns>
        /// True if the <paramref name="subjectKey"/> matches the given <paramref name="queryKey"/>, otherwise false.
        /// </returns>
        public static bool MatchesKey(this string subjectKey, string queryKey, string separator)
        {
            if (queryKey == null)
            {
                // This can happen when mapping to types with multiple, nested
                // recursive relationships, e.g:
                // Dictionary<,> -> Order -> OrderItems -> Order -> OrderItems
                // ...it's basically not supported
                return false;
            }

            return subjectKey.EqualsIgnoreCase(queryKey) ||
                   subjectKey.MatchesFlattenedKey(queryKey, separator);
        }

        private static bool MatchesFlattenedKey(this string subjectKey, string queryKey, string separator)
        {
            return (queryKey.IndexOf(separator, StringComparison.Ordinal) != -1) &&
                   subjectKey.EqualsIgnoreCase(queryKey.Replace(separator, null));
        }

        /// <summary>
        /// Determines if the <paramref name="subjectKey"/> matches the given <paramref name="queryKey"/>, using
        /// the default separator and element key part pattern.
        /// </summary>
        /// <param name="subjectKey">The subject key for which to make the determination.</param>
        /// <param name="queryKey">The query key for which to make the determination.</param>
        /// <returns>
        /// True if the <paramref name="subjectKey"/> matches the given <paramref name="queryKey"/>, otherwise false.
        /// </returns>
        public static bool MatchesKey(this string subjectKey, string queryKey)
        {
            if (queryKey == null)
            {
                // This can happen when mapping to types with multiple, nested
                // recursive relationships, e.g:
                // Dictionary<,> -> Order -> OrderItems -> Order -> OrderItems
                // ...it's basically not supported
                return false;
            }

            return subjectKey.EqualsIgnoreCase(queryKey);
        }
    }
}