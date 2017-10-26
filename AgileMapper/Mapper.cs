namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq.Expressions;
    using Api;
    using Api.Configuration;
    using Plans;

    /// <summary>
    /// Provides a configurable mapping service. Create new instances with Mapper.CreateNew or use the default
    /// instance via the static Mapper access methods.
    /// </summary>
    public sealed class Mapper : IMapper
    {
        private static readonly IMapper _default = CreateNew();

        private Mapper(MapperContext context)
        {
            Context = context;
        }

        #region Factory Methods

        /// <summary>
        /// Creates an instance implementing IMapper with which to perform mappings.
        /// </summary>
        /// <returns>A new instance implementing IMapper.</returns>
        public static IMapper CreateNew()
        {
            var mapper = new Mapper(new MapperContext());

            MapperCache.Add(mapper);

            return mapper;
        }

        #endregion

        internal MapperContext Context { get; }

        IPlanTargetTypeAndRuleSetSelector<TSource> IMapper.GetPlanFor<TSource>(TSource exampleInstance) => GetPlan<TSource>();

        IPlanTargetTypeAndRuleSetSelector<TSource> IMapper.GetPlanFor<TSource>() => GetPlan<TSource>();

        IPlanTargetTypeSelector IMapper.GetPlansFor<TSource>(TSource exampleInstance) => GetPlan<TSource>();

        IPlanTargetTypeSelector IMapper.GetPlansFor<TSource>() => GetPlan<TSource>();

        string IMapper.GetPlansInCache() => MappingPlanSet.For(Context);

        private PlanTargetTypeSelector<TSource> GetPlan<TSource>()
            => new PlanTargetTypeSelector<TSource>(Context);

        PreEventConfigStartingPoint IMapper.Before => new PreEventConfigStartingPoint(Context);

        PostEventConfigStartingPoint IMapper.After => new PostEventConfigStartingPoint(Context);

        #region Static Access Methods

        /// <summary>
        /// Create and compile mapping functions for a particular type of mapping of the source type specified by 
        /// the given <paramref name="exampleInstance"/>. Use this overload for anonymous types.
        /// </summary>
        /// <typeparam name="TSource">The type of the given <paramref name="exampleInstance"/>.</typeparam>
        /// <param name="exampleInstance">
        /// An instance specifying the source type for which a mapping plan should be created.
        /// </param>
        /// <returns>
        /// An IPlanTargetTypeAndRuleSetSelector with which to specify the type of mapping the functions for which 
        /// should be cached.
        /// </returns>
        public static IPlanTargetTypeAndRuleSetSelector<TSource> GetPlanFor<TSource>(TSource exampleInstance) => GetPlanFor<TSource>();

        /// <summary>
        /// Create and compile mapping functions for a particular type of mapping of the source type
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TSource">The source type for which to create the mapping functions.</typeparam>
        /// <returns>
        /// An IPlanTargetTypeAndRuleSetSelector with which to specify the type of mapping the functions for which 
        /// should be cached.
        /// </returns>
        public static IPlanTargetTypeAndRuleSetSelector<TSource> GetPlanFor<TSource>() => _default.GetPlanFor<TSource>();

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
        /// An IPlanTargetTypeSelector with which to specify the target type the mapping functions for which 
        /// should be cached.
        /// </returns>
        public static IPlanTargetTypeSelector GetPlansFor<TSource>(TSource exampleInstance) => GetPlansFor<TSource>();

        /// <summary>
        /// Create and compile mapping functions for the source type specified by the type argument, for all
        /// mapping types (create new, merge, overwrite).
        /// </summary>
        /// <typeparam name="TSource">The source type for which to create the mapping functions.</typeparam>
        /// <returns>
        /// An IPlanTargetTypeSelector with which to specify the target type the mapping functions for which 
        /// should be cached.
        /// </returns>
        public static IPlanTargetTypeSelector GetPlansFor<TSource>() => _default.GetPlansFor<TSource>();

        /// <summary>
        /// Returns mapping plans for all mapping functions currently cached by the default <see cref="IMapper"/>.
        /// </summary>
        /// <returns>A string containing the currently-cached functions to be executed during mappings.</returns>
        public static string GetPlansInCache() => _default.GetPlansInCache();

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
        public static TSource Clone<TSource>(TSource source) => _default.Clone(source);

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
        public static ITargetTypeSelector<TSource> Map<TSource>(TSource source) => _default.Map(source);

        internal static void ResetDefaultInstance() => _default.Dispose();

        #endregion

        MappingConfigStartingPoint IMapper.WhenMapping => new MappingConfigStartingPoint(Context);

        IMapper IMapper.CloneSelf() => new Mapper(Context.Clone());

        TSource IMapper.Clone<TSource>(TSource source) => ((IMapper)this).Map(source).ToANew<TSource>();

        TSource IMapper.Clone<TSource>(
            TSource source,
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TSource>>>[] configurations)
        {
            return ((IMapper)this).Map(source).ToANew(configurations);
        }

        dynamic IMapper.Flatten<TSource>(TSource source) => Context.ObjectFlattener.Flatten(source);

        ITargetTypeSelector<TSource> IMapper.Map<TSource>(TSource source)
            => new MappingExecutor<TSource>(source, Context);

        #region IDisposable Members

        /// <summary>
        /// Removes the mapper's cached data.
        /// </summary>
        public void Dispose() => Context.Reset();

        #endregion
    }

    internal static class MapperCache
    {
        public static void Add(IMapper mapper)
        {

        }
    }
}
