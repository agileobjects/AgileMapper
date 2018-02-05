namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq;
    using Api;
    using Queryables.Api;

    /// <summary>
    /// Provides extension methods to support projecting an IQueryable to an IQueryable of a different type.
    /// </summary>
    public static class ProjectionExtensions
    {
        /// <summary>
        /// Project the elements of the given <paramref name="sourceQueryable"/> to instances of a specified 
        /// result Type, using a mapper provided by a given <paramref name="mapperSelector"/>, or the default
        /// mapper if none is supplied. The projection operation is performed entirely on the data source.
        /// </summary>
        /// <typeparam name="TSourceElement">The Type of the elements to project to a new result Type.</typeparam>
        /// <param name="sourceQueryable">The source IQueryable{T} on which to perform the projection.</param>
        /// <param name="mapperSelector">
        /// A func providing the mapper with which the projection should be performed. If not supplied, the default
        /// mapper will be used.
        /// </param>
        /// <returns>An IProjectionResultSpecifier with which to specify the type of query projection to perform.</returns>
        public static IProjectionResultSpecifier<TSourceElement> Project<TSourceElement>(
            this IQueryable<TSourceElement> sourceQueryable,
            Func<ProjectionMapperSelector, IMapper> mapperSelector = null)
        {
            MapperContext mapperContext;

            if (mapperSelector != null)
            {
                var mapper = mapperSelector.Invoke(ProjectionMapperSelector.Instance);
                mapperContext = ((IMapperInternal)mapper).Context;
            }
            else
            {
                mapperContext = Mapper.Default.Context;
            }

            return new ProjectionExecutor<TSourceElement>(mapperContext, sourceQueryable);
        }
    }
}
