namespace AgileObjects.AgileMapper
{
    using Api;
    using Api.Configuration;

    /// <summary>
    /// Provides a configurable mapping service. Create new instances with Mapper.CreateNew or use the default
    /// instance via the static Mapper access methods.
    /// </summary>
    public sealed class Mapper : IMapper
    {
        private static readonly IMapper _default = CreateNew();

        private readonly MapperContext _mapperContext;

        private Mapper(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        #region Factory Methods

        /// <summary>
        /// Creates an instance implementing IMapper with which to perform mappings.
        /// </summary>
        /// <returns>A new instance implementing IMapper.</returns>
        public static IMapper CreateNew() => new Mapper(new MapperContext());

        #endregion

        PlanTargetTypeSelector<TSource> IMapper.GetPlanFor<TSource>()
            => new PlanTargetTypeSelector<TSource>(_mapperContext);

        PreEventConfigStartingPoint IMapper.Before => new PreEventConfigStartingPoint(_mapperContext);

        PostEventConfigStartingPoint IMapper.After => new PostEventConfigStartingPoint(_mapperContext);

        #region Static Access Methods

        /// <summary>
        /// Create and compile mapping functions for a particular type of mapping of the source type
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TSource">The source type for which to create the mapping functions.</typeparam>
        /// <returns>
        /// A PlanTargetTypeSelector with which to specify the type of mapping the functions for which should 
        /// be cached.
        /// </returns>
        public static PlanTargetTypeSelector<TSource> GetPlanFor<TSource>() => _default.GetPlanFor<TSource>();

        /// <summary>
        /// Configure callbacks to be executed before a particular type of event occurs for all source
        /// and target types.
        /// </summary>
        public static PreEventConfigStartingPoint Before => _default.Before;

        /// <summary>
        /// Configure callbacks to be executed after a particular type of event occurs for all source
        /// and target types.
        /// </summary>
        public static PostEventConfigStartingPoint After => _default.After;

        /// <summary>
        /// Configure how the default mapper performs a mapping.
        /// </summary>
        public static MappingConfigStartingPoint WhenMapping => _default.WhenMapping;

        /// <summary>
        /// Performs a deep clone of the given <paramref name="source"/> object and returns the result.
        /// </summary>
        /// <typeparam name="TSource">The type of object for which to perform a deep clone.</typeparam>
        /// <param name="source">The object to deep clone.</param>
        /// <returns>A deep clone of the given <paramref name="source"/> object.</returns>
        public static TSource Clone<TSource>(TSource source) where TSource : class
            => _default.Clone(source);

        /// <summary>
        /// Flattens the given <paramref name="source"/> object so it has only value-type or string members
        /// and returns the result.
        /// </summary>
        /// <typeparam name="TSource">The type of object to flatten.</typeparam>
        /// <param name="source">The object to flatten.</param>
        /// <returns>
        /// A dynamic object containing flattened versions of the given <paramref name="source"/> object's 
        /// properties.
        /// </returns>
        public static dynamic Flatten<TSource>(TSource source) where TSource : class
            => _default.Flatten(source);

        /// <summary>
        /// Perform a mapping operation on the given <paramref name="source"/> object.
        /// </summary>
        /// <typeparam name="TSource">The type of source object on which to perform the mapping.</typeparam>
        /// <param name="source">The source object on which to perform the mapping.</param>
        /// <returns>A TargetTypeSelector with which to specify the type of mapping to perform.</returns>
        public static TargetTypeSelector<TSource> Map<TSource>(TSource source) => _default.Map(source);

        internal static void ResetDefaultInstance() => _default.Dispose();

        #endregion

        MappingConfigStartingPoint IMapper.WhenMapping => new MappingConfigStartingPoint(_mapperContext);

        TSource IMapper.Clone<TSource>(TSource source) => ((IMapper)this).Map(source).ToANew<TSource>();

        dynamic IMapper.Flatten<TSource>(TSource source) => _mapperContext.ObjectFlattener.Flatten(source);

        TargetTypeSelector<TSource> IMapper.Map<TSource>(TSource source)
        {
            return new TargetTypeSelector<TSource>(source, _mapperContext);
        }

        #region IDisposable Members

        /// <summary>
        /// Removes the mapper's cached data.
        /// </summary>
        public void Dispose() => _mapperContext.Reset();

        #endregion
    }
}
