namespace AgileObjects.AgileMapper
{
    using ObjectPopulation;

    /// <summary>
    /// Base type providing <see cref="IObjectMappingData{TSource, TTarget}"/> creation for objects
    /// of the <typeparamref name="TSource"/> type.
    /// </summary>
    /// <typeparam name="TSource">
    /// The type of source objects from which this <see cref="MappingExecutionContextBase{TSource}"/>
    /// will perform mappings.
    /// </typeparam>
    public abstract class MappingExecutionContextBase<TSource> : IMappingContext
    {
        private readonly MapperContext _mapperContext;
        private readonly TSource _source;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingExecutionContextBase{TSource}"/>
        /// class with a default MapperContext and the given <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The source object from which the mapping is to be performed.</param>
        protected MappingExecutionContextBase(TSource source)
        {
            _mapperContext = Mapper.Default.Context.ThrowIfDisposed();
            _source = source;
        }

        MapperContext IMapperContextOwner.MapperContext => _mapperContext;

        MappingRuleSet IRuleSetOwner.RuleSet => null;

        bool IMappingContext.IncludeCodeComments => false;

        bool IMappingContext.IgnoreUnsuccessfulMemberPopulations => true;

        bool IMappingContext.LazyLoadRepeatMappingFuncs => true;

        /// <summary>
        /// Create an <see cref="IObjectMappingData{TSource, TTarget}"/> object for this
        /// <see cref="MappingExecutionContextBase{TSource}"/>'s source object and the given
        /// <paramref name="target"/> object, optionally building a Mapper for the types.
        /// </summary>
        /// <typeparam name="TTarget">The type of target object to which the mapping is being performed.</typeparam>
        /// <param name="target">The target object to which the mapping is being performed.</param>
        /// <param name="createMapper">Whether a Mapper should be created for the types being mapped.</param>
        /// <returns>
        /// An <see cref="IObjectMappingData{TSource, TTarget}"/> object for this
        /// <see cref="MappingExecutionContextBase{TSource}"/>'s source object and the given
        /// <paramref name="target"/> object.
        /// </returns>
        protected IObjectMappingData<TSource, TTarget> CreateRootMappingData<TTarget>(
            TTarget target,
            bool createMapper)
        {
            return ObjectMappingDataFactory.ForRootFixedTypes(_source, target, this, createMapper);
        }
    }
}