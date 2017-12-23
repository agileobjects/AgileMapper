namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Api.Configuration;

    /// <summary>
    /// Provides options for selecting the type of flattening to perform on an object.
    /// </summary>
    /// <typeparam name="TSource">The Type of object to be flattened.</typeparam>
    public interface IFlatteningSelector<TSource>
    {
        /// <summary>
        /// Flatten to an ExpandoObject using the default <see cref="IMapper"/>.
        /// </summary>
        /// <returns>An ExpandoObject dynamic containing the flattened source object.</returns>
        dynamic ToDynamic();

        /// <summary>
        /// Flatten to an IDictionary{string, object} using the default <see cref="IMapper"/>.
        /// </summary>
        /// <returns>
        /// An IDictionary{string, object} implementation containing the flattened source object.
        /// </returns>
        Dictionary<string, object> ToDictionary();

        /// <summary>
        /// Flatten to an IDictionary{string, object} using the default <see cref="IMapper"/> and the given 
        /// <paramref name="configurations"/>.
        /// </summary>
        /// <param name="configurations">
        /// One or more mapping configurations. The mapping will be configured by combining these inline 
        /// <paramref name="configurations"/> with any configuration already set up via the Mapper.WhenMapping 
        /// API.
        /// </param>
        /// <returns>
        /// An IDictionary{string, object} implementation containing the flattened source object.
        /// </returns>
        Dictionary<string, object> ToDictionary(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, Dictionary<string, object>>>>[] configurations);

        /// <summary>
        /// Flatten to an IDictionary{string, TValue} using the default <see cref="IMapper"/>.
        /// </summary>
        /// <typeparam name="TValue">
        /// The Type of objects to store in the result IDictionary{string, TValue}. Values which cannot
        /// be converted to this Type will be ignored.
        /// </typeparam>
        /// <returns>
        /// An IDictionary{string, TValue} implementation containing the flattened source object.
        /// </returns>
        Dictionary<string, TValue> ToDictionary<TValue>();

        /// <summary>
        /// Flatten to an IDictionary{string, TValue} using the default <see cref="IMapper"/> and the given 
        /// <paramref name="configurations"/>.
        /// </summary>
        /// <typeparam name="TValue">
        /// The Type of objects to store in the result IDictionary{string, TValue}. Values which cannot be 
        /// converted to this Type will be ignored.
        /// </typeparam>
        /// <param name="configurations">
        /// One or more mapping configurations. The mapping will be configured by combining these inline 
        /// <paramref name="configurations"/> with any configuration already set up via the Mapper.WhenMapping 
        /// API.
        /// </param>
        /// <returns>
        /// An IDictionary{string, TValue} implementation containing the flattened source object.
        /// </returns>
        Dictionary<string, TValue> ToDictionary<TValue>(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, Dictionary<string, TValue>>>>[] configurations);

        /// <summary>
        /// Flatten the source object into an ampersand-separted, key=value pair query string format, using the 
        /// default <see cref="IMapper"/>. The value is returned without a leading question mark.
        /// </summary>
        /// <returns>The flattened query-string-formatted source object data.</returns>
        string ToQueryString();

        /// <summary>
        /// Flatten the source object into an ampersand-separted, key=value pair query string format, using the 
        /// default <see cref="IMapper"/> and the given <paramref name="configurations"/>. The value is returned 
        /// without a leading question mark.
        /// </summary>
        /// <param name="configurations">
        ///     One or more mapping configurations. The mapping will be configured by combining these inline 
        ///     <paramref name="configurations"/> with any configuration already set up via the Mapper.WhenMapping API.
        /// </param>
        /// <returns>The flattened query-string-formatted source object data.</returns>
        string ToQueryString(
            params Expression<Action<IFullMappingInlineConfigurator<TSource, Dictionary<string, string>>>>[] configurations);
    }
}