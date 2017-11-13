namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using Queryables;
    using Queryables.Api;

    /// <summary>
    /// Provides extension methods to support projecting an IQueryable to an IQueryable of a different type.
    /// </summary>
    public static class ProjectionExtensions
    {
        /// <summary>
        /// Project the elements of the given <paramref name="sourceQueryable"/> to instances of the given 
        /// <typeparamref name="TResultElement"/>.
        /// </summary>
        /// <param name="sourceQueryable">The source collection on which to perform the projection.</param>
        /// <typeparam name="TResultElement">
        /// The target Type to which the elements of the given <paramref name="sourceQueryable"/> should be projected.
        /// </typeparam>
        /// <returns>
        /// An IQueryable of the given <paramref name="sourceQueryable"/> to instances of the given 
        /// <typeparamref name="TResultElement"/>. The projection is not performed until the Queryable is enumerated 
        /// by a call to .ToArray() or similar.
        /// </returns>
        public static IQueryable<TResultElement> ProjectTo<TResultElement>(this IQueryable sourceQueryable)
            where TResultElement : class
        {
            return ProjectTo<TResultElement>(sourceQueryable, Mapper.Default);
        }

        /// <summary>
        /// Project the elements of the given <paramref name="sourceQueryable"/> to instances of the given 
        /// <typeparamref name="TResultElement"/>.
        /// </summary>
        /// <param name="sourceQueryable">The source collection on which to perform the projection.</param>
        /// <param name="mapperSelector">A func providing the mapper with which the projection should be performed.</param>
        /// <typeparam name="TResultElement">
        /// The target Type to which the elements of the given <paramref name="sourceQueryable"/> should be projected.
        /// </typeparam>
        /// <returns>
        /// An IQueryable of the given <paramref name="sourceQueryable"/> to instances of the given 
        /// <typeparamref name="TResultElement"/>. The projection is not performed until the Queryable is enumerated 
        /// by a call to .ToArray() or similar.
        /// </returns>
        public static IQueryable<TResultElement> ProjectTo<TResultElement>(
            this IQueryable sourceQueryable,
            Func<ProjectionMapperSelector, IMapper> mapperSelector)
            where TResultElement : class
        {
            var mapper = mapperSelector.Invoke(ProjectionMapperSelector.Instance);

            return ProjectTo<TResultElement>(sourceQueryable, mapper);
        }

        private static IQueryable<TResultElement> ProjectTo<TResultElement>(
            IQueryable sourceQueryable,
            IMapper mapper)
            where TResultElement : class
        {
            var projectCaller = GlobalContext.Instance.Cache.GetOrAdd(
                new SourceAndTargetTypesKey(sourceQueryable.ElementType, typeof(TResultElement)),
                key =>
                {
                    var projectQueryMethod = typeof(ProjectionExtensions)
                        .GetNonPublicStaticMethod("ProjectQuery")
                        .MakeGenericMethod(key.SourceType, key.TargetType);

                    var typedSourceQueryable = typeof(IQueryable<>).MakeGenericType(key.SourceType);

                    var projectQueryCall = Expression.Call(
                        projectQueryMethod,
                        Parameters.Queryable.GetConversionTo(typedSourceQueryable),
                        Parameters.Mapper);

                    var projectQueryLambda = Expression.Lambda<Func<IQueryable, IMapper, IQueryable<TResultElement>>>(
                        projectQueryCall,
                        Parameters.Queryable,
                        Parameters.Mapper);

                    return projectQueryLambda.Compile();
                });

            return projectCaller.Invoke(sourceQueryable, mapper);
        }

        internal static IQueryable<TResultElement> ProjectQuery<TSourceElement, TResultElement>(
            IQueryable<TSourceElement> sourceQueryable,
            IMapper mapper)
        {
            var mapperContext = ((IMapperInternal)mapper).Context;

            var projectorKey = new QueryProjectorKey(
                MappingTypes<TSourceElement, TResultElement>.Fixed,
                sourceQueryable,
                mapperContext);

            var rootMappingData = ObjectMappingDataFactory
                .ForProjection<IQueryable<TSourceElement>, IQueryable<TResultElement>>(
                    projectorKey,
                    sourceQueryable,
                    mapperContext.QueryProjectionMappingContext);

            var queryProjection = rootMappingData.MapStart();

            return queryProjection;
        }
    }
}
