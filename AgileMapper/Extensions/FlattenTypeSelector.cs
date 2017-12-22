namespace AgileObjects.AgileMapper.Extensions
{
    using System.Dynamic;

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
    }
}