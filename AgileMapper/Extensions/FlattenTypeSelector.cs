namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;

    /// <summary>
    /// Provides options for selecting the type of flattening to perform on an object.
    /// </summary>
    /// <typeparam name="TSource">The Type of object to be flattened.</typeparam>
    public class FlattenTypeSelector<TSource>
    {
        private readonly TSource _source;
        private readonly IMapper _mapper;

        internal FlattenTypeSelector(TSource source, IMapper mapper)
        {
            _source = source;
            _mapper = mapper;
        }

        /// <summary>
        /// Flatten to an ExpandoObject using the default <see cref="IMapper"/>.
        /// </summary>
        /// <returns>An ExpandoObject dynamic containing the flattened source object.</returns>
        public dynamic ToDynamic() => _mapper.Map(_source).ToANew<ExpandoObject>();

        /// <summary>
        /// Flatten to an IDictionary{string, object} using the default <see cref="IMapper"/>.
        /// </summary>
        /// <returns>
        /// An IDictionary{string, object} implementation containing the flattened source object.
        /// </returns>
        public IDictionary<string, object> ToDictionary()
            => _mapper.Map(_source).ToANew<Dictionary<string, object>>();

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
        public IDictionary<string, TValue> ToDictionary<TValue>()
            => _mapper.Map(_source).ToANew<Dictionary<string, TValue>>();

        /// <summary>
        /// Flatten the source object into an ampersand-separted, key=value pair, query string format, using the 
        /// default <see cref="IMapper"/>. The value is returned without a leading question mark.
        /// </summary>
        /// <returns>The flattened query-string-formatted source object data.</returns>
        public string ToQueryString()
        {
            var flattened = ToDictionary<string>();

            var queryString = string.Join(
                "&",
                flattened.Select(kvp =>
                    Uri.EscapeUriString(kvp.Key) + "=" + Uri.EscapeUriString(kvp.Value)));

            return queryString;
        }
    }
}