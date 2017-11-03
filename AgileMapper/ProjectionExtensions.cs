namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using Queryables;

    /// <summary>
    /// Provides extension methods to support projecting an IQueryable to an IQueryable of a different type.
    /// </summary>
    public static class ProjectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceQueryable"></param>
        /// <typeparam name="TResultElement"></typeparam>
        /// <returns></returns>
        public static IQueryable<TResultElement> ProjectTo<TResultElement>(this IQueryable sourceQueryable)
            where TResultElement : class
        {
            var projectCaller = GlobalContext.Instance.Cache.GetOrAdd(
                new SourceAndTargetTypesKey(sourceQueryable.ElementType, typeof(TResultElement)),
                key =>
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var projectQueryMethod = typeof(ProjectionExtensions)
                        .GetNonPublicStaticMethod("ProjectQuery")
                        .MakeGenericMethod(key.SourceType, key.TargetType);

                    var projectQueryCall = Expression.Call(
                        projectQueryMethod,
                        Parameters.Queryable,
                        Parameters.MapperInternal);

                    var projectQueryLambda = Expression.Lambda<Func<IQueryable, IMapperInternal, IQueryable<TResultElement>>>(
                        projectQueryCall,
                        Parameters.Queryable,
                        Parameters.MapperInternal);

                    return projectQueryLambda.Compile();
                });

            return projectCaller.Invoke(sourceQueryable, Mapper.Default);
        }

        internal static IQueryable<TResultElement> ProjectQuery<TSourceElement, TResultElement>(
            IQueryable sourceQueryable,
            IMapperInternal mapper)
        {
            var projectorKey = new QueryProjectorKey(
                MappingTypes<TSourceElement, TResultElement>.Fixed,
                sourceQueryable,
                mapper.Context);

            var rootMappingData = ObjectMappingDataFactory
                .ForProjection<TSourceElement, TResultElement>(projectorKey, mapper);

            //mapper.Context.ObjectMapperFactory.GetOrCreateRoot()

            //items.

            return null;
        }
    }
}
