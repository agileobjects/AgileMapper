namespace AgileObjects.AgileMapper.Configuration.Inline
{
    using System;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Caching;

    internal class InlineMapperSet
    {
        private readonly ICache<IInlineMapperKey, MulticastDelegate> _inlineExecutorsCache;

        public InlineMapperSet(MapperContext mapperContext)
        {
            _inlineExecutorsCache = mapperContext.Cache.CreateScoped<IInlineMapperKey, MulticastDelegate>();
        }

        public InlineMappingExecutor<TSource, TTarget> GetExecutorFor<TSource, TTarget>(
            Expression<Action<IFullMappingConfigurator<TSource, TTarget>>>[] configurations,
            MappingExecutor<TSource> mappingExecutor)
        {
            var inlineExecutor = _inlineExecutorsCache.GetOrAdd(
                new InlineMapperKey<TSource, TTarget>(configurations, mappingExecutor),
                k => k.CreateExecutor());

            return (InlineMappingExecutor<TSource, TTarget>)inlineExecutor;
        }
    }
}
