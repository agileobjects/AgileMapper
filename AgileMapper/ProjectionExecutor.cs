namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Api;
    using Api.Configuration.Projection;
    using ObjectPopulation;

    internal class ProjectionExecutor<TSourceElement> : IProjectionResultSpecifier<TSourceElement>
    {
        private readonly MapperContext _mapperContext;
        private readonly IQueryable<TSourceElement> _sourceQueryable;

        public ProjectionExecutor(MapperContext mapperContext, IQueryable<TSourceElement> sourceQueryable)
        {
            _mapperContext = mapperContext;
            _sourceQueryable = sourceQueryable;
        }

        #region To Overloads

        IQueryable<TResultElement> IProjectionResultSpecifier<TSourceElement>.To<TResultElement>()
            => PerformProjection<TResultElement>(_mapperContext);

        IQueryable<TResultElement> IProjectionResultSpecifier<TSourceElement>.To<TResultElement>(
            Expression<Action<IFullProjectionInlineConfigurator<TSourceElement, TResultElement>>> configuration)
        {
            return PerformProjection(new[] { configuration });
        }

        #endregion

        private IQueryable<TResultElement> PerformProjection<TResultElement>(
            Expression<Action<IFullProjectionInlineConfigurator<TSourceElement, TResultElement>>>[] configurations)
        {
            var inlineMapperContext = _mapperContext.InlineContexts.GetContextFor(configurations, this);

            return PerformProjection<TResultElement>(inlineMapperContext);
        }

        private IQueryable<TResultElement> PerformProjection<TResultElement>(MapperContext mapperContext)
        {
            var rootMappingData = ObjectMappingDataFactory.ForProjection<TSourceElement, TResultElement>(
                _sourceQueryable,
                mapperContext.QueryProjectionMappingContext);

            var queryProjection = rootMappingData.MapStart();

            return queryProjection;
        }
    }
}
