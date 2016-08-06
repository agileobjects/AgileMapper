namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Members;

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
                    // TODO: Derived grandchild pair tests
                    typePairs.Add(typePair);
                }
                else
                {
                    _typePairsByTargetType[typePair.ParentTargetType] = new List<DerivedTypePair> { typePair };
                }

                // ReSharper disable once PossibleNullReferenceException
                parentType = parentType.BaseType;
            }
        }

        public Type GetDerivedTypeOrNull(IMappingData data)
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


        }
    }
}