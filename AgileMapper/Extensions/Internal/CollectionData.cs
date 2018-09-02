namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using NetStandardPolyfills;

    internal static class CollectionData
    {
        private static readonly MethodInfo[] _createMethods = typeof(CollectionData)
            .GetPublicStaticMethods("Create")
            .ToArray();

        public static readonly MethodInfo IdSameTypesCreateMethod = _createMethods[0];
        public static readonly MethodInfo IdDifferentTypesCreateMethod = _createMethods[1];

        public static CollectionData<T, T> Create<T, TId>(
            IEnumerable<T> sourceItems,
            IEnumerable<T> targetItems,
            Func<T, TId> idFactory)
            => Create(sourceItems, targetItems, idFactory, idFactory);

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

    internal class CollectionData<TSource, TTarget>
    {
        public CollectionData(
            IEnumerable<TTarget> absentTargetItems,
            IEnumerable<Tuple<TSource, TTarget>> intersection,
            IEnumerable<TSource> newSourceItems)
        {
            AbsentTargetItems = absentTargetItems;
            Intersection = intersection;
            NewSourceItems = newSourceItems;
        }

        public IEnumerable<TTarget> AbsentTargetItems { get; }

        public IEnumerable<Tuple<TSource, TTarget>> Intersection { get; }

        public IEnumerable<TSource> NewSourceItems { get; }
    }
}