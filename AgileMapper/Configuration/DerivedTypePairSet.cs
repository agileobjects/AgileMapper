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
        private static readonly DerivedTypePair[] _noPairs = { };
        private readonly Dictionary<Type, List<DerivedTypePair>> _typePairsByTargetType;

        public DerivedTypePairSet()
        {
            _typePairsByTargetType = new Dictionary<Type, List<DerivedTypePair>>();
        }

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

        public ICollection<DerivedTypePair> GetDerivedTypePairsFor(IBasicMapperData mapperData)
        {
            if (_typePairsByTargetType.None())
            {
                return _noPairs;
            }

            List<DerivedTypePair> typePairs;

            if (_typePairsByTargetType.TryGetValue(mapperData.TargetType, out typePairs))
            {
                return typePairs.Where(tp => tp.AppliesTo(mapperData)).ToArray();
            }

            return _noPairs;
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