namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;

    internal class DerivedTypePairSet
    {
        private static readonly object _lookupSync = new object();

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
                // ReSharper disable once AssignNullToNotNullAttribute
                if (_typePairsByTargetType.TryGetValue(parentType, out var typePairs))
                {
                    RemoveConflictingPairIfAppropriate(typePair, typePairs);

                    typePairs.AddSorted(typePair);
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
                return typePairs.Filter(tp => tp.AppliesTo(mapperData)).ToArray();
            }

            return Enumerable<DerivedTypePair>.EmptyArray;
        }

        #region Auto-Registration

        private void LookForDerivedTypePairs(ITypePair mapperData, MapperContext mapperContext)
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

                var candidatePairsData = derivedSourceTypes
                    .Project(t => new
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
                derivedTypes._autoCheckedTypes.AddRange(_autoCheckedTypes);
            }
        }
    }
}