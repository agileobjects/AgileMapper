namespace AgileObjects.AgileMapper
{
    using ObjectPopulation;
    using Plans;

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
        /// class with the given <paramref name="source"/> object.
        /// </summary>
        /// <param name="source">The source object from which the mapping is to be performed.</param>
        protected MappingExecutionContextBase(TSource source)
        {
            _mapperContext = Mapper.Default.Context.ThrowIfDisposed();
            _source = source;
        }

        MapperContext IMapperContextOwner.MapperContext => _mapperContext;

        MappingRuleSet IRuleSetOwner.RuleSet => null;

        MappingPlanSettings IMappingContext.PlanSettings => null;

        /// <summary>
        /// Creates a root <see cref="IObjectMappingData{TSource, TTarget}"/> object for this
        /// <see cref="MappingExecutionContextBase{TSource}"/>'s source object and the given
        /// <paramref name="target"/> object.
        /// </summary>
        /// <typeparam name="TTarget">The type of target object to which the mapping is being performed.</typeparam>
        /// <param name="target">The target object to which the mapping is being performed.</param>
        /// <returns>
        /// A root <see cref="IObjectMappingData{TSource, TTarget}"/> object for this
        /// <see cref="MappingExecutionContextBase{TSource}"/>'s source object and the given
        /// <paramref name="target"/> object.
        /// </returns>
        protected IObjectMappingData<TSource, TTarget> CreateRootMappingData<TTarget>(TTarget target)
            => ObjectMappingDataFactory.ForRootFixedTypes(_source, target, this, createMapper: false);

        /// <summary>
        /// Creates an <see cref="IObjectMappingData{TSource, TTarget}"/> object for the given
        /// <paramref name="source"/> and <paramref name="target"/> child objects.
        /// </summary>
        /// <typeparam name="TChildSource">The type of source object from which the child mapping is being performed.</typeparam>
        /// <typeparam name="TChildTarget">The type of target object to which the child mapping is being performed.</typeparam>
        /// <param name="source">The child source object from which the mapping is being performed.</param>
        /// <param name="target">The child target object to which the mapping is being performed.</param>
        /// <param name="parent">The mapping data parent object of the child object to create.</param>
        /// <returns>
        /// A child <see cref="IObjectMappingData{TSource, TTarget}"/> object for the given
        /// <paramref name="source"/> and <paramref name="target"/> objects.
        /// </returns>
        protected static IObjectMappingData<TChildSource, TChildTarget> CreateChildMappingData<TChildSource, TChildTarget>(
            TChildSource source,
            TChildTarget target,
            IObjectMappingDataUntyped parent)
        {
            var parentMappingData = (IObjectMappingData)parent;

            var childMappingData = ObjectMappingDataFactory.ForChild(
                source,
                target,
                parentMappingData.GetElementIndex(),
                parentMappingData.GetElementKey(),
                targetMemberRegistrationName: string.Empty,
                dataSourceIndex: 0,
                parentMappingData);

            return (IObjectMappingData<TChildSource, TChildTarget>)childMappingData;
        }
    }
}