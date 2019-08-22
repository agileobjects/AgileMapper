namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;

    internal class DerivedTypePairSet
    {
        private static readonly object _lookupSync = new object();

        private readonly Dictionary<Type, List<DerivedTypePair>> _typePairsByTargetType;
        private readonly List<SourceAndTargetTypesKey> _checkedTypes;

        public DerivedTypePairSet()
        {
            _typePairsByTargetType = new Dictionary<Type, List<DerivedTypePair>>();
            _checkedTypes = new List<SourceAndTargetTypesKey>();
        }

        // ReSharper disable once InconsistentlySynchronizedField
        private int CheckedTypesCount => _checkedTypes.Count;

        public void Add(DerivedTypePair typePair)
        {
            if (typePair.IsImplementationPairing)
            {
                AddTypePairFor(typePair.ConfigInfo.TargetType, typePair);
                return;
            }

            var targetType = typePair.DerivedTargetType.GetBaseType();

            while (targetType != typeof(object))
            {
                AddTypePairFor(targetType, typePair);
                targetType = targetType.GetBaseType();
            }
        }

        private void AddTypePairFor(Type targetType, DerivedTypePair typePair)
        {
            if (_typePairsByTargetType.TryGetValue(targetType, out var typePairs))
            {
                RemoveConflictingPairIfAppropriate(typePair, typePairs);
                typePairs.AddSorted(typePair);
                return;
            }

            _typePairsByTargetType[targetType] = new List<DerivedTypePair> { typePair };
        }

        private static void RemoveConflictingPairIfAppropriate(
            DerivedTypePair typePair,
            IList<DerivedTypePair> typePairs)
        {
            if (typePair.HasConfiguredCondition)
            {
                return;
            }

            var existingTypePair = typePairs.FirstOrDefault(tp =>
                !tp.HasConfiguredCondition && (tp.DerivedSourceType == typePair.DerivedSourceType));

            if (existingTypePair != null)
            {
                typePairs.Remove(existingTypePair);
            }
        }

        public IList<DerivedTypePair> GetImplementationTypePairsFor(
            IBasicMapperData mapperData,
            MapperContext mapperContext)
        {
            if (_typePairsByTargetType.TryGetValue(mapperData.TargetType, out var typePairs))
            {
                return typePairs
                    .Filter(mapperData, (md, tp) => tp.IsImplementationPairing && tp.AppliesTo(md))
                    .ToArray();
            }

            return Enumerable<DerivedTypePair>.EmptyArray;
        }

        public IList<DerivedTypePair> GetDerivedTypePairsFor(
            IBasicMapperData mapperData,
            MapperContext mapperContext)
        {
            LookForDerivedTypePairs(mapperData, mapperContext);

            if (_typePairsByTargetType.None())
            {
                return Enumerable<DerivedTypePair>.EmptyArray;
            }

            if (_typePairsByTargetType.TryGetValue(mapperData.TargetType, out var typePairs))
            {
                return typePairs.Filter(mapperData, (md, tp) => tp.AppliesTo(md)).ToArray();
            }

            return Enumerable<DerivedTypePair>.EmptyArray;
        }

        #region Auto-Registration

        private void LookForDerivedTypePairs(ITypePair mapperData, MapperContext mapperContext)
        {
            var rootSourceType = GetRootType(mapperData.SourceType);
            var rootTargetType = GetRootType(mapperData.TargetType);
            var typesKey = new SourceAndTargetTypesKey(rootSourceType, rootTargetType);

            if (TypesChecked(typesKey, 0))
            {
                return;
            }

            // ReSharper disable once InconsistentlySynchronizedField
            var currentTypeCount = CheckedTypesCount;

            lock (_lookupSync)
            {
                if ((CheckedTypesCount > currentTypeCount) && TypesChecked(typesKey, startIndex: currentTypeCount))
                {
                    return;
                }

                Store(typesKey);

                if (rootSourceType == rootTargetType)
                {
                    AddSameRootTypePairs(rootSourceType, mapperContext);
                    return;
                }

                if (SkipDerivedTypePairsLookup(
                    rootSourceType,
                    rootTargetType,
                    out var derivedTargetTypeNameFactory))
                {
                    return;
                }

                var derivedSourceTypes = GlobalContext.Instance.DerivedTypes.GetTypesDerivedFrom(rootSourceType);

                if (derivedSourceTypes.None())
                {
                    return;
                }

                var derivedTargetTypes = GlobalContext.Instance.DerivedTypes.GetTypesDerivedFrom(rootTargetType);

                if (derivedTargetTypes.None())
                {
                    return;
                }

                var candidatePairsData = derivedSourceTypes.ProjectToArray(t => new
                {
                    DerivedSourceType = t,
                    DerivedTargetTypeName = derivedTargetTypeNameFactory.Invoke(t)
                });

                foreach (var candidatePairData in candidatePairsData)
                {
                    var derivedTargetType = derivedTargetTypes
                        .FirstOrDefault(t => t.Name == candidatePairData.DerivedTargetTypeName);

                    if (derivedTargetType == null)
                    {
                        continue;
                    }

                    var derivedTypePair = CreatePairFor(
                        rootSourceType,
                        candidatePairData.DerivedSourceType,
                        rootTargetType,
                        derivedTargetType,
                        mapperContext);

                    Add(derivedTypePair);
                }
            }
        }

        private bool TypesChecked(SourceAndTargetTypesKey typesKey, int startIndex)
        {
            // ReSharper disable InconsistentlySynchronizedField
            if (CheckedTypesCount == 0)
            {
                return false;
            }

            if ((typesKey < _checkedTypes[0]) && (typesKey > _checkedTypes.Last()))
            {
                return false;
            }

            var lowerBound = Math.Max(startIndex, 0);
            var upperBound = CheckedTypesCount - 1;

            while (lowerBound <= upperBound)
            {
                var searchIndex = (lowerBound + upperBound) / 2;

                if (_checkedTypes[searchIndex].Equals(typesKey))
                {
                    return true;
                }

                if (_checkedTypes[searchIndex] > typesKey)
                {
                    upperBound = searchIndex - 1;
                }
                else
                {
                    lowerBound = searchIndex + 1;
                }
            }

            return false;
        }

        private void Store(SourceAndTargetTypesKey typesKey)
        {
            if (CheckedTypesCount == 0)
            {
                StoreKeyAt(0, typesKey, insert: false);
                return;
            }

            if (typesKey < _checkedTypes[0])
            {
                StoreKeyAt(0, typesKey, insert: true);
                return;
            }

            if (typesKey > _checkedTypes.Last())
            {
                StoreKeyAt(CheckedTypesCount, typesKey, insert: false);
                return;
            }

            var lowerBound = 1;
            var upperBound = CheckedTypesCount - 2;

            while (true)
            {
                if ((upperBound - lowerBound) <= 1)
                {
                    while (lowerBound <= upperBound)
                    {
                        if (_checkedTypes[lowerBound] < typesKey)
                        {
                            ++lowerBound;
                            continue;
                        }

                        break;
                    }

                    StoreKeyAt(lowerBound, typesKey, insert: true);
                    return;
                }

                var searchIndex = (lowerBound + upperBound) / 2;

                if (_checkedTypes[searchIndex] > typesKey)
                {
                    upperBound = searchIndex - 1;
                }
                else
                {
                    lowerBound = searchIndex + 1;
                }
            }
        }

        private void StoreKeyAt(int i, SourceAndTargetTypesKey typesKey, bool insert)
        {
            if (insert)
            {
                _checkedTypes.Insert(i, typesKey);
                return;
            }

            _checkedTypes.Add(typesKey);
        }
        // ReSharper restore InconsistentlySynchronizedField

        private void AddSameRootTypePairs(Type rootType, MapperContext mapperContext)
        {
            var derivedTypes = GlobalContext.Instance.DerivedTypes.GetTypesDerivedFrom(rootType);

            foreach (var derivedType in derivedTypes)
            {
                Add(CreatePairFor(rootType, derivedType, rootType, derivedType, mapperContext));
            }
        }

        private static bool SkipDerivedTypePairsLookup(
            Type rootSourceType,
            Type rootTargetType,
            out Func<Type, string> derivedTargetTypeNameFactory)
        {
            if (rootSourceType.IsSealed() || rootTargetType.IsSealed() ||
                rootSourceType.IsFromBcl() || rootTargetType.IsFromBcl())
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
            if (type.IsInterface())
            {
                return type;
            }

            if (type.IsValueType())
            {
                return typeof(ValueType);
            }

            var parentType = type;

            while (parentType != typeof(object))
            {
                type = parentType;
                parentType = parentType.GetBaseType();
            }

            return type;
        }

        private static DerivedTypePair CreatePairFor(
            Type rootSourceType,
            Type derivedSourceType,
            Type rootTargetType,
            Type derivedTargetType,
            MapperContext mapperContext)
        {
            var configInfo = new MappingConfigInfo(mapperContext)
                .ForAllRuleSets()
                .ForSourceType(rootSourceType)
                .ForTargetType(rootTargetType);

            var derivedTypePair = new DerivedTypePair(
                configInfo,
                derivedSourceType,
                derivedTargetType);

            return derivedTypePair;
        }

        #endregion

        public void Reset() => _typePairsByTargetType.Clear();

        public void CloneTo(DerivedTypePairSet derivedTypes)
        {
            foreach (var targetTypeAndTypePair in _typePairsByTargetType)
            {
                derivedTypes._typePairsByTargetType
                    .Add(targetTypeAndTypePair.Key, targetTypeAndTypePair.Value);
            }

            lock (_lookupSync)
            {
                derivedTypes._checkedTypes.AddRange(_checkedTypes);
            }
        }
    }
}