namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;

    internal class MappingConfigInfo
    {
        private static readonly Type _allSourceTypes = typeof(MappingConfigInfo);
        private static readonly string _allRuleSets = Guid.NewGuid().ToString();

        private Type _sourceType;
        private Type _sourceValueType;
        private string _mappingRuleSetName;

        public MappingConfigInfo(MapperContext mapperContext)
        {
            MapperContext = mapperContext;
        }

        public GlobalContext GlobalContext => MapperContext.GlobalContext;

        public MapperContext MapperContext { get; }

        public MappingConfigInfo ForAllSourceTypes() => ForSourceType(_allSourceTypes);

        public MappingConfigInfo ForSourceType<TSource>() => ForSourceType(typeof(TSource));

        private MappingConfigInfo ForSourceType(Type sourceType)
        {
            _sourceType = sourceType;
            return this;
        }

        public bool IsForSourceType(Type sourceType)
            => (_sourceType == _allSourceTypes) || _sourceType.IsAssignableFrom(sourceType);

        public MappingConfigInfo ForAllRuleSets() => ForRuleSet(_allRuleSets);

        public MappingConfigInfo ForRuleSet(string name)
        {
            _mappingRuleSetName = name;
            return this;
        }

        public bool IsForRuleSet(string mappingRuleSetName)
        {
            return (_mappingRuleSetName == _allRuleSets) ||
                (mappingRuleSetName == _mappingRuleSetName);
        }

        public MappingConfigInfo ForSourceValueType(Type sourceValueType)
        {
            _sourceValueType = sourceValueType;
            return this;
        }

        public void ThrowIfSourceTypeDoesNotMatch<TTargetValue>()
        {
            MapperContext.ValueConverters.ThrowIfUnconvertible(_sourceValueType, typeof(TTargetValue));
        }
    }
}