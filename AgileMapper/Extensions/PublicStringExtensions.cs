using System;
using System.Collections.Generic;
using System.Text;

namespace AgileObjects.AgileMapper.Extensions
{
    /// <summary>
    /// Provides mapping-related extension methods for strings.
    /// </summary>
    public static class PublicStringExtensions
    {
        /// <summary>
        /// Convert this <paramref name="queryString"/>-formatted string to a <see cref="QueryString"/>
        /// instance.
        /// </summary>
        /// <param name="queryString">The query string-formatted string to convert.</param>
        /// <returns>A <see cref="QueryString"/> based on this <paramref name="queryString"/>.</returns>
        public static QueryString ToQueryString(this string queryString)
            => QueryString.Parse(queryString);
    }
}
