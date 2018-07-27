namespace AgileObjects.AgileMapper.Members
{
    using Configuration;

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
        /// Gets the index of the current enumerable being mapped in the mapping context described by the 
        /// <see cref="IMappingData"/>, if applicable.
        /// </summary>
        /// <returns>
        /// The index of the current enumerable being mapped in the mapping context described by the 
        /// <see cref="IMappingData"/>, otherwise null.
        /// </returns>
        int? GetEnumerableIndex();

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
    public interface IMappingData<out TSource, out TTarget>
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
        /// Gets the index of the current enumerable being mapped in the mapping context described by the 
        /// <see cref="IMappingData{TSource, TTarget}"/>, if applicable.
        /// </summary>
        int? EnumerableIndex { get; }

        /// <summary>
        /// Use the registered service provider to resolve an instance of the given <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The Type of service to resolve.</typeparam>
        /// <param name="name">The name of the registered service instance to resolve, if applicable.</param>
        /// <returns>
        /// The <typeparamref name="TService"/> instance resolved by the registered service provider.
        /// </returns>
        TService GetService<TService>(string name = null)
            where TService : class;

        /// <summary>
        /// Retrieve a previously-registered service provider object of type <typeparamref name="TServiceProvider"/>.
        /// If no service provider object of the given type exists a <see cref="MappingConfigurationException"/> is thrown.
        /// </summary>
        /// <typeparam name="TServiceProvider">The type of previously-registered service provider object to retrieve.</typeparam>
        /// <returns>The previously-registered service provider object of type <typeparamref name="TServiceProvider"/>.</returns>
        TServiceProvider GetServiceProvider<TServiceProvider>()
            where TServiceProvider : class;
    }
}