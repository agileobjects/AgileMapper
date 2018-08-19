namespace AgileObjects.AgileMapper.Api
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides options for supplying a source object to be unflattened into thean instance of the given
    /// <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The Type of object to which unflattening will be performed.</typeparam>
    public class UnflatteningSelector<TResult>
    {
        private readonly IMapper _mapper;

        internal UnflatteningSelector(IMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// Unflatten the given <paramref name="sourceDictionary"/> to a <typeparamref name="TResult"/> instance.
        /// </summary>
        /// <typeparam name="TValue">The Type of the source dictionary's value objects.</typeparam>
        /// <param name="sourceDictionary">The source dictionary to unflatten.</param>
        /// <returns>
        /// The <typeparamref name="TResult"/> created by unflattening the <paramref name="sourceDictionary"/>.
        /// </returns>
        public TResult From<TValue>(IDictionary<string, TValue> sourceDictionary)
            => _mapper.Map(sourceDictionary).ToANew<TResult>();
    }
}