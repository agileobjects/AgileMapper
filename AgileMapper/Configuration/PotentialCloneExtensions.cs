namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions.Internal;

    internal static class PotentialCloneExtensions
    {
        public static IList<T> CloneItems<T>(this IList<T> cloneableItems)
            where T : IPotentialClone
        {
            var clonedItems = new T[cloneableItems.Count];

            for (var i = 0; i < cloneableItems.Count; i++)
            {
                clonedItems[i] = (T)cloneableItems[i].Clone();
            }

            return clonedItems;
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