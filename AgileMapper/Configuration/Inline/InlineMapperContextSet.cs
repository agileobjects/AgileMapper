namespace AgileObjects.AgileMapper.Configuration.Inline
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Caching;

    internal class InlineMapperContextSet : IEnumerable<MapperContext>
    {
        private readonly ICache<IInlineMapperKey, MapperContext> _inlineContextsCache;

        public InlineMapperContextSet(MapperContext parentMapperContext)
        {
            _inlineContextsCache = parentMapperContext.Cache.CreateScoped<IInlineMapperKey, MapperContext>();
        }

        public MapperContext GetContextFor<TSource, TTarget>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations,
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
#if CODE_COVERAGE_SUPPORTED
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
