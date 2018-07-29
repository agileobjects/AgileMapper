namespace AgileObjects.AgileMapper.Configuration
{
    using System.Linq;
    using Api;
    using Api.Configuration;
    using Queryables.Api;

    /// <summary>
    /// Base class for multiple, dedicated mapper configuration classes.
    /// </summary>
    public abstract class MapperConfiguration
    {
        private IMapperInternal _mapper;

        internal void ApplyTo(IMapperInternal mapper)
        {
            _mapper = mapper;

            Configure();
        }

        /// <summary>
        /// Configure how mappings should be performed.
        /// </summary>
        protected abstract void Configure();

        /// <summary>
        /// Use the previously-registered service provider to resolve the instance of the given
        /// <typeparamref name="TService"/>, optionally with the given <paramref name="name"/>. Register
        /// a service provider using Mapper.WhenMapping.UseServiceProvider().
        /// </summary>
        /// <typeparam name="TService">The Type of service to resolve.</typeparam>
        /// <param name="name">The name of the registered service instance to resolve.</param>
        /// <returns>
        /// The named <typeparamref name="TService"/> instance resolved by the registered service provider.
        /// </returns>
        protected TService GetService<TService>(string name = null)
            where TService : class
        {
            return _mapper.Context.UserConfigurations.GetServiceOrThrow<TService>(name);
        }

        /// <summary>
        /// Retrieve a previously-registered service provider object of type <typeparamref name="TServiceProvider"/>.
        /// If no service provider object of the given type exists, a <see cref="MappingConfigurationException"/> is
        /// thrown. Register a service provider using Mapper.WhenMapping.UseServiceProvider().
        /// </summary>
        /// <typeparam name="TServiceProvider">The type of previously-registered service provider object to retrieve.</typeparam>
        /// <returns>The previously-registered service provider object of type <typeparamref name="TServiceProvider"/>.</returns>
        protected TServiceProvider GetServiceProvider<TServiceProvider>()
            where TServiceProvider : class
        {
            return _mapper.Context.UserConfigurations.GetServiceProviderOrThrow<TServiceProvider>();
        }

        #region Configuration Members

        /// <summary>
        /// Creates a clone of the mapper being configured including all user configurations.
        /// </summary>
        /// <returns>A cloned copy of the mapper being configured.</returns>
        protected IMapper CreateNewMapper() => _mapper.CloneSelf();

        /// <summary>
        /// Create and compile a mapping function for a particular type of mapping of the source type specified by 
        /// the given <paramref name="exampleInstance"/>. Use this overload for anonymous types.
        /// </summary>
        /// <typeparam name="TSource">The type of the given <paramref name="exampleInstance"/>.</typeparam>
        /// <param name="exampleInstance">
        /// An instance specifying the source type for which a mapping plan should be created.
        /// </param>
        /// <returns>
        /// An IPlanTargetAndRuleSetSelector with which to specify the type of mapping the function for which 
        /// should be cached.
        /// </returns>
        protected IPlanTargetAndRuleSetSelector<TSource> GetPlanFor<TSource>(TSource exampleInstance)
            => _mapper.GetPlanFor(exampleInstance);

        /// <summary>
        /// Create and compile a mapping function for a particular type of mapping of the source type
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TSource">The source type for which to create the mapping functions.</typeparam>
        /// <returns>
        /// An IPlanTargetAndRuleSetSelector with which to specify the type of mapping the function for which 
        /// should be cached.
        /// </returns>
        protected IPlanTargetAndRuleSetSelector<TSource> GetPlanFor<TSource>()
            => _mapper.GetPlanFor<TSource>();

        /// <summary>
        /// Create and compile a query projection function from the source IQueryable Type specified by the given 
        /// <paramref name="exampleQueryable"/>.
        /// </summary>
        /// <typeparam name="TSourceElement">
        /// The type of element contained in the source IQueryable from which the projection function to be created will project.
        /// </typeparam>
        /// <param name="exampleQueryable">
        /// An IQueryable instance specifying the source IQueryable for which a query projection mapping plan should be created.
        /// </param>
        /// <returns>
        /// An IProjectionPlanTargetSelector with which to specify the target Type to which the query projection function to 
        /// be created should be cached.
        /// </returns>
        protected IProjectionPlanTargetSelector<TSourceElement> GetPlanForProjecting<TSourceElement>(IQueryable<TSourceElement> exampleQueryable)
            => _mapper.GetPlanForProjecting(exampleQueryable);

        /// <summary>
        /// Create and compile mapping functions for mapping from the source type specified by the given 
        /// <paramref name="exampleInstance"/>, for all mapping types (create new, merge, overwrite). Use this 
        /// overload for anonymous types.
        /// </summary>
        /// <typeparam name="TSource">The source type for which to create the mapping functions.</typeparam>
        /// <param name="exampleInstance">
        /// An instance specifying the source type for which a mapping plan should be created.
        /// </param>
        /// <returns>
        /// An IPlanTargetSelector with which to specify the target type the mapping functions for which 
        /// should be cached.
        /// </returns>
        protected IPlanTargetSelector<TSource> GetPlansFor<TSource>(TSource exampleInstance)
            => _mapper.GetPlansFor(exampleInstance);

        /// <summary>
        /// Create and compile mapping functions for the source type specified by the type argument, for all
        /// mapping types (create new, merge, overwrite).
        /// </summary>
        /// <typeparam name="TSource">The source type for which to create the mapping functions.</typeparam>
        /// <returns>
        /// An IPlanTargetSelector with which to specify the target type the mapping functions for which 
        /// should be cached.
        /// </returns>
        protected IPlanTargetSelector<TSource> GetPlansFor<TSource>() => _mapper.GetPlansFor<TSource>();

        /// <summary>
        /// Returns mapping plans for all mapping functions currently cached by the <see cref="IMapper"/> being configured.
        /// </summary>
        /// <returns>A string containing the currently-cached functions to be executed during mappings.</returns>
        protected string GetPlansInCache() => _mapper.GetPlansInCache();

        /// <summary>
        /// Configure callbacks to be executed before a particular type of event occurs for all source
        /// and target types.
        /// </summary>
        protected PreEventConfigStartingPoint Before => _mapper.Before;

        /// <summary>
        /// Configure callbacks to be executed after a particular type of event occurs for all source
        /// and target types.
        /// </summary>
        protected PostEventConfigStartingPoint After => _mapper.After;

        /// <summary>
        /// Configure how this mapper performs a mapping.
        /// </summary>
        protected MappingConfigStartingPoint WhenMapping => _mapper.WhenMapping;

        /// <summary>
        /// Throw an exception upon execution of this statement if any cached mapping plans have any target members 
        /// which will not be mapped, or map from a source enum to a target enum which does not support all of its 
        /// values. Use calls to this method to validate a mapping plan; remove them in production code.
        /// </summary>
        protected void ThrowNowIfAnyMappingPlanIsIncomplete() => _mapper.ThrowNowIfAnyMappingPlanIsIncomplete();

        #endregion
    }
}
