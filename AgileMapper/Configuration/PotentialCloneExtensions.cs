namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using Extensions.Internal;

    internal static class PotentialCloneExtensions
    {
        public static IList<T> CloneItems<T>(this IList<T> cloneableItems)
            where T : IPotentialAutoCreatedItem
        {
            var clonedItems = new T[cloneableItems.Count];

            for (var i = 0; i < cloneableItems.Count; i++)
            {
                clonedItems[i] = (T)cloneableItems[i].Clone();
            }

            return clonedItems;
        }

        public static void AddThenSort<T>(this IList<T> items, T newItem)
            where T : IComparable<T>
        {
            if (items.None())
            {
                items.Add(newItem);
                return;
            }

            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].CompareTo(newItem) == 1)
                {
                    items.Insert(i, newItem);
                    return;
                }
            }

            items.Add(newItem);
        }

        public static void AddOrReplaceThenSort<T>(this List<T> cloneableItems, T newItem)
            where T : IPotentialAutoCreatedItem, IComparable<T>
        {
            if (cloneableItems.None())
            {
                cloneableItems.Add(newItem);
                return;
            }

            var replacedItem = cloneableItems
                .Project((item, index) => new { Item = item, Index = index, IsClone = item.WasAutoCreated })
                .Filter(d => d.IsClone)
                .FirstOrDefault(d => newItem.IsReplacementFor(d.Item));

            if (replacedItem != null)
            {
                cloneableItems.RemoveAt(replacedItem.Index);
                cloneableItems.Insert(replacedItem.Index, newItem);
                return;
            }

            cloneableItems.AddThenSort(newItem);
        }
    }
}