namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using Members;

    internal class MapperCreationCallbackKey
    {
        public static readonly IEqualityComparer<MapperCreationCallbackKey> Comparer = new KeyComparer();

        private readonly MappingRuleSet _ruleSet;
        private readonly Type _sourceType;
        private readonly Type _targetType;

        public MapperCreationCallbackKey(IBasicMapperData mapperData)
            : this(mapperData.RuleSet, mapperData.SourceType, mapperData.TargetType)
        {
        }

        public MapperCreationCallbackKey(
            MappingRuleSet ruleSet,
            Type sourceType,
            Type targetType)
        {
            _ruleSet = ruleSet;
            _sourceType = sourceType;
            _targetType = targetType;
        }

        private class KeyComparer : IEqualityComparer<MapperCreationCallbackKey>
        {
            public bool Equals(MapperCreationCallbackKey x, MapperCreationCallbackKey y)
            {
                return (x._ruleSet == y._ruleSet) &&
                       (x._sourceType == y._sourceType) &&
                       (x._targetType == y._targetType);
            }

            #region ExcludeFromCodeCoverage
#if CODE_COVERAGE_SUPPORTED
            [ExcludeFromCodeCoverage]
#endif
            #endregion
            public int GetHashCode(MapperCreationCallbackKey obj) => 0;
        }
    }
}