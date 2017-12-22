namespace AgileObjects.AgileMapper.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Configuration;
    using Configuration.Dictionaries;

    /// <summary>
    /// Provides options for specifying the type of mapping to perform.
    /// </summary>
    /// <typeparam name="TSource">The type of source object from which mapping is being performed.</typeparam>
    public interface ITargetSelector<TSource>
    {
        /// <summary>
        /// Perform a new object mapping.
        /// </summary>
        /// <typeparam name="TResult">The type of object to create from the specified source object.</typeparam>
        /// <returns>The result of the new object mapping.</returns>
        TResult ToANew<TResult>();

        /// <summary>
        /// Perform a new object mapping using the given <paramref name="configurations"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of object to create from the specified source object.</typeparam>
        /// <param name="configurations">
        /// One or more mapping configurations. The mapping will be configured by combining these inline 
        /// <paramref name="configurations"/> with any configuration already set up via the Mapper.WhenMapping API.
        /// </param>
        /// <returns>The result of the new object mapping.</returns>
        TResult ToANew<TResult>(params Expression<Action<IFullMappingInlineConfigurator<TSource, TResult>>>[] configurations);

        /// <summary>
        /// Perform a new object to Dictionary{string, TValue} mapping.
        /// </summary>
        /// <typeparam name="TValue">
        /// The Type of objects to store in the result IDictionary{string, TValue}. Values which cannot be converted to 
        /// this Type will be ignored.
        /// </typeparam>
        /// <returns>The result of the new Dictionary mapping.</returns>
        IDictionary<string, TValue> ToANewDictionary<TValue>();

        /// <summary>
        /// Perform a new object to Dictionary{string, TValue} mapping using the given <paramref name="configurations"/>.
        /// </summary>
        /// <typeparam name="TValue">
        /// The Type of objects to store in the result IDictionary{string, TValue}. Values which cannot be converted to 
        /// this Type will be ignored.
        /// </typeparam>
        /// <param name="configurations">
        /// One or more Dictionary mapping configurations. The mapping will be configured by combining these inline 
        /// <paramref name="configurations"/> with any configuration already set up via the Mapper.WhenMapping API.
        /// </param>
        /// <returns>The result of the new Dictionary mapping.</returns>
        IDictionary<string, TValue> ToANewDictionary<TValue>(
            params Expression<Action<ITargetDictionaryMappingInlineConfigurator<TSource, TValue>>>[] configurations);

        /// <summary>
        /// Perform an OnTo (merge) mapping.
        /// </summary>
        /// <typeparam name="TTarget">The type of object on which to perform the mapping.</typeparam>
        /// <param name="existing">The object on which to perform the mapping.</param>
        /// <returns>The mapped object.</returns>
        TTarget OnTo<TTarget>(TTarget existing);

        /// <summary>
        /// Perform an OnTo (merge) mapping using the given <paramref name="configurations"/>.
        /// </summary>
        /// <typeparam name="TTarget">The type of object on which to perform the mapping.</typeparam>
        /// <param name="existing">The object on which to perform the mapping.</param>
        /// <param name="configurations">
        /// One or more mapping configurations. The mapping will be configured by combining these inline 
        /// <paramref name="configurations"/> with any configuration already set up via the Mapper.WhenMapping API.
        /// </param>
        /// <returns>The mapped object.</returns>
        TTarget OnTo<TTarget>(
            TTarget existing,
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations);

        /// <summary>
        /// Perform an Over (overwrite) mapping.
        /// </summary>
        /// <typeparam name="TTarget">The type of object on which to perform the mapping.</typeparam>
        /// <param name="existing">The object on which to perform the mapping.</param>
        /// <returns>The mapped object.</returns>
        TTarget Over<TTarget>(TTarget existing);

        /// <summary>
        /// Perform an Over (overwrite) mapping using the given <paramref name="configurations"/>.
        /// </summary>
        /// <typeparam name="TTarget">The type of object on which to perform the mapping.</typeparam>
        /// <param name="existing">The object on which to perform the mapping.</param>
        /// <param name="configurations">
        /// One or more mapping configurations. The mapping will be configured by combining these inline 
        /// <paramref name="configurations"/> with any configuration already set up via the Mapper.WhenMapping API.
        /// </param>
        /// <returns>The mapped object.</returns>
        TTarget Over<TTarget>(
            TTarget existing,
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TTarget>>>[] configurations);
    }
}
