namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    /// <summary>
    /// Provides untyped services available at a particular point in a mapping.
    /// </summary>
    public interface IObjectMappingDataUntyped : IMappingData
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
        /// Map the given <paramref name="sourceValue"/> to the given <paramref name="targetValue"/> as 
        /// part of the mapping of a recursive relationship.
        /// </summary>
        /// <typeparam name="TDeclaredSource">
        /// The declared type of the given <paramref name="sourceValue"/>.
        /// </typeparam>
        /// <typeparam name="TDeclaredTarget">
        /// The declared type of the given <paramref name="targetValue"/>.
        /// </typeparam>
        /// <param name="sourceValue">The source object from which to map.</param>
        /// <param name="targetValue">The target object to which to map.</param>
        /// <param name="enumerableIndex">
        /// The index of the current enumerable being mapped in the mapping context described by the 
        /// <see cref="IObjectMappingDataUntyped"/>, if applicable.
        /// </param>
        /// <param name="targetMemberName">The name of the target member being mapped.</param>
        /// <param name="dataSourceIndex">
        /// The index of the data source being used to perform the mapping.
        /// </param>
        /// <returns>The mapping result.</returns>
        TDeclaredTarget MapRecursion<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceValue,
            TDeclaredTarget targetValue,
            int? enumerableIndex,
            string targetMemberName,
            int dataSourceIndex);

        /// <summary>
        /// Gets the <see cref="IObjectMappingDataUntyped"/> typed as a 
        /// <see cref="IObjectMappingData{TNewSource, TNewTarget}"/>.
        /// </summary>
        /// <typeparam name="TNewSource">The type of source object being mapped in the current context.</typeparam>
        /// <typeparam name="TNewTarget">The type of target object being mapped in the current context.</typeparam>
        /// <param name="isForDerivedTypeMapping">
        /// Whether the new, typed <see cref="IObjectMappingData{TNewSource, TNewTarget}"/> is needed for the creation
        /// of a derived type mapping.
        /// </param>
        /// <returns>
        /// The <see cref="IObjectMappingDataUntyped"/> typed as a 
        /// <see cref="IObjectMappingData{TNewSource, TNewTarget}"/>.
        /// </returns>
        IObjectMappingData<TNewSource, TNewTarget> As<TNewSource, TNewTarget>(bool isForDerivedTypeMapping)
            where TNewSource : class where TNewTarget : class;
    }
}