namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class DerivedTypePairSet
    {
        private readonly Dictionary<Type, List<DerivedTypePair>> _typePairsByTargetType;

        public DerivedTypePairSet()
        {
            _typePairsByTargetType = new Dictionary<Type, List<DerivedTypePair>>();
        }

        public void Add(DerivedTypePair typePair)
        {
            var parentType = typePair.DerivedTargetType.BaseType;

            while (parentType != typeof(object))
            {
                List<DerivedTypePair> typePairs;

                // ReSharper disable once AssignNullToNotNullAttribute
                if (_typePairsByTargetType.TryGetValue(parentType, out typePairs))
                {
                    typePairs.Add(typePair);
                }
                else
                {
                    _typePairsByTargetType[parentType] = new List<DerivedTypePair> { typePair };
                }

                parentType = parentType.BaseType;
            }
        }

        public Type GetDerivedTypeOrNull(BasicMapperData data)
        {
            List<DerivedTypePair> typePairs;

            if (_typePairsByTargetType.TryGetValue(data.TargetType, out typePairs))
            {
                return typePairs.FirstOrDefault(tp => tp.AppliesTo(data))?.DerivedTargetType;
            }

            return null;
        }

        public void Reset()
        {
            _typePairsByTargetType.Clear();
        }
    }
}