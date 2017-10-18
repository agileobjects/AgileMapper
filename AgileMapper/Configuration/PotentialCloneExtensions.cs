namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;

    internal static class PotentialCloneExtensions
    {
        public static IList<T> CloneItems<T>(this IList<T> cloneableItems)
            where T : IPotentialClone
        {
            var clonedItems = new List<T>(cloneableItems.Count);

            clonedItems.AddRange(cloneableItems
                .Where(item => !item.IsInlineConfiguration)
                .Select(t => (T)t.Clone()));

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
                var insertIndex = (replacedItem.Index == 0) ? 0 : replacedItem.Index - 1;
                cloneableItems.Insert(insertIndex, newItem);
                return;
            }

            cloneableItems.Add(newItem);
            cloneableItems.Sort();
        }

        public static bool ConflictWith<TItem>(this IEnumerable<TItem> items, IEnumerable<TItem> otherItems)
            where TItem : UserConfiguredItemBase
        {
            return (items != null) &&
                   (otherItems != null) &&
                    otherItems.Any(otherItem => items.GetConflictingItemOrNull(otherItem) != null);
        }

        public static TItem GetConflictingItemOrNull<TItem>(
            this IEnumerable<TItem> items,
            UserConfiguredItemBase item)
            where TItem : UserConfiguredItemBase
        {
            return items?.FirstOrDefault(ci => ci.ConflictsWith(item));
        }
    }
}