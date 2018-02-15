namespace AgileObjects.AgileMapper.Api
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Configuration.Projection;

    /// <summary>
    /// Provides options for specifying the query projection result Type.
    /// </summary>
    /// <typeparam name="TSourceElement">
    /// The Type of object contained in the source IQueryable{T} which should be projected
    /// to a result Type.
    /// </typeparam>
    public interface IProjectionResultSpecifier<TSourceElement>
    {
        /// <summary>
        /// Project the elements of the source IQueryable{T} to instances of the given 
        /// <typeparamref name="TResultElement"/>, using the default mapper.
        /// </summary>
        /// <typeparam name="TResultElement">
        /// The result Type to which the elements of the source IQueryable{T} should be projected.
        /// </typeparam>
        /// <returns>
        /// An IQueryable{TResultElement} of the source IQueryable{T} projected to instances of the given 
        /// <typeparamref name="TResultElement"/>. The projection is not performed until the Queryable is 
        /// enumerated by a call to .ToArray() or similar.
        /// </returns>
        IQueryable<TResultElement> To<TResultElement>();

        /// <summary>
        /// Project the elements of the source IQueryable{T} to instances of the given 
        /// <typeparamref name="TResultElement"/>, using the default mapper and the given 
        /// <paramref name="configuration"/>.
        /// </summary>
        /// <typeparam name="TResultElement">
        /// The result Type to which the elements of the source IQueryable{T} should be projected.
        /// </typeparam>
        /// <param name="configuration">
        /// An inline query projection configuration. If non-null, the query projection will be configured 
        /// by combining this inline <paramref name="configuration"/> with any applicable configuration 
        /// already set up via the Mapper.WhenMapping API.
        /// </param>
        /// <returns>
        /// An IQueryable{TResultElement} of the source IQueryable{T} projected to instances of the given 
        /// <typeparamref name="TResultElement"/>. The projection is not performed until the Queryable is 
        /// enumerated by a call to .ToArray() or similar.
        /// </returns>
        IQueryable<TResultElement> To<TResultElement>(
            Expression<Action<IFullProjectionInlineConfigurator<TSourceElement, TResultElement>>> configuration);
    }
}