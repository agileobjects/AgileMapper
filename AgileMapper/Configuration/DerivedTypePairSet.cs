namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Extensions;
    using Members;
    using ReadableExpressions.Extensions;

    internal class DerivedTypePairSet
    {
        private readonly Dictionary<Type, List<DerivedTypePair>> _typePairsByTargetType;

        public DerivedTypePairSet()
        {
            _typePairsByTargetType = new Dictionary<Type, List<DerivedTypePair>>();
        }

        internal bool Configuring { get; set; }

        public void Add(DerivedTypePair typePair)
        {
            var parentType = typePair.DerivedTargetType.GetBaseType();

            while (parentType != typeof(object))
            {
                List<DerivedTypePair> typePairs;

                // ReSharper disable once AssignNullToNotNullAttribute
                if (_typePairsByTargetType.TryGetValue(parentType, out typePairs))
                {
                    typePairs.Add(typePair);
                    typePairs.Sort(DerivedTypePairComparer.Instance);
                }
                else
                {
                    _typePairsByTargetType[parentType] = new List<DerivedTypePair> { typePair };
                }

                parentType = parentType.GetBaseType();
            }
        }

        public Type GetDerivedTypeOrNull<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            Type runtimeSourceType,
            IMappingContext mappingContext,
            IBasicMappingData parent)
        {
            if (Configuring || _typePairsByTargetType.None())
            {
                return null;
            }

            List<DerivedTypePair> typePairs;

            if (_typePairsByTargetType.TryGetValue(typeof(TTarget), out typePairs))
            {
                var mappingData = new BasicMappingData<TSource, TTarget>(
                    source,
                    target,
                    enumerableIndex,
                    runtimeSourceType,
                    typeof(TTarget),
                    mappingContext.RuleSet,
                    parent);

                return typePairs.FirstOrDefault(tp => tp.AppliesTo(mappingData))?.DerivedTargetType;
            }

            return null;
        }

        public void Reset()
        {
            _typePairsByTargetType.Clear();
        }

        #region Helper Class

        private class DerivedTypePairComparer : IComparer<DerivedTypePair>
        {
            public static readonly IComparer<DerivedTypePair> Instance = new DerivedTypePairComparer();

            public int Compare(DerivedTypePair x, DerivedTypePair y)
            {
                var targetTypeX = x.DerivedTargetType;
                var targetTypeY = y.DerivedTargetType;

                if (targetTypeX == targetTypeY)
                {
                    return 0;
                }

                if (targetTypeX.IsAssignableFrom(targetTypeY))
                {
                    return 1;
                }

                return -1;
            }
        }

        #endregion
    }
}