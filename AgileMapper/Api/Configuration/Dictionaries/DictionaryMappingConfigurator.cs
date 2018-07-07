namespace AgileObjects.AgileMapper.Api.Configuration.Dictionaries
{
    using AgileMapper.Configuration;
    using AgileMapper.Configuration.Dictionaries;
#if DYNAMIC_SUPPORTED
    using Dynamics;
#endif
    using static AgileMapper.Configuration.Dictionaries.DictionaryContext;

    internal class DictionaryMappingConfigurator<TValue> :
        DictionaryMappingConfiguratorBase<object, object>,
        IGlobalDictionarySettings<TValue>,
        ISourceDictionaryTargetTypeSelector<TValue>
#if DYNAMIC_SUPPORTED
        ,
        IGlobalDynamicSettings,
        ISourceDynamicTargetTypeSelector
#endif
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
            => RegisterFlattenedTargetMemberNames(GetGlobalConfigInfo(All));

        public ISourceDictionarySettings<TValue> UseFlattenedTargetMemberNames()
            => RegisterFlattenedTargetMemberNames(GetConfigInfo(SourceOnly));

#if DYNAMIC_SUPPORTED
        IGlobalDynamicSettings IGlobalDynamicSettings.UseFlattenedTargetMemberNames()
            => RegisterFlattenedTargetMemberNames(GetGlobalConfigInfo(All));

        ISourceDynamicSettings ISourceDynamicSettings.UseFlattenedTargetMemberNames()
            => RegisterFlattenedTargetMemberNames(GetConfigInfo(SourceOnly));
#endif
        private DictionaryMappingConfigurator<TValue> RegisterFlattenedTargetMemberNames(MappingConfigInfo configInfo)
        {
            SetupFlattenedTargetMemberNames(configInfo);
            return this;
        }

        #endregion

        #region UseMemberNameSeparator

        IGlobalDictionarySettings<TValue> IGlobalDictionarySettings<TValue>.UseMemberNameSeparator(string separator)
            => RegisterMemberNameSeparator(separator, GetGlobalConfigInfo(All));

        public ISourceDictionarySettings<TValue> UseMemberNameSeparator(string separator)
            => RegisterMemberNameSeparator(separator, GetConfigInfo(SourceOnly));

#if DYNAMIC_SUPPORTED
        IGlobalDynamicSettings IGlobalDynamicSettings.UseMemberNameSeparator(string separator)
            => RegisterMemberNameSeparator(separator, GetGlobalConfigInfo(All));

        ISourceDynamicSettings ISourceDynamicSettings.UseMemberNameSeparator(string separator)
            => RegisterMemberNameSeparator(separator, GetConfigInfo(SourceOnly));
#endif
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
            => RegisterElementKeyPattern(pattern, GetGlobalConfigInfo(All));

        public ISourceDictionarySettings<TValue> UseElementKeyPattern(string pattern)
            => RegisterElementKeyPattern(pattern, GetConfigInfo(SourceOnly));

#if DYNAMIC_SUPPORTED
        IGlobalDynamicSettings IGlobalDynamicSettings.UseElementKeyPattern(string pattern)
            => RegisterElementKeyPattern(pattern, GetGlobalConfigInfo(All));

        ISourceDynamicSettings ISourceDynamicSettings.UseElementKeyPattern(string pattern)
            => RegisterElementKeyPattern(pattern, GetConfigInfo(SourceOnly));
#endif
        private DictionaryMappingConfigurator<TValue> RegisterElementKeyPattern(
            string pattern,
            MappingConfigInfo configInfo)
        {
            SetupElementKeyPattern(pattern, configInfo);
            return this;
        }

        #endregion

        private MappingConfigInfo GetConfigInfo(DictionaryContext context)
        {
            return (_configInfo.TargetType != typeof(object))
                ? _configInfo.Clone().Set(context)
                : GetGlobalConfigInfo(context);
        }

        private MappingConfigInfo GetGlobalConfigInfo(DictionaryContext context)
            => _configInfo.Clone().ForAllRuleSets().ForAllTargetTypes().Set(context);

        #region AndWhenMapping

        MappingConfigStartingPoint IGlobalDictionarySettings<TValue>.AndWhenMapping
            => new MappingConfigStartingPoint(_configInfo.MapperContext);

        public ISourceDictionaryTargetTypeSelector<TValue> AndWhenMapping => this;

#if DYNAMIC_SUPPORTED
        MappingConfigStartingPoint IGlobalDynamicSettings.AndWhenMapping
            => new MappingConfigStartingPoint(_configInfo.MapperContext);

        ISourceDynamicTargetTypeSelector ISourceDynamicSettings.AndWhenMapping => this;
#endif
        #endregion

        #endregion

        #region Dictionaries

        public ISourceDictionaryMappingConfigurator<TValue, TTarget> To<TTarget>()
            => CreateDictionaryConfigurator<TTarget>(_configInfo.ForAllRuleSets());

        public ISourceDictionaryMappingConfigurator<TValue, TTarget> ToANew<TTarget>()
            => CreateDictionaryConfigurator<TTarget>(Constants.CreateNew);

        public ISourceDictionaryMappingConfigurator<TValue, TTarget> OnTo<TTarget>()
            => CreateDictionaryConfigurator<TTarget>(Constants.Merge);

        public ISourceDictionaryMappingConfigurator<TValue, TTarget> Over<TTarget>()
            => CreateDictionaryConfigurator<TTarget>(Constants.Overwrite);

        private SourceDictionaryMappingConfigurator<TValue, TTarget> CreateDictionaryConfigurator<TTarget>(
            string ruleSetName)
            => CreateDictionaryConfigurator<TTarget>(_configInfo.ForRuleSet(ruleSetName));

        private static SourceDictionaryMappingConfigurator<TValue, TTarget> CreateDictionaryConfigurator<TTarget>(
            MappingConfigInfo configInfo)
            => new SourceDictionaryMappingConfigurator<TValue, TTarget>(configInfo);

        #endregion

#if DYNAMIC_SUPPORTED
        #region Dynamics

        ISourceDynamicMappingConfigurator<TTarget> ISourceDynamicTargetTypeSelector.To<TTarget>()
            => CreateDynamicConfigurator<TTarget>(_configInfo.ForAllRuleSets());

        ISourceDynamicMappingConfigurator<TTarget> ISourceDynamicTargetTypeSelector.ToANew<TTarget>()
            => CreateDynamicConfigurator<TTarget>(Constants.CreateNew);

        ISourceDynamicMappingConfigurator<TTarget> ISourceDynamicTargetTypeSelector.OnTo<TTarget>()
            => CreateDynamicConfigurator<TTarget>(Constants.Merge);

        ISourceDynamicMappingConfigurator<TTarget> ISourceDynamicTargetTypeSelector.Over<TTarget>()
            => CreateDynamicConfigurator<TTarget>(Constants.Overwrite);

        private SourceDynamicMappingConfigurator<TTarget> CreateDynamicConfigurator<TTarget>(
            string ruleSetName)
            => CreateDynamicConfigurator<TTarget>(_configInfo.ForRuleSet(ruleSetName));

        private static SourceDynamicMappingConfigurator<TTarget> CreateDynamicConfigurator<TTarget>(
            MappingConfigInfo configInfo)
            => new SourceDynamicMappingConfigurator<TTarget>(configInfo);

        #endregion
#endif
    }
}
