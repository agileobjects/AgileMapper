namespace AgileObjects.AgileMapper
{
    using System.Linq;

    /// <summary>
    /// Provides extension methods to support projecting an IQueryable to an IQueryable of a different type.
    /// </summary>
    public static class ProjectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static IQueryable<TResult> ProjectTo<TResult>(this IQueryable items)
            where TResult : class
        {
            //items.

            return null;
        }
    }
}
