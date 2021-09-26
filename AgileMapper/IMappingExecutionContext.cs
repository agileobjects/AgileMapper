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
        /// Create a new <see cref="IMappingExecutionContext"/> for the given
        /// <paramref name="sourceValue"/> and <paramref name="targetValue"/>.
        /// </summary>
        /// <typeparam name="TSourceValue">The type of the given <paramref name="sourceValue"/>. </typeparam>
        /// <typeparam name="TTargetValue">The declared type of the given <paramref name="targetValue"/>.</typeparam>
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
        /// <returns>The mapping result.</returns>
        IMappingExecutionContext AddChild<TSourceValue, TTargetValue>(
            TSourceValue sourceValue,
            TTargetValue targetValue,
            int? elementIndex,
            object elementKey,
            string targetMemberName,
            int dataSourceIndex);

        /// <summary>
        /// Create a new <see cref="IMappingExecutionContext"/> for the given
        /// <paramref name="sourceElement"/> and <paramref name="targetElement"/>.
        /// </summary>
        /// <typeparam name="TSourceElement">The type of the given <paramref name="sourceElement"/>. </typeparam>
        /// <typeparam name="TTargetElement">The declared type of the given <paramref name="targetElement"/>.</typeparam>
        /// <param name="sourceElement">The source object from which to map.</param>
        /// <param name="targetElement">The target object to which to map.</param>
        /// <param name="elementIndex">
        /// The index of the current enumerable <paramref name="sourceElement"/> being mapped in the
        /// mapping context to be described by the new <see cref="IMappingExecutionContext"/>, if
        /// applicable.
        /// </param>
        /// <param name="elementKey">
        /// The key of the current Dictionary KeyValuePair being mapped in the mapping context
        /// to be described by the new <see cref="IMappingExecutionContext"/>, if applicable.
        /// </param>
        /// <returns>The mapping result.</returns>
        IMappingExecutionContext AddElement<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int elementIndex,
            object elementKey);

        /// <summary>
        /// Use the given <paramref name="context"/> to map a source and target object with types
        /// which need to be determined at runtime.
        /// </summary>
        /// <param name="context">
        /// The <see cref="IMappingExecutionContext"/> describing the current context in which
        /// mapping is being performed.
        /// </param>
        /// <returns>The mapping result.</returns>
        object Map(IMappingExecutionContext context);

        /// <summary>
        /// Use the given <paramref name="context"/> to map a source and target object with types
        /// mapped multiple times within the object graph.
        /// </summary>
        /// <param name="context">
        /// The <see cref="IMappingExecutionContext"/> describing the current context in which
        /// mapping is being performed.
        /// </param>
        /// <returns>The mapping result.</returns>
        object MapRepeated(IMappingExecutionContext context);
    }
}