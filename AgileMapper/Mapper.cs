namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using Api;
    using Api.Configuration;
    using Plans;
    using Queryables.Api;
    using Validation;
#if NET35
    using Expr = Microsoft.Scripting.Ast.Expression;
#else
    using Expr = System.Linq.Expressions.Expression;
#endif

    /// <summary>
    /// Provides a configurable mapping service. Create new instances with Mapper.CreateNew or use the default
    /// instance via the static Mapper access methods.
    /// </summary>
    public sealed class Mapper : IMapperInternal
    {
        internal static readonly IMapperInternal Default = CreateNewInternal();

        private Mapper(MapperContext context)
        {
            Context = context;
            Context.Mapper = this;
        }

        #region Factory Methods

        /// <summary>
        /// Creates an instance implementing IMapper with which to perform mappings.
        /// </summary>
        /// <returns>A new instance implementing IMapper.</returns>
        public static IMapper CreateNew() => CreateNewInternal();

        private static IMapperInternal CreateNewInternal() => new Mapper(new MapperContext());

        #endregion

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
        /// An IPlanTargetAndRuleSetSelector with which to specify the type of mapping the functions for which 
        /// should be cached.
        /// </returns>
        public static IPlanTargetAndRuleSetSelector<TSource> GetPlanFor<TSource>(TSource exampleInstance) => GetPlanFor<TSource>();

        /// <summary>
        /// Create and compile mapping functions for a particular type of mapping of the source type
        /// specified by the type argument.
        /// </summary>
        /// <typeparam name="TSource">The source type for which to create the mapping functions.</typeparam>
        /// <returns>
        /// An IPlanTargetAndRuleSetSelector with which to specify the type of mapping the functions for which 
        /// should be cached.
        /// </returns>
        public static IPlanTargetAndRuleSetSelector<TSource> GetPlanFor<TSource>() => Default.GetPlanFor<TSource>();

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
        public static IPlanTargetSelector<TSource> GetPlansFor<TSource>(TSource exampleInstance) => GetPlansFor<TSource>();

        /// <summary>
        /// Create and compile mapping functions for the source type specified by the type argument, for all
        /// mapping types (create new, merge, overwrite).
        /// </summary>
        /// <typeparam name="TSource">The source type for which to create the mapping functions.</typeparam>
        /// <returns>
        /// An IPlanTargetSelector with which to specify the target type the mapping functions for which 
        /// should be cached.
        /// </returns>
        public static IPlanTargetSelector<TSource> GetPlansFor<TSource>() => Default.GetPlansFor<TSource>();

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
        public static IProjectionPlanTargetSelector<TSourceElement> GetPlanForProjecting<TSourceElement>(
            IQueryable<TSourceElement> exampleQueryable)
        {
            return Default.GetPlanForProjecting(exampleQueryable);
        }

        /// <summary>
        /// Returns mapping plans for all mapping functions currently cached by the default <see cref="IMapper"/>.
        /// </summary>
        /// <returns>A string containing the currently-cached functions to be executed during mappings.</returns>
        public static string GetPlansInCache() => Default.GetPlansInCache();

        /// <summary>
        /// Returns mapping plan Expressions for all mapping functions currently cached by the default <see cref="IMapper"/>.
        /// </summary>
        /// <returns>An Expression containing the currently-cached functions to be executed during mappings.</returns>
        public static ReadOnlyCollection<Expr> GetPlanExpressionsInCache() => Default.GetPlanExpressionsInCache();

        /// <summary>
        /// Configure callbacks to be executed before a particular type of event occurs for all source
        /// and target types.
        /// </summary>
        public static PreEventConfigStartingPoint Before => Default.Before;

        /// <summary>
        /// Configure callbacks to be executed after a particular type of event occurs for all source
        /// and target types.
        /// </summary>
        public static PostEventConfigStartingPoint After => Default.After;

        /// <summary>
        /// Configure how the default mapper performs a mapping.
        /// </summary>
        public static MappingConfigStartingPoint WhenMapping => Default.WhenMapping;

        /// <summary>
        /// Throw an exception upon execution of this statement if any cached mappings have any target 
        /// members which will not be mapped. Use calls to this method to validate a mapping plan, remove 
        /// them in production code.
        /// </summary>
        public static void ThrowNowIfAnyMappingIsIncomplete() => Default.ThrowNowIfAnyMappingPlanIsIncomplete();

        /// <summary>
        /// Performs a deep clone of the given <paramref name="source"/> object and returns the result.
        /// </summary>
        /// <typeparam name="TSource">The type of object for which to perform a deep clone.</typeparam>
        /// <param name="source">The object to deep clone.</param>
        /// <returns>A deep clone of the given <paramref name="source"/> object.</returns>
        public static TSource DeepClone<TSource>(TSource source) => Default.DeepClone(source);

        /// <summary>
        /// Performs a deep clone of the given <paramref name="source"/> object and returns the result.
        /// </summary>
        /// <typeparam name="TSource">The type of object for which to perform a deep clone.</typeparam>
        /// <param name="configurations">
        /// One or more mapping configurations. The mapping will be configured by combining these inline 
        /// <paramref name="configurations"/> with any configuration already set up via the Mapper.WhenMapping API.
        /// </param>
        /// <param name="source">The object to deep clone.</param>
        /// <returns>A deep clone of the given <paramref name="source"/> object.</returns>
        public static TSource DeepClone<TSource>(
            TSource source,
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TSource>>>[] configurations)
        {
            return Default.DeepClone(source, configurations);
        }

        /// <summary>
        /// Flatten the given <paramref name="source"/> object so it has only value-type or string members.
        /// </summary>
        /// <typeparam name="TSource">The type of object to flatten.</typeparam>
        /// <param name="source">The object to flatten.</param>
        /// <returns>An IFlatteningSelector with which to select the type of flattening to perform.</returns>
        public static IFlatteningSelector<TSource> Flatten<TSource>(TSource source) where TSource : class
            => Default.Flatten(source);

        /// <summary>
        /// Unflatten the given string-keyed <paramref name="source"/> Dictionary to a specified result
        /// Type, using the default mapper.
        /// </summary>
        /// <typeparam name="TValue">The Type of values the source Dictionary contains.</typeparam>
        /// <returns>
        /// An IUnflatteningSelector with which to specify the target Type to which unflattening should
        /// be performed.
        /// </returns>
        public static IUnflatteningSelector<IDictionary<string, TValue>> Unflatten<TValue>(
            IDictionary<string, TValue> source)
        {
            return Default.Unflatten(source);
        }

        /// <summary>
        /// Unflatten the given <paramref name="queryString"/> to a specified result Type, using the
        /// default mapper. Strings can be cast to a <see cref="QueryString"/> instance explicitly,
        /// or by using the string.ToQueryString() extension method.
        /// </summary>
        /// <param name="queryString">The <see cref="QueryString"/> from which to unflatten.</param>
        /// <returns>
        /// An IUnflatteningSelector with which to specify the target Type to which unflattening should
        /// be performed.
        /// </returns>
        public static IUnflatteningSelector<QueryString> Unflatten(QueryString queryString)
            => Default.Unflatten(queryString);

        /// <summary>
        /// Perform a mapping operation on the given <paramref name="source"/> object.
        /// </summary>
        /// <typeparam name="TSource">The type of source object on which to perform the mapping.</typeparam>
        /// <param name="source">The source object on which to perform the mapping.</param>
        /// <returns>A TargetSelector with which to specify the type of mapping to perform.</returns>
        public static ITargetSelector<TSource> Map<TSource>(TSource source) => Default.Map(source);

        /// <summary>
        /// Removes the default Mapper's cached data. Can be useful when testing code which uses
        /// the static Mapper API.
        /// </summary>
        public static void ResetDefaultInstance() => Default.Dispose();

        #endregion

        MapperContext IMapperInternal.Context => Context;

        internal MapperContext Context { get; }

        IPlanTargetAndRuleSetSelector<TSource> IMapper.GetPlanFor<TSource>(TSource exampleInstance) => GetPlan<TSource>();

        IPlanTargetAndRuleSetSelector<TSource> IMapper.GetPlanFor<TSource>() => GetPlan<TSource>();

        IProjectionPlanTargetSelector<TSourceElement> IMapper.GetPlanForProjecting<TSourceElement>(
            IQueryable<TSourceElement> exampleQueryable)
        {
            return new PlanTargetSelector<TSourceElement>(Context, exampleQueryable);
        }

        IPlanTargetSelector<TSource> IMapper.GetPlansFor<TSource>(TSource exampleInstance) => GetPlan<TSource>();

        IPlanTargetSelector<TSource> IMapper.GetPlansFor<TSource>() => GetPlan<TSource>();

        string IMapper.GetPlansInCache() => MappingPlanSet.For(Context);
        
        ReadOnlyCollection<Expr> IMapper.GetPlanExpressionsInCache() => MappingPlanSet.For(Context);

        private PlanTargetSelector<TSource> GetPlan<TSource>()
            => new PlanTargetSelector<TSource>(Context);

        PreEventConfigStartingPoint IMapper.Before => new PreEventConfigStartingPoint(Context);

        PostEventConfigStartingPoint IMapper.After => new PostEventConfigStartingPoint(Context);

        MappingConfigStartingPoint IMapper.WhenMapping => new MappingConfigStartingPoint(Context);

        void IMapper.ThrowNowIfAnyMappingPlanIsIncomplete() => MappingValidator.Validate(this);

        IMapper IMapper.CloneSelf() => new Mapper(Context.Clone());

        TSource IMapper.DeepClone<TSource>(TSource source) => ((IMapper)this).Map(source).ToANew<TSource>();

        TSource IMapper.DeepClone<TSource>(
            TSource source,
            params Expression<Action<IFullMappingInlineConfigurator<TSource, TSource>>>[] configurations)
        {
            return ((IMapper)this).Map(source).ToANew(configurations);
        }

        IFlatteningSelector<TSource> IMapper.Flatten<TSource>(TSource source)
            => new MappingExecutor<TSource>(source, Context);

        IUnflatteningSelector<IDictionary<string, TValue>> IMapper.Unflatten<TValue>(IDictionary<string, TValue> source)
            => new MappingExecutor<IDictionary<string, TValue>>(source, Context);

        IUnflatteningSelector<QueryString> IMapper.Unflatten(QueryString queryString)
            => new MappingExecutor<QueryString>(queryString, Context);

        ITargetSelector<TSource> IMapper.Map<TSource>(TSource source)
            => new MappingExecutor<TSource>(source, Context);

        #region IDisposable Members

        /// <summary>
        /// Removes the mapper's cached data.
        /// </summary>
        public void Dispose() => Context.Reset();

        #endregion
    }
}
