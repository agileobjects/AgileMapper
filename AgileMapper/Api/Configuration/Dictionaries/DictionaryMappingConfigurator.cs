namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using AgileMapper.Configuration;
    using Dynamics;

    internal class DictionaryMappingConfigurator<TValue> :
        IGlobalDictionarySettings<TValue>,
        ISourceDictionaryTargetTypeSelector<TValue>,
        ISourceDynamicTargetTypeSelector
    {
        private readonly MappingConfigInfo _configInfo;

        internal DictionaryMappingConfigurator(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;

            if (_configInfo.SourceValueType == null)
            {
                _configInfo.ForSourceValueType<TValue>();
            }
        }

        #region Dictionary Mapping Settings

        private MappingConfigInfo GetConfigInfo()
            => (_configInfo.TargetType != null) ? _configInfo : GlobalConfigInfo;

        private MappingConfigInfo GlobalConfigInfo => _configInfo.Clone().ForAllRuleSets().ForAllTargetTypes();

        IGlobalDictionarySettings<TValue> IGlobalDictionarySettings<TValue>.UseFlattenedTargetMemberNames()
            => RegisterFlattenedTargetMemberNames(GlobalConfigInfo);

        public ISourceDictionarySettings<TValue> UseFlattenedTargetMemberNames()
            => RegisterFlattenedTargetMemberNames(GetConfigInfo());

        private DictionaryMappingConfigurator<TValue> RegisterFlattenedTargetMemberNames(MappingConfigInfo configInfo)
        {
            var flattenedJoiningNameFactory = JoiningNameFactory.Flattened(configInfo);

            _configInfo.MapperContext.UserConfigurations.Dictionaries.Add(flattenedJoiningNameFactory);
            return this;
        }

        IGlobalDictionarySettings<TValue> IGlobalDictionarySettings<TValue>.UseMemberNameSeparator(string separator)
            => RegisterMemberNameSeparator(separator, GlobalConfigInfo);

        public ISourceDictionarySettings<TValue> UseMemberNameSeparator(string separator)
            => RegisterMemberNameSeparator(separator, GetConfigInfo());

        private DictionaryMappingConfigurator<TValue> RegisterMemberNameSeparator(
            string separator,
            MappingConfigInfo configInfo)
        {
            var joiningNameFactory = JoiningNameFactory.For(separator, configInfo);

            _configInfo.MapperContext.UserConfigurations.Dictionaries.Add(joiningNameFactory);
            return this;
        }

        IGlobalDictionarySettings<TValue> IGlobalDictionarySettings<TValue>.UseElementKeyPattern(string pattern)
            => RegisterElementKeyPattern(pattern, GlobalConfigInfo);

        public ISourceDictionarySettings<TValue> UseElementKeyPattern(string pattern)
            => RegisterElementKeyPattern(pattern, GetConfigInfo());

        private DictionaryMappingConfigurator<TValue> RegisterElementKeyPattern(
            string pattern,
            MappingConfigInfo configInfo)
        {
            var keyPartFactory = ElementKeyPartFactory.For(pattern, configInfo);

            _configInfo.MapperContext.UserConfigurations.Dictionaries.Add(keyPartFactory);
            return this;
        }

        MappingConfigStartingPoint IGlobalDictionarySettings<TValue>.AndWhenMapping
            => new MappingConfigStartingPoint(_configInfo.MapperContext);

        public ISourceDictionaryTargetTypeSelector<TValue> AndWhenMapping => this;

        #endregion

        public ISourceDictionaryMappingConfigurator<TValue, TTarget> To<TTarget>()
            => CreateConfigurator<TTarget>(_configInfo.ForAllRuleSets());

        ISourceDynamicMappingConfigurator<TTarget> ISourceDynamicTargetTypeSelector.To<TTarget>()
            => CreateConfigurator<TTarget>(_configInfo.ForAllRuleSets());

        public ISourceDictionaryMappingConfigurator<TValue, TTarget> ToANew<TTarget>()
            => CreateConfigurator<TTarget>(Constants.CreateNew);

        ISourceDynamicMappingConfigurator<TTarget> ISourceDynamicTargetTypeSelector.ToANew<TTarget>()
            => CreateConfigurator<TTarget>(Constants.CreateNew);

        public ISourceDictionaryMappingConfigurator<TValue, TTarget> OnTo<TTarget>()
            => CreateConfigurator<TTarget>(Constants.Merge);

        ISourceDynamicMappingConfigurator<TTarget> ISourceDynamicTargetTypeSelector.OnTo<TTarget>()
            => CreateConfigurator<TTarget>(Constants.Merge);

        public ISourceDictionaryMappingConfigurator<TValue, TTarget> Over<TTarget>()
            => CreateConfigurator<TTarget>(Constants.Overwrite);

        ISourceDynamicMappingConfigurator<TTarget> ISourceDynamicTargetTypeSelector.Over<TTarget>()
            => CreateConfigurator<TTarget>(Constants.Overwrite);

        private SourceDictionaryMappingConfigurator<TValue, TTarget> CreateConfigurator<TTarget>(string ruleSetName)
            => CreateConfigurator<TTarget>(_configInfo.ForRuleSet(ruleSetName));

        private static SourceDictionaryMappingConfigurator<TValue, TTarget> CreateConfigurator<TTarget>(
            MappingConfigInfo configInfo)
            => new SourceDictionaryMappingConfigurator<TValue, TTarget>(configInfo);
    }
}
