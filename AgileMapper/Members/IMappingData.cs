namespace AgileObjects.AgileMapper.Members
{
    using System;

    /// <summary>
    /// Provides the data being used at a particular point during a mapping.
    /// </summary>
    public interface IMappingData
    {
        /// <summary>
        /// Gets the data of the mapping context directly 'above' that described by the 
        /// <see cref="IMappingData"/>.
        /// </summary>
        IMappingData Parent { get; }

        /// <summary>
        /// Gets the source object for the mapping context described by the 
        /// <see cref="IMappingData"/>, cast to the given <typeparamref name="TSource">type</typeparamref>.
        /// </summary>
        /// <typeparam name="TSource">The type to which to cast the source object.</typeparam>
        /// <returns>The source object for the mapping context.</returns>
        TSource GetSource<TSource>();

        /// <summary>
        /// Gets the target object for the mapping context described by the 
        /// <see cref="IMappingData"/>, cast to the given <typeparamref name="TTarget">type</typeparamref>.
        /// </summary>
        /// <typeparam name="TTarget">The type to which to cast the target object.</typeparam>
        /// <returns>The target object for the mapping context.</returns>
        TTarget GetTarget<TTarget>();

        /// <summary>
        /// Gets the index of the current enumerable element being mapped in the mapping context
        /// described by this <see cref="IMappingData"/>, if applicable.
        /// </summary>
        /// <returns>
        /// The index of the current enumerable element being mapped in the mapping context described
        /// by this <see cref="IMappingData"/>, otherwise null.
        /// </returns>
        [Obsolete("Use GetElementIndex() instead. This method will be removed in a future release.")]
        int? GetEnumerableIndex();

        /// <summary>
        /// Gets the index of the current enumerable element being mapped in the mapping context
        /// described by this <see cref="IMappingData"/>, if applicable.
        /// </summary>
        /// <returns>
        /// The index of the current enumerable element being mapped in the mapping context described
        /// by this <see cref="IMappingData"/> if applicable, otherwise null.
        /// </returns>
        int? GetElementIndex();

        /// <summary>
        /// Gets the key of the current Dictionary KeyValuePair being mapped in the mapping context
        /// described by this <see cref="IMappingData"/>, if applicable.
        /// </summary>
        /// <returns>
        /// The key of the current Dictionary KeyValuePair being mapped in the mapping context
        /// described by this <see cref="IMappingData"/> if applicable, otherwise null.
        /// </returns>
        object GetElementKey();

        /// <summary>
        /// Gets the <see cref="IMappingData"/> as a typed <see cref="IMappingData{TSource, TTarget}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of source object being mapped in the current context.</typeparam>
        /// <typeparam name="TTarget">The type of target object being mapped in the current context.</typeparam>
        /// <returns>The <see cref="IMappingData"/> as a typed <see cref="IMappingData{TSource, TTarget}"/>.</returns>
        IMappingData<TSource, TTarget> As<TSource, TTarget>()
            where TSource : class where TTarget : class;
    }

    /// <summary>
    /// Provides the data being used at a particular point during a mapping.
    /// </summary>
    /// <typeparam name="TSource">The type of source object being mapped from in the current context.</typeparam>
    /// <typeparam name="TTarget">The type of target object being mapped to in the current context.</typeparam>
    public interface IMappingData<out TSource, out TTarget> : IServiceProviderAccessor
    {
        /// <summary>
        /// Gets the data of the mapping context directly 'above' that described by the 
        /// <see cref="IMappingData{TSource, TTarget}"/>.
        /// </summary>
        IMappingData Parent { get; }

        /// <summary>
        /// Gets the source object for the mapping context described by the 
        /// <see cref="IMappingData{TSource, TTarget}"/>.
        /// </summary>
        TSource Source { get; }

        /// <summary>
        /// Gets the target object for the mapping context described by the 
        /// <see cref="IMappingData{TSource, TTarget}"/>.
        /// </summary>
        TTarget Target { get; }

        /// <summary>
        /// Gets the index of the current enumerable element being mapped in the mapping context
        /// described by this <see cref="IMappingData{TSource, TTarget}"/>, if applicable.
        /// </summary>
        [Obsolete("Use ElementIndex instead. This property will be removed in a future release.")]
        int? EnumerableIndex { get; }

        /// <summary>
        /// Gets the index of the current enumerable element being mapped in the mapping context
        /// described by this <see cref="IMappingData{TSource, TTarget}"/>, if applicable.
        /// </summary>
        int? ElementIndex { get; }
        
        /// <summary>
        /// Gets the key of the current Dictionary KeyValuePair being mapped in the mapping context
        /// described by this <see cref="IMappingData{TSource, TTarget}"/>, if applicable.
        /// </summary>
        object ElementKey { get; }
    }
}