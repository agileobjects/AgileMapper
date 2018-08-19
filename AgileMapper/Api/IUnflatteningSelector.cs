namespace AgileObjects.AgileMapper.Api
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides options for specifying the result Type to which an unflattening should be performed.
    /// </summary>
    public interface IUnflatteningSelector
    {
        /// <summary>
        /// Unflatten the given source object to a <typeparamref name="TResult"/> instance.
        /// </summary>
        /// <typeparam name="TResult">The Type of the source dictionary's value objects.</typeparam>
        /// <returns>
        /// The <typeparamref name="TResult"/> created by unflattening the source object.
        /// </returns>
        TResult To<TResult>();
    }
}