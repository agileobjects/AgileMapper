namespace AgileObjects.AgileMapper.Api
{
    using System;
    using Configuration;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides options for specifying the type of mapping to perform.
    /// </summary>
    /// <typeparam name="TSource">The type of source object from which mapping is being performed.</typeparam>
    public interface ITargetSelector<TSource>
    {
        /// <summary>
        /// Create an instance of the given <paramref name="resultType"/> from the specified source object.
        /// </summary>
        /// <param name="resultType">The type of object to create from the specified source object.</param>
        /// <returns>The result of the new object mapping.</returns>
        object ToANew(Type resultType);

        /// <summary>
        /// Create an instance of <typeparamref name="TResult"/> from the specified source object.
        /// </summary>
        /// <typeparam name="TResult">The type of object to create from the specified source object.</typeparam>
        /// <returns>The result of the new object mapping.</returns>
        TResult ToANew<TResult>();

        /// <summary>
        /// Create an instance of <typeparamref name="TResult"/> from the specified source object, using the given 
        /// <paramref name="configuration"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of object to create from the specified source object.</typeparam>
        /// <param name="configuration">
        /// An inline mapping configuration. If non-null, the mapping will be configured by combining this inline 
        /// <paramref name="configuration"/> with any configuration already set up via the  Mapper.WhenMapping API.
        /// </param>
        /// <returns>The result of the new object mapping.</returns>
        TResult ToANew<TResult>(
            Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>> configuration);

        /// <summary>
        /// Create an instance of <typeparamref name="TResult"/> from the specified source object, using the given 
        /// <paramref name="configurations"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of object to create from the specified source object.</typeparam>
        /// <param name="configurations">
        /// Zero or more mapping configurations. If supplied, the mapping will be configured by combining 
        /// these inline <paramref name="configurations"/> with any configuration already set up via the 
        /// Mapper.WhenMapping API.
        /// </param>
        /// <returns>The result of the new object mapping.</returns>
        TResult ToANew<TResult>(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>[] configurations);

        /// <summary>
        /// Merge the specified source object on to the given <paramref name="existing"/> object.
        /// </summary>
        /// <typeparam name="TTarget">The type of object on which to perform the mapping.</typeparam>
        /// <param name="existing">The object on which to perform the mapping.</param>
        /// <returns>The mapped object.</returns>
        TTarget OnTo<TTarget>(TTarget existing);

        /// <summary>
        /// Merge the specified source object on to the given <paramref name="existing"/> object, using the given 
        /// <paramref name="configuration"/>.
        /// </summary>
        /// <typeparam name="TTarget">The type of object on which to perform the mapping.</typeparam>
        /// <param name="existing">The object on which to perform the mapping.</param>
        /// <param name="configuration">
        /// An inline mapping configuration. If non-null, the mapping will be configured by combining this inline 
        /// <paramref name="configuration"/> with any configuration already set up via the Mapper.WhenMapping API.
        /// </param>
        /// <returns>The mapped object.</returns>
        TTarget OnTo<TTarget>(
            TTarget existing,
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>> configuration);

        /// <summary>
        /// Merge the specified source object on to the given <paramref name="existing"/> object, using the given 
        /// <paramref name="configurations"/>.
        /// </summary>
        /// <typeparam name="TTarget">The type of object on which to perform the mapping.</typeparam>
        /// <param name="existing">The object on which to perform the mapping.</param>
        /// <param name="configurations">
        /// Zero or more mapping configurations. If supplied, the mapping will be configured by combining 
        /// these inline <paramref name="configurations"/> with any configuration already set up via the 
        /// Mapper.WhenMapping API.
        /// </param>
        /// <returns>The mapped object.</returns>
        TTarget OnTo<TTarget>(
            TTarget existing,
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations);

        /// <summary>
        /// Update the given <paramref name="existing"/> object using values from the specified source object.
        /// </summary>
        /// <typeparam name="TTarget">The type of object on which to perform the mapping.</typeparam>
        /// <param name="existing">The object on which to perform the mapping.</param>
        /// <returns>The mapped object.</returns>
        TTarget Over<TTarget>(TTarget existing);

        /// <summary>
        /// Update the given <paramref name="existing"/> object using values from the specified source object, 
        /// using the given <paramref name="configuration"/>.
        /// </summary>
        /// <typeparam name="TTarget">The type of object on which to perform the mapping.</typeparam>
        /// <param name="existing">The object on which to perform the mapping.</param>
        /// <param name="configuration">
        /// An inline mapping configuration. If non-null, the mapping will be configured by combining this inline 
        /// <paramref name="configuration"/> with any configuration already set up via the Mapper.WhenMapping API.
        /// </param>
        /// <returns>The mapped object.</returns>
        TTarget Over<TTarget>(
            TTarget existing,
            Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>> configuration);

        /// <summary>
        /// Update the given <paramref name="existing"/> object using values from the specified source object, 
        /// using any given <paramref name="configurations"/>.
        /// </summary>
        /// <typeparam name="TTarget">The type of object on which to perform the mapping.</typeparam>
        /// <param name="existing">The object on which to perform the mapping.</param>
        /// <param name="configurations">
        /// Zero or more mapping configurations. If supplied, the mapping will be configured by combining 
        /// these inline <paramref name="configurations"/> with any configuration already set up via the 
        /// Mapper.WhenMapping API.
        /// </param>
        /// <returns>The mapped object.</returns>
        TTarget Over<TTarget>(
            TTarget existing,
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations);
    }
}
