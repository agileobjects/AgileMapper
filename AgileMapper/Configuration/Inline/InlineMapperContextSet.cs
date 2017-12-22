namespace AgileObjects.AgileMapper.Configuration.Inline
{
    using System.Collections;
    using System.Collections.Generic;
    using Caching;

    internal class InlineMapperContextSet : IEnumerable<MapperContext>
    {
        private readonly ICache<IInlineMapperKey, MapperContext> _inlineContextsCache;

        public InlineMapperContextSet(MapperContext parentMapperContext)
        {
            _inlineContextsCache = parentMapperContext.Cache.CreateScoped<IInlineMapperKey, MapperContext>();
        }

        public MapperContext GetContextFor<TSource, TTarget>(
            IInlineConfigurationSet configurations,
            MappingExecutor<TSource> executor)
        {
            var key = new InlineMapperKey<TSource, TTarget>(configurations, executor);

            var inlineMapperContext = _inlineContextsCache.GetOrAdd(
                key,
                k => k.CreateInlineMapperContext());

            return inlineMapperContext;
        }

        #region IEnumerable Members

        public IEnumerator<MapperContext> GetEnumerator()
        {
            return _inlineContextsCache.Values.GetEnumerator();
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
