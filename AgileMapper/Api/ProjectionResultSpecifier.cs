namespace AgileObjects.AgileMapper.Api
{
    using System;
    using System.Linq;
    using ObjectPopulation;
    using Queryables.Api;

    /// <summary>
    /// Provides options for specifying the query projection result Type.
    /// </summary>
    /// <typeparam name="TSourceElement">
    /// The Type of object contained in the source IQueryable{T} which should be projected
    /// to a result Type.
    /// </typeparam>
    public class ProjectionResultSpecifier<TSourceElement>
    {
        private readonly IQueryable<TSourceElement> _sourceQueryable;

        internal ProjectionResultSpecifier(IQueryable<TSourceElement> sourceQueryable)
        {
            _sourceQueryable = sourceQueryable;
        }

        /// <summary>
        /// Project the elements of the source IQueryable{T} to instances of the given 
        /// <typeparamref name="TResultElement"/>, using the default mapper.
        /// </summary>
        /// <typeparam name="TResultElement">
        /// The target Type to which the elements of the source IQueryable{T} should be projected.
        /// </typeparam>
        /// <returns>
        /// An IQueryable{TResultElement} of the source IQueryable{T} projected to instances of the given 
        /// <typeparamref name="TResultElement"/>. The projection is not performed until the Queryable is 
        /// enumerated by a call to .ToArray() or similar.
        /// </returns>
        public IQueryable<TResultElement> To<TResultElement>()
            where TResultElement : class
        {
            return ProjectQuery<TResultElement>(Mapper.Default);
        }

        /// <summary>
        /// Project the elements of the source IQueryable{T} to instances of the given 
        /// <typeparamref name="TResultElement"/>, using the mapper specified by the given 
        /// <paramref name="mapperSelector"/>.
        /// </summary>
        /// <param name="mapperSelector">A func providing the mapper with which the projection should be performed.</param>
        /// <typeparam name="TResultElement">
        /// The target Type to which the elements of the source IQueryable{T} should be projected.
        /// </typeparam>
        /// <returns>
        /// An IQueryable{TResultElement} of the source IQueryable{T} projected to instances of the given 
        /// <typeparamref name="TResultElement"/>. The projection is not performed until the Queryable is 
        /// enumerated by a call to .ToArray() or similar.
        /// </returns>
        public IQueryable<TResultElement> To<TResultElement>(Func<ProjectionMapperSelector, IMapper> mapperSelector)
        {
            var mapper = mapperSelector.Invoke(ProjectionMapperSelector.Instance);

            return ProjectQuery<TResultElement>(mapper);
        }

        //public static IQueryable<TResultElement> To<TResultElement>(
        //    Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>> configuration)
        //{

        //}

        private IQueryable<TResultElement> ProjectQuery<TResultElement>(IMapper mapper)
        {
            var mapperContext = ((IMapperInternal)mapper).Context;

            var rootMappingData = ObjectMappingDataFactory.ForProjection<TSourceElement, TResultElement>(
                _sourceQueryable,
                mapperContext.QueryProjectionMappingContext);

            var queryProjection = rootMappingData.MapStart();

            return queryProjection;
        }
    }
}
