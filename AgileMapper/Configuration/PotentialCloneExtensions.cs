namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class PotentialCloneExtensions
    {
        public static IEnumerable<T> SelectClones<T>(this IEnumerable<T> cloneableItems)
            where T : IPotentialClone
        {
            return cloneableItems.Select(item => item.Clone()).Cast<T>();
        }
    }
}