namespace AgileObjects.AgileMapper.Queryables.Api
{
    /// <summary>
    /// Provides the option to supply a particular <see cref="IMapper"/> with which to perform a query projection.
    /// </summary>
    public class ProjectionMapperSelector
    {
        internal static readonly ProjectionMapperSelector Instance = new ProjectionMapperSelector();

        private ProjectionMapperSelector()
        {
        }

        /// <summary>
        /// Use the given <see cref="IMapper"/> in this query projection.
        /// </summary>
        /// <param name="mapper">The <see cref="IMapper"/> to use in this query projection.</param>
        /// <returns>The <see cref="IMapper"/> to use in this query projection.</returns>
        public IMapper Using(IMapper mapper) => mapper;
    }
}