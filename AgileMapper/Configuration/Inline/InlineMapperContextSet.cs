namespace AgileObjects.AgileMapper.Configuration.Inline
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Api.Configuration.Projection;
    using Caching;

    internal class InlineMapperContextSet : IEnumerable<MapperContext>
    {
        private readonly ICache<IInlineMapperKey, MapperContext> _inlineContextsCache;
        private readonly IMappingContext _queryProjectionMappingContext;

        public InlineMapperContextSet(MapperContext parentMapperContext)
        {
            _inlineContextsCache = parentMapperContext.Cache.CreateScoped<IInlineMapperKey, MapperContext>();
            _queryProjectionMappingContext = parentMapperContext.QueryProjectionMappingContext;
        }

        public MapperContext GetContextFor<TSource, TTarget>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations,
            MappingExecutor<TSource> executor)
        {
            return GetContextFor<TSource, TTarget, IFullMappingInlineConfigurator<TSource, TTarget>>(
                configurations,
                configInfo => new MappingConfigurator<TSource, TTarget>(configInfo),
                executor);
        }

        public MapperContext GetContextFor<TSourceElement, TResultElement>(
            Expression<Action<IFullProjectionInlineConfigurator<TSourceElement, TResultElement>>>[] configurations,
            ProjectionExecutor<TSourceElement> executor)
        {
            return GetContextFor<TSourceElement, TResultElement, IFullProjectionInlineConfigurator<TSourceElement, TResultElement>>(
                configurations,
                configInfo => new ProjectionConfigurator<TSourceElement, TResultElement>(configInfo),
                _queryProjectionMappingContext);
        }

        private MapperContext GetContextFor<TSource, TTarget, TConfigurator>(
            Expression<Action<TConfigurator>>[] configurations,
            Func<MappingConfigInfo, TConfigurator> configuratorFactory,
            IMappingContext mappingContext)
        {
            var key = new InlineMapperKey<TSource, TTarget, TConfigurator>(
                configurations,
                configuratorFactory,
                mappingContext);

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
