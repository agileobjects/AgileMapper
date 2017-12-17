namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using AgileMapper.Configuration;
    using Dynamics;

    internal class DictionaryMappingConfigurator<TValue> :
        DictionaryMappingConfiguratorBase<object, object>,
        IGlobalDictionarySettings<TValue>,
        ISourceDictionaryTargetTypeSelector<TValue>,
        ISourceDynamicTargetTypeSelector
    {
        private readonly MappingConfigInfo _configInfo;

        internal DictionaryMappingConfigurator(MappingConfigInfo configInfo)
            : base(configInfo)
        {
            _configInfo = configInfo;
        }

        #region Mapping Settings

        #region UseFlattenedTargetMemberNames

        IGlobalDictionarySettings<TValue> IGlobalDictionarySettings<TValue>.UseFlattenedTargetMemberNames()
            => RegisterFlattenedTargetMemberNames(GetGlobalConfigInfo());

        public ISourceDictionarySettings<TValue> UseFlattenedTargetMemberNames()
            => RegisterFlattenedTargetMemberNames(GetConfigInfo());

        ISourceDynamicSettings ISourceDynamicSettings.UseFlattenedTargetMemberNames()
            => RegisterFlattenedTargetMemberNames(GetConfigInfo());

        private DictionaryMappingConfigurator<TValue> RegisterFlattenedTargetMemberNames(MappingConfigInfo configInfo)
        {
            SetupFlattenedTargetMemberNames(configInfo);
            return this;
        }

        #endregion

        #region UseMemberNameSeparator

        IGlobalDictionarySettings<TValue> IGlobalDictionarySettings<TValue>.UseMemberNameSeparator(string separator)
            => RegisterMemberNameSeparator(separator, GetGlobalConfigInfo());

        public ISourceDictionarySettings<TValue> UseMemberNameSeparator(string separator)
            => RegisterMemberNameSeparator(separator, GetConfigInfo());

        ISourceDynamicSettings ISourceDynamicSettings.UseMemberNameSeparator(string separator)
            => RegisterMemberNameSeparator(separator, GetConfigInfo());

        private DictionaryMappingConfigurator<TValue> RegisterMemberNameSeparator(
            string separator,
            MappingConfigInfo configInfo)
        {
            SetupMemberNameSeparator(separator, configInfo);
            return this;
        }

        #endregion

        #region UseElementKeyPattern

        IGlobalDictionarySettings<TValue> IGlobalDictionarySettings<TValue>.UseElementKeyPattern(string pattern)
            => RegisterElementKeyPattern(pattern, GetGlobalConfigInfo());

        public ISourceDictionarySettings<TValue> UseElementKeyPattern(string pattern)
            => RegisterElementKeyPattern(pattern, GetConfigInfo());

        ISourceDynamicSettings ISourceDynamicSettings.UseElementKeyPattern(string pattern)
            => RegisterElementKeyPattern(pattern, GetConfigInfo());

        private DictionaryMappingConfigurator<TValue> RegisterElementKeyPattern(
            string pattern,
            MappingConfigInfo configInfo)
        {
            SetupElementKeyPattern(pattern, configInfo);
            return this;
        }

        #endregion

        private MappingConfigInfo GetConfigInfo()
            => (_configInfo.TargetType != typeof(object)) ? _configInfo.Clone() : GetGlobalConfigInfo();

        private MappingConfigInfo GetGlobalConfigInfo() => _configInfo.Clone().ForAllRuleSets().ForAllTargetTypes();

        #region AndWhenMapping

        MappingConfigStartingPoint IGlobalDictionarySettings<TValue>.AndWhenMapping
            => new MappingConfigStartingPoint(_configInfo.MapperContext);

        public ISourceDictionaryTargetTypeSelector<TValue> AndWhenMapping => this;

        ISourceDynamicTargetTypeSelector ISourceDynamicSettings.AndWhenMapping => this;

        #endregion

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
