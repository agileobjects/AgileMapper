namespace AgileObjects.AgileMapper
{
    /// <summary>
    /// Provides services required during a mapping.
    /// </summary>
    public interface IMappingExecutionContext
    {
        /// <summary>
        /// Returns a value indicating if the given <paramref name="key">source object</paramref>
        /// has already been mapped to a target object of the given 
        /// <typeparamref name="TComplex">target type</typeparamref>. If so, the previous mapping
        /// result is assigned to the <paramref name="complexType"/> parameter.
        /// </summary>
        /// <typeparam name="TKey">
        /// The type of source object for which to make the determination.
        /// </typeparam>
        /// <typeparam name="TComplex">
        /// The type of target object for which to make the determination.</typeparam>
        /// <param name="key">The source object for which to make the determination.</param>
        /// <param name="complexType">
        /// The target object to which to assign the already-mapped result object, if applicable.
        /// </param>
        /// <returns>
        /// True if the given <paramref name="key">source object</paramref> has already been mapped 
        /// to a target object of the given <typeparamref name="TComplex">target type</typeparamref>,
        /// otherwise false.
        /// </returns>
        bool TryGet<TKey, TComplex>(TKey key, out TComplex complexType)
            where TComplex : class;

        /// <summary>
        /// Registers the given <paramref name="complexType">target object</paramref> as the result of
        /// mapping the given <paramref name="key">source object</paramref>.
        /// </summary>
        /// <typeparam name="TKey">The type of source object to register.</typeparam>
        /// <typeparam name="TComplex">The type of target object to register.</typeparam>
        /// <param name="key">The source object to register.</param>
        /// <param name="complexType">The result target object to register.</param>
        void Register<TKey, TComplex>(TKey key, TComplex complexType);
    }
}