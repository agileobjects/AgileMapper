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

        /// <summary>
        /// Map the given <paramref name="sourceValue"/> to the given <paramref name="targetValue"/>.
        /// </summary>
        /// <typeparam name="TDeclaredSource">
        /// The declared type of the given <paramref name="sourceValue"/>.
        /// </typeparam>
        /// <typeparam name="TDeclaredTarget">
        /// The declared type of the given <paramref name="targetValue"/>.
        /// </typeparam>
        /// <param name="sourceValue">The source object from which to map.</param>
        /// <param name="targetValue">The target object to which to map.</param>
        /// <param name="targetMemberName">The name of the target member being mapped.</param>
        /// <param name="dataSourceIndex">
        /// The index of the data source being used to perform the mapping in the ser of matching
        /// data sources.
        /// </param>
        /// <param name="elementIndex">
        /// The index of the current enumerable element being mapped in the mapping context
        /// described by this <see cref="IMappingExecutionContext"/>, if applicable.
        /// </param>
        /// <param name="elementKey">
        /// The key of the current Dictionary KeyValuePair being mapped in the mapping context
        /// described by this <see cref="IMappingExecutionContext"/>, if applicable.
        /// </param>
        /// <param name="parent">
        /// The <see cref="IMappingExecutionContext"/> describing the parent context of the given
        /// <paramref name="sourceValue"/> and <paramref name="targetValue"/>.
        /// </param>
        /// <returns>The mapping result.</returns>
        TDeclaredTarget Map<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceValue,
            TDeclaredTarget targetValue,
            int? elementIndex,
            object elementKey,
            string targetMemberName,
            int dataSourceIndex,
            IMappingExecutionContext parent);

        /// <summary>
        /// Map the given <paramref name="sourceElement"/> to the given <paramref name="targetElement"/>.
        /// </summary>
        /// <typeparam name="TSourceElement">
        /// The declared type of the given <paramref name="sourceElement"/>.
        /// </typeparam>
        /// <typeparam name="TTargetElement">
        /// The declared type of the given <paramref name="targetElement"/>.
        /// </typeparam>
        /// <param name="sourceElement">The source object from which to map.</param>
        /// <param name="targetElement">The target object to which to map.</param>
        /// <param name="elementIndex">
        /// The index of the current enumerable <paramref name="sourceElement"/> being mapped in the
        /// mapping context described by this <see cref="IMappingExecutionContext"/>, if applicable.
        /// </param>
        /// <param name="elementKey">
        /// The key of the current Dictionary KeyValuePair being mapped in the mapping context
        /// described by this <see cref="IMappingExecutionContext"/>, if applicable.
        /// </param>
        /// <param name="parent">
        /// The <see cref="IMappingExecutionContext"/> describing the parent context of the given
        /// <paramref name="sourceElement"/> and <paramref name="targetElement"/>.
        /// </param>
        /// <returns>The element mapping result.</returns>
        TTargetElement Map<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int elementIndex,
            object elementKey,
            IMappingExecutionContext parent);
    }
}