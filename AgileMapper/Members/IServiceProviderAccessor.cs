namespace AgileObjects.AgileMapper.Members
{
    using Configuration;

    /// <summary>
    /// Provides access to service-provider methods or a configured Service Provider implementation.
    /// </summary>
    public interface IServiceProviderAccessor
    {
        /// <summary>
        /// Use the registered service provider to resolve an instance of the given <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The Type of service to resolve.</typeparam>
        /// <returns>
        /// The <typeparamref name="TService"/> instance resolved by the registered service provider.
        /// </returns>
        TService GetService<TService>()
            where TService : class;

        /// <summary>
        /// Use the registered service provider to resolve the instance of the given <typeparamref name="TService"/>
        /// with the given <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="TService">The Type of service to resolve.</typeparam>
        /// <param name="name">The name of the registered service instance to resolve.</param>
        /// <returns>
        /// The named <typeparamref name="TService"/> instance resolved by the registered service provider.
        /// </returns>
        TService GetService<TService>(string name)
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