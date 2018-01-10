namespace AgileObjects.AgileMapper.Api
{
    using System;
    using System.Linq.Expressions;
    using Configuration;

    /// <summary>
    /// Provides options for specifying the type of mapping to perform.
    /// </summary>
    /// <typeparam name="TSource">The type of source object from which mapping is being performed.</typeparam>
    public interface ITargetSelector<TSource>
    {
        /// <summary>
        /// Perform a new object mapping using any given <paramref name="configurations"/>.
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
        /// Perform an OnTo (merge) mapping using any given <paramref name="configurations"/>.
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
        /// Perform an Over (overwrite) mapping using any given <paramref name="configurations"/>.
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
