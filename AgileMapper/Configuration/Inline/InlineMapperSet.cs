namespace AgileObjects.AgileMapper.Configuration.Inline
{
    using System;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Caching;

    internal class InlineMapperSet
    {
        private readonly ICache<IInlineMapperKey, MapperContext> _inlineContextsCache;

        public InlineMapperSet(MapperContext parentMapperContext)
        {
            _inlineContextsCache = parentMapperContext.Cache.CreateScoped<IInlineMapperKey, MapperContext>();
        }

        public MapperContext GetContextFor<TSource, TTarget>(
            Expression<Action<IFullMappingConfigurator<TSource, TTarget>>>[] configurations,
            MappingExecutor<TSource> initiatingExecutor)
        {
            var key = new InlineMapperKey<TSource, TTarget>(configurations, initiatingExecutor);

            var inlineMapperContext = _inlineContextsCache.GetOrAdd(
                key,
                k => k.CreateInlineMapperContext());

            return inlineMapperContext;
        }
    }
}
