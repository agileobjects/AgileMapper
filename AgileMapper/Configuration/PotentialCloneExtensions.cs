namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;

    internal static class PotentialCloneExtensions
    {
        public static IEnumerable<T> SelectClones<T>(this IEnumerable<T> cloneableItems)
            where T : IPotentialClone
        {
            return cloneableItems.Select(item => item.Clone()).Cast<T>();
        }

        public static void AddSortFilter<T>(this List<T> cloneableItems, T newItem)
            where T : IPotentialClone
        {
            if (cloneableItems.None())
            {
                cloneableItems.Add(newItem);
                return;
            }

            var replacedItem = cloneableItems
                .Where(item => item.IsClone)
                .Select((item, index) => new { Item = item, Index = index })
                .FirstOrDefault(d => newItem.IsReplacementFor(d.Item));

            if (replacedItem != null)
            {
                cloneableItems.RemoveAt(replacedItem.Index);
                cloneableItems.Insert(replacedItem.Index, newItem);
                return;
            }

            cloneableItems.Add(newItem);
            cloneableItems.Sort();
        }
    }
}