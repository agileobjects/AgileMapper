namespace AgileObjects.AgileMapper.Queryables.Api
{
    using System;
    using System.Linq;

    /// <summary>
    /// Provides the option to specify a Type of IQueryProvider implementation to use in a query projection caching.
    /// </summary>
    public class QueryProviderTypeSelector
    {
        internal static readonly QueryProviderTypeSelector Instance = new QueryProviderTypeSelector();

        private QueryProviderTypeSelector()
        {
        }

        /// <summary>
        /// Use the given <typeparamref name="TQueryProvider"/> in this query projection caching.
        /// </summary>
        /// <typeparam name="TQueryProvider">
        /// The Type of the IQueryProvider implementation to use in this query projection caching.
        /// </typeparam>
        /// <returns>
        /// The Type of the IQueryProvider implementation to use in this query projection caching.
        /// </returns>
        public Type Using<TQueryProvider>()
            where TQueryProvider : IQueryProvider
        {
            return typeof(TQueryProvider);
        }
    }
}