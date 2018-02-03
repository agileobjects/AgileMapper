namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Api;
    using Api.Configuration.Projection;
    using ObjectPopulation;
    using Queryables.Api;

    internal class ProjectionExecutor<TSourceElement> : IProjectionResultSpecifier<TSourceElement>
    {
        private readonly IQueryable<TSourceElement> _sourceQueryable;

        public ProjectionExecutor(IQueryable<TSourceElement> sourceQueryable)
        {
            _sourceQueryable = sourceQueryable;
        }

        #region To Overloads

        IQueryable<TResultElement> IProjectionResultSpecifier<TSourceElement>.To<TResultElement>()
        {
            return PerformProjection<TResultElement>(Mapper.Default);
        }

        IQueryable<TResultElement> IProjectionResultSpecifier<TSourceElement>.To<TResultElement>(
            Func<ProjectionMapperSelector, IMapper> mapperSelector)
        {
            var mapper = mapperSelector.Invoke(ProjectionMapperSelector.Instance);

            return PerformProjection<TResultElement>(mapper);
        }

        IQueryable<TResultElement> IProjectionResultSpecifier<TSourceElement>.To<TResultElement>(
            Expression<Action<IFullProjectionInlineConfigurator<TSourceElement, TResultElement>>> configuration)
        {
            return PerformProjection(Mapper.Default, new[] { configuration });
        }

        #endregion

        private IQueryable<TResultElement> PerformProjection<TResultElement>(IMapper mapper)
            => PerformProjection<TResultElement>(((IMapperInternal)mapper).Context);

        private IQueryable<TResultElement> PerformProjection<TResultElement>(
            IMapper mapper,
            Expression<Action<IFullProjectionInlineConfigurator<TSourceElement, TResultElement>>>[] configurations)
        {
            var mapperContext = ((IMapperInternal)mapper).Context;
            var inlineMapperContext = mapperContext.InlineContexts.GetContextFor(configurations, this);

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
