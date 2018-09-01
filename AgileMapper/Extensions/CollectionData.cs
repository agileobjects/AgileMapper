namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using NetStandardPolyfills;

    /// <summary>
    /// Untyped factory class for creating <see cref="CollectionData{T, T}"/> instances.
    /// </summary>
    public static class CollectionData
    {
        private static readonly MethodInfo[] _createMethods = typeof(CollectionData)
            .GetPublicStaticMethods("Create")
            .ToArray();

        internal static readonly MethodInfo IdSameTypesCreateMethod = _createMethods[0];
        internal static readonly MethodInfo IdDifferentTypesCreateMethod = _createMethods[1];

        /// <summary>
        /// Creates a new <see cref="CollectionData{T, T}"/> instance using the given items.
        /// </summary>
        /// <typeparam name="T">The type of object stored in the source and target collections.</typeparam>
        /// <typeparam name="TId">The type of the stored object's identifiers.</typeparam>
        /// <param name="sourceItems">The collection of source items.</param>
        /// <param name="targetItems">The collection of target items.</param>
        /// <param name="idFactory">
        /// A Func with which to retrieve the unique identifier of an object in the source or target collections.
        /// </param>
        /// <returns>A new <see cref="CollectionData{T, T}"/> instance.</returns>
        public static CollectionData<T, T> Create<T, TId>(
            IEnumerable<T> sourceItems,
            IEnumerable<T> targetItems,
            Func<T, TId> idFactory)
            => Create(sourceItems, targetItems, idFactory, idFactory);

        /// <summary>
        /// Creates a new <see cref="CollectionData{TSource, TTarget}"/> instance using the given items.
        /// </summary>
        /// <typeparam name="TSource">The type of object stored in the source collection.</typeparam>
        /// <typeparam name="TTarget">The type of object stored in the target collection.</typeparam>
        /// <typeparam name="TId">The type of the stored object's identifiers.</typeparam>
        /// <param name="sourceItems">The collection of source items.</param>
        /// <param name="targetItems">The collection of target items.</param>
        /// <param name="sourceIdFactory">
        /// A Func with which to retrieve the unique identifier of an object in the source collection.
        /// </param>
        /// <param name="targetIdFactory">
        /// A Func with which to retrieve the unique identifier of an object in the target collection.
        /// </param>
        /// <returns>A new <see cref="CollectionData{TSource, TTarget}"/> instance.</returns>
        public static CollectionData<TSource, TTarget> Create<TSource, TTarget, TId>(
            IEnumerable<TSource> sourceItems,
            IEnumerable<TTarget> targetItems,
            Func<TSource, TId> sourceIdFactory,
            Func<TTarget, TId> targetIdFactory)
        {
            if (targetItems == null)
            {
                return new CollectionData<TSource, TTarget>(
                    Enumerable<TTarget>.Empty,
                    Enumerable<Tuple<TSource, TTarget>>.Empty,
                    sourceItems);
            }

            var targetsById = GetItemsById(targetItems, targetIdFactory);
            var absentTargetItems = new List<TTarget>(targetItems);

            var intersection = new List<Tuple<TSource, TTarget>>(targetsById.Count);
            var newSourceItems = new List<TSource>();

            foreach (var sourceItem in sourceItems)
            {
                if (sourceItem == null)
                {
                    newSourceItems.Add(default(TSource));
                    continue;
                }

                var sourceItemId = sourceIdFactory.Invoke(sourceItem);

                if (sourceItemId == null)
                {
                    newSourceItems.Add(sourceItem);
                    continue;
                }

                if (targetsById.TryGetValue(sourceItemId, out var targetsWithId))
                {
                    if (EqualityComparer<TId>.Default.Equals(sourceItemId, default(TId)))
                    {
                        newSourceItems.Add(sourceItem);
                        continue;
                    }

                    var targetItem = targetsWithId[0];

                    absentTargetItems.Remove(targetItem);
                    intersection.Add(Tuple.Create(sourceItem, targetItem));
                    targetsWithId.Remove(targetItem);

                    if (targetsWithId.Count == 0)
                    {
                        targetsById.Remove(sourceItemId);
                    }
                }
                else
                {
                    newSourceItems.Add(sourceItem);
                }
            }

            return new CollectionData<TSource, TTarget>(absentTargetItems, intersection, newSourceItems);
        }

        private static Dictionary<TId, List<TItem>> GetItemsById<TItem, TId>(IEnumerable<TItem> items, Func<TItem, TId> idFactory)
        {
            return items
                .WhereNotNull()
                .Project(item => new
                {
                    Id = idFactory.Invoke(item),
                    Item = item
                })
                .Filter(d => d.Id != null)
                .GroupBy(d => d.Id)
                .ToDictionary(grp => grp.Key, grp => grp.Project(d => d.Item).ToList());
        }
    }

    /// <summary>
    /// Helper class for merging or updating collections.
    /// </summary>
    /// <typeparam name="TSource">The type of object stored in the source collection.</typeparam>
    /// <typeparam name="TTarget">The type of object stored in the target collection.</typeparam>
    public class CollectionData<TSource, TTarget>
    {
        internal CollectionData(
            IEnumerable<TTarget> absentTargetItems,
            IEnumerable<Tuple<TSource, TTarget>> intersection,
            IEnumerable<TSource> newSourceItems)
        {
            AbsentTargetItems = absentTargetItems;
            Intersection = intersection;
            NewSourceItems = newSourceItems;
        }

        /// <summary>
        /// Gets the items which exist in the target collection but not the source collection.
        /// </summary>
        public IEnumerable<TTarget> AbsentTargetItems { get; }

        /// <summary>
        /// Gets the items which exist in both the source and target collections.
        /// </summary>
        public IEnumerable<Tuple<TSource, TTarget>> Intersection { get; }

        /// <summary>
        /// Gets the items which exist in the source collection but not the target collection.
        /// </summary>
        public IEnumerable<TSource> NewSourceItems { get; }
    }
}