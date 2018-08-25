namespace AgileObjects.AgileMapper.Api
{
    using System;
    using System.Linq.Expressions;
    using Configuration;

    /// <summary>
    /// Provides options for specifying the result Type to which an unflattening should be performed.
    /// </summary>
    /// <typeparam name="TSource">The Type of object from which unflattening is being performed.</typeparam>
    public interface IUnflatteningSelector<TSource>
    {
        /// <summary>
        /// Unflatten the given source object to an instance of the given <paramref name="resultType"/>.
        /// </summary>
        /// <param name="resultType">The type of object to create from the specified source object.</param>
        /// <returns>The result of the unflattening.</returns>
        object To(Type resultType);

        /// <summary>
        /// Unflatten the given source object to a <typeparamref name="TResult"/> instance.
        /// </summary>
        /// <typeparam name="TResult">The type of object to create from the specified source object.</typeparam>
        /// <param name="configurations">
        /// Zero or more mapping configurations. If supplied, the mapping will be configured by combining these 
        /// inline <paramref name="configurations"/> with any configuration already set up via the 
        /// Mapper.WhenMapping API.
        /// </param>
        /// <returns>
        /// The <typeparamref name="TResult"/> created by unflattening the source object.
        /// </returns>
        TResult To<TResult>(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>[] configurations);
    }
}