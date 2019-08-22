namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using DataSources.Factories;
    using Dictionaries;
    using Extensions;
    using Extensions.Internal;
    using Members;
    using ObjectPopulation;
    using Projection;
    using ReadableExpressions.Extensions;

    internal class UserConfigurationSet
    {
        private readonly MapperContext _mapperContext;
        private ICollection<Type> _appliedConfigurationTypes;
        private List<MappedObjectCachingSetting> _mappedObjectCachingSettings;
        private List<MapToNullCondition> _mapToNullConditions;
        private List<NullCollectionsSetting> _nullCollectionsSettings;
        private List<EntityKeyMappingSetting> _entityKeyMappingSettings;
        private List<DataSourceReversalSetting> _dataSourceReversalSettings;
        private ConfiguredServiceProvider _serviceProvider;
        private ConfiguredServiceProvider _namedServiceProvider;
        private List<ConfiguredObjectFactory> _objectFactories;
        private MemberIdentifierSet _identifiers;
        private List<ConfiguredIgnoredSourceMember> _ignoredSourceMembers;
        private List<ConfiguredIgnoredMember> _ignoredMembers;
        private List<EnumMemberPair> _enumPairings;
        private DictionarySettings _dictionaries;
        private List<ConfiguredDataSourceFactory> _dataSourceFactories;
        private List<MappingCallbackFactory> _mappingCallbackFactories;
        private List<ObjectCreationCallbackFactory> _creationCallbackFactories;
        private List<ExceptionCallback> _exceptionCallbackFactories;
        private DerivedTypePairSet _derivedTypes;
        private List<RecursionDepthSettings> _recursionDepthSettings;

        public UserConfigurationSet(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public bool ValidateMappingPlans { get; set; }

        public ICollection<Type> AppliedConfigurationTypes
            => _appliedConfigurationTypes ?? (_appliedConfigurationTypes = new List<Type>());

        #region MappedObjectCachingSettings

        private List<MappedObjectCachingSetting> MappedObjectCachingSettings
            => _mappedObjectCachingSettings ?? (_mappedObjectCachingSettings = new List<MappedObjectCachingSetting>());

        public void Add(MappedObjectCachingSetting setting)
        {
            ThrowIfConflictingItemExists(
                setting,
                _mappedObjectCachingSettings,
                (s, conflicting) => conflicting.GetConflictMessage(s));

            MappedObjectCachingSettings.AddSorted(setting);
        }

        public MappedObjectCachingMode CacheMappedObjects(IBasicMapperData basicData)
        {
            if (MappedObjectCachingSettings.None() || !basicData.TargetMember.IsComplex)
            {
                return MappedObjectCachingMode.AutoDetect;
            }

            var applicableSettings = _mappedObjectCachingSettings
                .FirstOrDefault(basicData, (bd, tm) => tm.AppliesTo(bd));

            if (applicableSettings == null)
            {
                return MappedObjectCachingMode.AutoDetect;
            }

            return applicableSettings.Cache
                ? MappedObjectCachingMode.Cache
                : MappedObjectCachingMode.DoNotCache;
        }

        #endregion

        #region MapToNullConditions

        private List<MapToNullCondition> MapToNullConditions
            => _mapToNullConditions ?? (_mapToNullConditions = new List<MapToNullCondition>());

        public void Add(MapToNullCondition condition)
        {
            var conditions = MapToNullConditions;

            ThrowIfConflictingItemExists(condition, conditions, (c, cC) => c.GetConflictMessage());

            conditions.AddSorted(condition);
        }

        public Expression GetMapToNullConditionOrNull(IMemberMapperData mapperData)
            => _mapToNullConditions.FindMatch(mapperData)?.GetConditionOrNull(mapperData);

        #endregion

        #region NullCollectionSettings

        private List<NullCollectionsSetting> NullCollectionsSettings
            => _nullCollectionsSettings ?? (_nullCollectionsSettings = new List<NullCollectionsSetting>());

        public void Add(NullCollectionsSetting setting) => NullCollectionsSettings.Add(setting);

        public bool MapToNullCollections(IBasicMapperData basicData)
            => _nullCollectionsSettings?.Any(s => s.AppliesTo(basicData)) == true;

        #endregion

        #region EntityKeyMappingSettings

        private List<EntityKeyMappingSetting> EntityKeyMappingSettings
            => _entityKeyMappingSettings ?? (_entityKeyMappingSettings = new List<EntityKeyMappingSetting>());

        public void Add(EntityKeyMappingSetting setting)
        {
            ThrowIfConflictingKeyMappingSettingExists(setting);

            EntityKeyMappingSettings.AddSorted(setting);
        }

        public bool MapEntityKeys(IBasicMapperData basicData)
        {
            var applicableSetting = _entityKeyMappingSettings?
                .FirstOrDefault(basicData, (bd, s) => s.AppliesTo(bd))?
                .MapKeys;

            return (applicableSetting == true) ||
                   (basicData.RuleSet.Settings.AllowEntityKeyMapping && (applicableSetting != false));
        }

        #endregion

        #region ConfiguredDataSourceReversalSettings

        private List<DataSourceReversalSetting> DataSourceReversalSettings
            => _dataSourceReversalSettings ?? (_dataSourceReversalSettings = new List<DataSourceReversalSetting>());

        public void Add(DataSourceReversalSetting setting)
        {
            ThrowIfConflictingDataSourceReversalSettingExists(setting);

            DataSourceReversalSettings.AddSorted(setting);
        }

        public void AddReverseDataSourceFor(ConfiguredDataSourceFactory dataSourceFactory)
            => AddReverse(dataSourceFactory, isAutoReversal: false);

        private void AddReverse(ConfiguredDataSourceFactory dataSourceFactory, bool isAutoReversal)
        {
            var reverseDataSourceFactory = dataSourceFactory.CreateReverseIfAppropriate(isAutoReversal);

            if (reverseDataSourceFactory != null)
            {
                DataSourceFactories.AddSortFilter(reverseDataSourceFactory);
            }
        }

        public void RemoveReverseOf(MappingConfigInfo configInfo)
        {
            var dataSourceFactory = GetDataSourceFactoryFor(configInfo);
            var reverseConfigInfo = dataSourceFactory.GetReverseConfigInfo();

            for (var i = 0; i < _dataSourceFactories.Count; ++i)
            {
                if (_dataSourceFactories[i].ConfigInfo == reverseConfigInfo)
                {
                    _dataSourceFactories.RemoveAt(i);
                    return;
                }
            }
        }

        public bool AutoDataSourceReversalEnabled(ConfiguredDataSourceFactory dataSourceFactory)
            => AutoDataSourceReversalEnabled(dataSourceFactory, dsf => dsf.ConfigInfo.ToMapperData(dsf.TargetMember));

        public bool AutoDataSourceReversalEnabled(MappingConfigInfo configInfo)
            => AutoDataSourceReversalEnabled(configInfo, ci => ci.ToMapperData());

        private bool AutoDataSourceReversalEnabled<T>(T dataItem, Func<T, IBasicMapperData> mapperDataFactory)
        {
            if (_dataSourceReversalSettings == null)
            {
                return false;
            }

            var basicData = mapperDataFactory.Invoke(dataItem);

            return _dataSourceReversalSettings
                .FirstOrDefault(basicData, (bd, s) => s.AppliesTo(bd))?.Reverse == true;
        }

        #endregion

        #region ServiceProviders

        public void Add(ConfiguredServiceProvider serviceProvider)
        {
            if (serviceProvider.IsNamed)
            {
                if (_namedServiceProvider != null)
                {
                    throw new MappingConfigurationException("A named service provider has already been configured.");
                }

                _namedServiceProvider = serviceProvider;
                return;
            }

            if (_serviceProvider != null)
            {
                throw new MappingConfigurationException("A service provider has already been configured.");
            }

            _serviceProvider = serviceProvider;
        }

        public TService GetServiceOrThrow<TService>(string name)
        {
            var hasName = !string.IsNullOrEmpty(name);

            var serviceProvider = hasName
                ? _namedServiceProvider
                : _serviceProvider ?? _namedServiceProvider;

            if (serviceProvider != null)
            {
                return serviceProvider.GetService<TService>(name);
            }

            if (hasName)
            {
                throw new MappingConfigurationException(
                    "No named service providers configured. " +
                    "Use Mapper.WhenMapping.UseServiceProvider(provider) to configure a named service provider");
            }

            throw new MappingConfigurationException(
                "No service providers configured. " +
                "Use Mapper.WhenMapping.UseServiceProvider(provider) to configure a service provider");
        }

        public TServiceProvider GetServiceProviderOrThrow<TServiceProvider>()
            where TServiceProvider : class
        {
            var serviceProvider =
                _serviceProvider?.ProviderObject as TServiceProvider ??
                _namedServiceProvider?.ProviderObject as TServiceProvider;

            if (serviceProvider != null)
            {
                return serviceProvider;
            }

            var providerType = typeof(TServiceProvider).GetFriendlyName();

            throw new MappingConfigurationException(
                $"No service provider of type {providerType} is configured");
        }

        #endregion

        #region ObjectFactories

        private List<ConfiguredObjectFactory> ObjectFactories
            => _objectFactories ?? (_objectFactories = new List<ConfiguredObjectFactory>());

        public void Add(ConfiguredObjectFactory objectFactory)
        {
            ThrowIfConflictingItemExists(
                objectFactory,
                _objectFactories,
                (of1, of2) => $"An object factory for type {of1.ObjectTypeName} has already been configured");

            ObjectFactories.AddSortFilter(objectFactory);
        }

        public IEnumerable<ConfiguredObjectFactory> GetObjectFactories(IBasicMapperData mapperData)
            => _objectFactories.FindMatches(mapperData);

        #endregion

        public MemberIdentifierSet Identifiers => _identifiers ?? (_identifiers = new MemberIdentifierSet(_mapperContext));

        #region IgnoredMembers

        private List<ConfiguredIgnoredSourceMember> IgnoredSourceMembers
            => _ignoredSourceMembers ?? (_ignoredSourceMembers = new List<ConfiguredIgnoredSourceMember>());

        public bool HasSourceMemberIgnores(IBasicMapperData mapperData)
            => _ignoredSourceMembers?.Any(sm => sm.CouldApplyTo(mapperData)) == true;

        public void Add(ConfiguredIgnoredSourceMember ignoredSourceMember)
        {
            IgnoredSourceMembers.Add(ignoredSourceMember);
        }

        public ConfiguredIgnoredSourceMember GetSourceMemberIgnoreOrNull(IBasicMapperData mapperData)
            => _ignoredSourceMembers.FindMatch(mapperData);

        private List<ConfiguredIgnoredMember> IgnoredMembers
            => _ignoredMembers ?? (_ignoredMembers = new List<ConfiguredIgnoredMember>());

        public void Add(ConfiguredIgnoredMember ignoredMember)
        {
            ThrowIfMemberIsUnmappable(ignoredMember);
            ThrowIfConflictingIgnoredMemberExists(ignoredMember, (im, cIm) => im.GetConflictMessage(cIm));
            ThrowIfConflictingDataSourceExists(ignoredMember, (im, cDsf) => im.GetConflictMessage(cDsf));

            IgnoredMembers.AddSortFilter(ignoredMember);
        }

        public IList<ConfiguredIgnoredMember> GetPotentialMemberIgnores(IBasicMapperData mapperData)
            => _ignoredMembers.FindPotentialMatches(mapperData);

        #endregion

        #region EnumPairing

        private List<EnumMemberPair> EnumPairings
            => _enumPairings ?? (_enumPairings = new List<EnumMemberPair>());

        public void Add(EnumMemberPair enumPairing) => EnumPairings.Add(enumPairing);

        public IEnumerable<EnumMemberPair> GetEnumPairingsFor(Type sourceEnumType, Type targetEnumType)
            => _enumPairings?.Filter(ep => ep.IsFor(sourceEnumType, targetEnumType)) ?? Enumerable<EnumMemberPair>.Empty;

        #endregion

        public DictionarySettings Dictionaries =>
            _dictionaries ?? (_dictionaries = new DictionarySettings(_mapperContext));

        #region DataSources

        public IEnumerable<TFactory> QueryDataSourceFactories<TFactory>()
            where TFactory : ConfiguredDataSourceFactory
        {
            return _dataSourceFactories?.OfType<TFactory>() ?? Enumerable<TFactory>.Empty;
        }

        private List<ConfiguredDataSourceFactory> DataSourceFactories
            => _dataSourceFactories ?? (_dataSourceFactories = new List<ConfiguredDataSourceFactory>());

        public void Add(ConfiguredDataSourceFactory dataSourceFactory)
        {
            if (!dataSourceFactory.TargetMember.IsRoot)
            {
                ThrowIfConflictingIgnoredMemberExists(dataSourceFactory);
                ThrowIfConflictingDataSourceExists(dataSourceFactory, (dsf, cDsf) => dsf.GetConflictMessage(cDsf));
            }

            DataSourceFactories.AddSortFilter(dataSourceFactory);

            if (dataSourceFactory.TargetMember.IsRoot)
            {
                HasConfiguredToTargetDataSources = true;
                return;
            }

            if (AutoDataSourceReversalEnabled(dataSourceFactory))
            {
                AddReverse(dataSourceFactory, isAutoReversal: true);
            }
        }

        public ConfiguredDataSourceFactory GetDataSourceFactoryFor(MappingConfigInfo configInfo)
            => _dataSourceFactories.First(configInfo, (ci, dsf) => dsf.ConfigInfo == ci);

        public bool HasConfiguredToTargetDataSources { get; private set; }

        public IList<ConfiguredDataSourceFactory> GetPotentialDataSourceFactories(IMemberMapperData mapperData)
            => _dataSourceFactories.FindPotentialMatches(mapperData);

        public IList<IConfiguredDataSource> GetDataSourcesForToTarget(IMemberMapperData mapperData)
        {
            if (!HasConfiguredToTargetDataSources)
            {
                return Enumerable<IConfiguredDataSource>.EmptyArray;
            }

            var toTargetDataSources = QueryDataSourceFactories(mapperData)
                .Filter(dsf => dsf.TargetMember.IsRoot)
                .Project(mapperData, (md, dsf) => dsf.Create(md))
                .ToArray();

            return toTargetDataSources;
        }

        public IEnumerable<ConfiguredDataSourceFactory> QueryDataSourceFactories(IBasicMapperData mapperData)
            => _dataSourceFactories.FindMatches(mapperData);

        #endregion

        #region MappingCallbacks

        private List<MappingCallbackFactory> MappingCallbackFactories
            => _mappingCallbackFactories ?? (_mappingCallbackFactories = new List<MappingCallbackFactory>());

        public void Add(MappingCallbackFactory callbackFactory) => MappingCallbackFactories.Add(callbackFactory);

        public Expression GetCallbackOrNull(
            CallbackPosition position,
            IBasicMapperData basicData,
            IMemberMapperData mapperData)
        {
            return _mappingCallbackFactories?.FirstOrDefault(f => f.AppliesTo(position, basicData))?.Create(mapperData);
        }

        private List<ObjectCreationCallbackFactory> CreationCallbackFactories
            => _creationCallbackFactories ?? (_creationCallbackFactories = new List<ObjectCreationCallbackFactory>());

        public void Add(ObjectCreationCallbackFactory callbackFactory) => CreationCallbackFactories.Add(callbackFactory);

        public Expression GetCreationCallbackOrNull(CallbackPosition position, IMemberMapperData mapperData)
            => _creationCallbackFactories?.FirstOrDefault(f => f.AppliesTo(position, mapperData))?.Create(mapperData);

        #endregion

        #region ExceptionCallbacks

        private List<ExceptionCallback> ExceptionCallbackFactories
            => _exceptionCallbackFactories ?? (_exceptionCallbackFactories = new List<ExceptionCallback>());

        public void Add(ExceptionCallback callback) => ExceptionCallbackFactories.Add(callback);

        public Expression GetExceptionCallbackOrNull(IBasicMapperData mapperData)
            => _exceptionCallbackFactories?.FindMatch(mapperData)?.Callback;

        #endregion

        public DerivedTypePairSet DerivedTypes => _derivedTypes ?? (_derivedTypes = new DerivedTypePairSet());

        #region RecursionDepthSettings

        private List<RecursionDepthSettings> RecursionDepthSettings
            => _recursionDepthSettings ?? (_recursionDepthSettings = new List<RecursionDepthSettings>());

        public void Add(RecursionDepthSettings settings)
        {
            RecursionDepthSettings.Add(settings);
        }

        public bool ShortCircuitRecursion(IBasicMapperData mapperData)
        {
            if (_recursionDepthSettings == null)
            {
                return true;
            }

            return RecursionDepthSettings.FindMatch(mapperData)?.IsBeyondDepth(mapperData) != false;
        }

        #endregion

        #region Validation

        private void ThrowIfMemberIsUnmappable(ConfiguredIgnoredMember ignoredMember)
        {
            if (ignoredMember.ConfigInfo.ToMapperData().TargetMemberIsUnmappable(
                ignoredMember.TargetMember,
                QueryDataSourceFactories,
                this,
                out var reason))
            {
                throw new MappingConfigurationException(
                    $"{ignoredMember.TargetMember.GetPath()} will not be mapped and does not need to be ignored ({reason})");
            }
        }

        private void ThrowIfConflictingKeyMappingSettingExists(EntityKeyMappingSetting setting)
        {
            if ((_entityKeyMappingSettings == null) && !setting.MapKeys)
            {
                throw new MappingConfigurationException("Entity key mapping is disabled by default");
            }

            ThrowIfConflictingItemExists(
                setting,
                _entityKeyMappingSettings,
                (s, conflicting) => conflicting.GetConflictMessage(s));
        }

        private void ThrowIfConflictingDataSourceReversalSettingExists(DataSourceReversalSetting setting)
        {
            if ((_dataSourceReversalSettings == null) && !setting.Reverse)
            {
                throw new MappingConfigurationException("Configured data source reversal is disabled by default");
            }

            ThrowIfConflictingItemExists(
                setting,
                _dataSourceReversalSettings,
                (s, conflicting) => conflicting.GetConflictMessage(s));
        }

        internal void ThrowIfConflictingIgnoredMemberExists<TConfiguredItem>(TConfiguredItem configuredItem)
            where TConfiguredItem : UserConfiguredItemBase
        {
            ThrowIfConflictingIgnoredMemberExists(configuredItem, (ci, im) => im.GetConflictMessage(ci));
        }

        private void ThrowIfConflictingIgnoredMemberExists<TConfiguredItem>(
            TConfiguredItem configuredItem,
            Func<TConfiguredItem, ConfiguredIgnoredMember, string> messageFactory)
            where TConfiguredItem : UserConfiguredItemBase
        {
            ThrowIfConflictingItemExists(configuredItem, _ignoredMembers, messageFactory);
        }

        internal void ThrowIfConflictingDataSourceExists<TConfiguredItem>(
            TConfiguredItem configuredItem,
            Func<TConfiguredItem, ConfiguredDataSourceFactory, string> messageFactory)
            where TConfiguredItem : UserConfiguredItemBase
        {
            ThrowIfConflictingItemExists(configuredItem, _dataSourceFactories, messageFactory);
        }

        private static void ThrowIfConflictingItemExists<TConfiguredItem, TExistingItem>(
            TConfiguredItem configuredItem,
            IList<TExistingItem> existingItems,
            Func<TConfiguredItem, TExistingItem, string> messageFactory)
            where TConfiguredItem : UserConfiguredItemBase
            where TExistingItem : UserConfiguredItemBase
        {
            var conflictingItem = existingItems?
                .FirstOrDefault(configuredItem, (sci, ci) => ci.ConflictsWith(sci));

            if (conflictingItem == null)
            {
                return;
            }

            var conflictMessage = messageFactory.Invoke(configuredItem, conflictingItem);

            throw new MappingConfigurationException(conflictMessage);
        }

        #endregion

        public void CloneTo(UserConfigurationSet configurations)
        {
            configurations.ValidateMappingPlans = ValidateMappingPlans;
            _mappedObjectCachingSettings?.CopyTo(configurations.MappedObjectCachingSettings);
            _mapToNullConditions?.CopyTo(configurations.MapToNullConditions);
            _nullCollectionsSettings?.CopyTo(configurations.NullCollectionsSettings);
            _entityKeyMappingSettings?.CopyTo(configurations.EntityKeyMappingSettings);
            _dataSourceReversalSettings?.CopyTo(configurations.DataSourceReversalSettings);
            _objectFactories?.CloneItems().CopyTo(configurations.ObjectFactories);
            _identifiers?.CloneTo(configurations.Identifiers);
            _ignoredMembers?.CloneItems().CopyTo(configurations.IgnoredMembers);
            _enumPairings?.CopyTo(configurations.EnumPairings);
            _dictionaries?.CloneTo(configurations.Dictionaries);
            _dataSourceFactories?.CloneItems().CopyTo(configurations.DataSourceFactories);
            _mappingCallbackFactories?.CopyTo(configurations.MappingCallbackFactories);
            _creationCallbackFactories?.CopyTo(configurations.CreationCallbackFactories);
            _exceptionCallbackFactories?.CopyTo(configurations.ExceptionCallbackFactories);
            _derivedTypes?.CloneTo(configurations.DerivedTypes);
        }

        public void Reset()
        {
            ValidateMappingPlans = false;
            _appliedConfigurationTypes?.Clear();
            _mappedObjectCachingSettings?.Clear();
            _mapToNullConditions?.Clear();
            _nullCollectionsSettings?.Clear();
            _entityKeyMappingSettings?.Clear();
            _dataSourceReversalSettings?.Clear();
            _serviceProvider = _namedServiceProvider = null;
            _objectFactories?.Clear();
            _identifiers?.Reset();
            _ignoredMembers?.Clear();
            _enumPairings?.Clear();
            _dictionaries?.Reset();
            _dataSourceFactories?.Clear();
            _mappingCallbackFactories?.Clear();
            _creationCallbackFactories?.Clear();
            _exceptionCallbackFactories?.Clear();
            _derivedTypes?.Reset();
        }
    }
}