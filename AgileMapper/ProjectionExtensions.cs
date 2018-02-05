namespace AgileObjects.AgileMapper
{
    using System.Linq;
    using Api;

    /// <summary>
    /// Provides extension methods to support projecting an IQueryable to an IQueryable of a different type.
    /// </summary>
    public static class ProjectionExtensions
    {
        /// <summary>
        /// Project the elements of the given <paramref name="sourceQueryable"/> to instances of a specified 
        /// result Type, using the default mapper. The projection operation is performed entirely on the data source.
        /// </summary>
        /// <typeparam name="TSourceElement">The Type of the elements to project to a new result Type.</typeparam>
        /// <param name="sourceQueryable">The source IQueryable{T} on which to perform the projection.</param>
        /// <returns>An IProjectionResultSpecifier with which to specify the type of query projection to perform.</returns>
        public static IProjectionResultSpecifier<TSourceElement> Project<TSourceElement>(
            this IQueryable<TSourceElement> sourceQueryable)
        {
            return new ProjectionExecutor<TSourceElement>(Mapper.Default.Context, sourceQueryable);
        }

        /// <summary>
        /// Project the elements of the given <paramref name="sourceQueryable"/> to instances of a specified 
        /// result Type, using the given <paramref name="mapper"/>. The projection operation is performed 
        /// entirely on the data source.
        /// </summary>
        /// <typeparam name="TSourceElement">The Type of the elements to project to a new result Type.</typeparam>
        /// <param name="sourceQueryable">The source IQueryable{T} on which to perform the projection.</param>
        /// <param name="mapper">The mapper with which the projection should be performed.</param>
        /// <returns>An IProjectionResultSpecifier with which to specify the type of query projection to perform.</returns>
        public static IProjectionResultSpecifier<TSourceElement> ProjectUsing<TSourceElement>(
            this IQueryable<TSourceElement> sourceQueryable,
            IMapper mapper)
        {
            return new ProjectionExecutor<TSourceElement>(((IMapperInternal)mapper).Context, sourceQueryable);
        }
    }
}
