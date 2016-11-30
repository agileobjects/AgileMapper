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
    using NetStandardPolyfills;

    internal class DerivedTypePairSet
    {
        private static readonly object _lookupSync = new object();
        private static readonly DerivedTypePair[] _noPairs = Enumerable<DerivedTypePair>.EmptyArray;
        private readonly Dictionary<Type, List<DerivedTypePair>> _typePairsByTargetType;
        private readonly List<SourceAndTargetTypesKey> _autoCheckedTypes;

        public DerivedTypePairSet()
        {
            _typePairsByTargetType = new Dictionary<Type, List<DerivedTypePair>>();
            _autoCheckedTypes = new List<SourceAndTargetTypesKey>();
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
                    RemoveConflictingPairIfAppropriate(typePair, typePairs);

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

        private static void RemoveConflictingPairIfAppropriate(
            DerivedTypePair typePair,
            ICollection<DerivedTypePair> typePairs)
        {
            if (typePair.HasConfiguredCondition)
            {
                return;
            }

            var existingTypePair = typePairs.FirstOrDefault(tp =>
                !tp.HasConfiguredCondition && tp.HasSourceType(typePair.DerivedSourceType));

            if (existingTypePair != null)
            {
                typePairs.Remove(existingTypePair);
            }
        }

        public ICollection<DerivedTypePair> GetDerivedTypePairsFor(IMemberMapperData mapperData)
            => GetDerivedTypePairsFor(mapperData, mapperData.MapperContext);

        public ICollection<DerivedTypePair> GetDerivedTypePairsFor(
            IBasicMapperData mapperData,
            MapperContext mapperContext)
        {
            LookForDerivedTypePairs(mapperData, mapperContext);

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

        #region Auto-Registration

        private void LookForDerivedTypePairs(IBasicMapperData mapperData, MapperContext mapperContext)
        {
            var rootSourceType = GetRootType(mapperData.SourceType);
            var rootTargetType = GetRootType(mapperData.TargetType);
            var typesKey = new SourceAndTargetTypesKey(rootSourceType, rootTargetType);

            lock (_lookupSync)
            {
                if (_autoCheckedTypes.Contains(typesKey))
                {
                    return;
                }

                _autoCheckedTypes.Add(typesKey);
            }

            if (mapperData.TargetMember.IsSimple)
            {
                mapperData = mapperData.Parent;
            }

            Func<Type, string> derivedTargetTypeNameFactory;

            if (SkipDerivedTypePairsLookup(
                mapperData,
                rootSourceType,
                rootTargetType,
                out derivedTargetTypeNameFactory))
            {
                return;
            }

            var derivedSourceTypes = mapperContext.DerivedTypes.GetTypesDerivedFrom(rootSourceType);

            if (derivedSourceTypes.None())
            {
                return;
            }

            var derivedTargetTypes = mapperContext.DerivedTypes.GetTypesDerivedFrom(rootTargetType);

            if (derivedTargetTypes.None())
            {
                return;
            }

            var candidatePairsData = derivedSourceTypes
                .Select(t => new
                {
                    DerivedSourceType = t,
                    DerivedTargetTypeName = derivedTargetTypeNameFactory.Invoke(t)
                })
                .ToArray();

            foreach (var candidatePairData in candidatePairsData)
            {
                var derivedTargetType = derivedTargetTypes
                    .FirstOrDefault(t => t.Name == candidatePairData.DerivedTargetTypeName);

                if (derivedTargetType == null)
                {
                    continue;
                }

                var configInfo = new MappingConfigInfo(mapperContext)
                    .ForRuleSet(mapperData.RuleSet)
                    .ForSourceType(rootSourceType)
                    .ForTargetType(rootTargetType);

                var derivedTypePair = new DerivedTypePair(
                    configInfo,
                    candidatePairData.DerivedSourceType,
                    derivedTargetType);

                Add(derivedTypePair);
            }
        }

        private static bool SkipDerivedTypePairsLookup(
            IBasicMapperData mapperData,
            Type rootSourceType,
            Type rootTargetType,
            out Func<Type, string> derivedTargetTypeNameFactory)
        {
            if (mapperData.TargetMember.IsEnumerable || (rootSourceType == rootTargetType))
            {
                derivedTargetTypeNameFactory = null;
                return true;
            }

            var sourceTypeName = rootSourceType.Name;
            var targetTypeName = rootTargetType.Name;

            if (sourceTypeName.Length == targetTypeName.Length)
            {
                derivedTargetTypeNameFactory = null;
                return true;
            }

            var sourceNameIsShorter = sourceTypeName.Length < targetTypeName.Length;

            if (sourceNameIsShorter)
            {
                if (!targetTypeName.Contains(sourceTypeName))
                {
                    derivedTargetTypeNameFactory = null;
                    return true;
                }
            }
            else
            {
                if (!sourceTypeName.Contains(targetTypeName))
                {
                    derivedTargetTypeNameFactory = null;
                    return true;
                }
            }

            if (mapperData.SourceType.IsSealed() || mapperData.TargetType.IsSealed() ||
                mapperData.SourceType.IsFromBcl() || mapperData.TargetType.IsFromBcl())
            {
                derivedTargetTypeNameFactory = null;
                return false;
            }

            if (sourceNameIsShorter)
            {
                const string TYPE_NAME_PATTERN_TOKEN = "[$]";
                var typeNamePattern = targetTypeName.Replace(sourceTypeName, TYPE_NAME_PATTERN_TOKEN);

                derivedTargetTypeNameFactory = sourceType
                    => typeNamePattern.Replace(TYPE_NAME_PATTERN_TOKEN, sourceType.Name);
            }
            else
            {
                var targetTypeNameStartIndex = sourceTypeName.IndexOf(targetTypeName, StringComparison.Ordinal);
                var targetTypeNameLength = targetTypeName.Length;
                var targetTypeNameEndIndex = sourceTypeName.Length - targetTypeNameLength;

                derivedTargetTypeNameFactory = sourceType => sourceType.Name.Substring(
                    targetTypeNameStartIndex,
                    sourceType.Name.Length - targetTypeNameEndIndex);
            }

            return false;
        }

        private static Type GetRootType(Type type)
        {
            var parentType = type;

            while (parentType != typeof(object))
            {
                type = parentType;
                parentType = parentType.GetBaseType();
            }

            return type;
        }

        #endregion

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