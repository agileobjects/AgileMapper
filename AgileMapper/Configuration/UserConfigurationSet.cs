namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataSources;
    using Dictionaries;
    using Extensions.Internal;
    using Members;
    using ObjectPopulation;
    using Projection;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class UserConfigurationSet
    {
        private readonly MapperContext _mapperContext;
        private List<MappedObjectCachingSettings> _mappedObjectCachingSettings;
        private List<MapToNullCondition> _mapToNullConditions;
        private List<NullCollectionsSetting> _nullCollectionsSettings;
        private ConfiguredServiceProvider _serviceProvider;
        private ConfiguredServiceProvider _namedServiceProvider;
        private List<ConfiguredObjectFactory> _objectFactories;
        private MemberIdentifierSet _identifiers;
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

        #region MappedObjectCachingSettings

        private List<MappedObjectCachingSettings> MappedObjectCachingSettings
            => _mappedObjectCachingSettings ?? (_mappedObjectCachingSettings = new List<MappedObjectCachingSettings>());

        public void Add(MappedObjectCachingSettings settings)
        {
            ThrowIfConflictingItemExists(
                settings,
                _mappedObjectCachingSettings,
                (s, conflicting) => conflicting.GetConflictMessage(s));

            MappedObjectCachingSettings.AddSorted(settings);
        }

        public MappedObjectCachingMode CacheMappedObjects(IBasicMapperData basicData)
        {
            if (MappedObjectCachingSettings.None() || !basicData.TargetMember.IsComplex)
            {
                return MappedObjectCachingMode.AutoDetect;
            }

            var applicableSettings = _mappedObjectCachingSettings
                .FirstOrDefault(tm => tm.AppliesTo(basicData));

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

        private List<ConfiguredIgnoredMember> IgnoredMembers
            => _ignoredMembers ?? (_ignoredMembers = new List<ConfiguredIgnoredMember>());

        public void Add(ConfiguredIgnoredMember ignoredMember)
        {
            ThrowIfMemberIsUnmappable(ignoredMember);
            ThrowIfConflictingIgnoredMemberExists(ignoredMember, (im, cIm) => im.GetConflictMessage(cIm));
            ThrowIfConflictingDataSourceExists(ignoredMember, (im, cDsf) => im.GetConflictMessage(cDsf));

            IgnoredMembers.AddSortFilter(ignoredMember);
        }

        public ConfiguredIgnoredMember GetMemberIgnoreOrNull(IBasicMapperData mapperData)
            => _ignoredMembers.FindMatch(mapperData);

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

            if (!HasConfiguredRootDataSources && dataSourceFactory.TargetMember.IsRoot)
            {
                HasConfiguredRootDataSources = true;
            }
        }

        public bool HasConfiguredRootDataSources { get; private set; }

        public IList<IConfiguredDataSource> GetDataSources(IMemberMapperData mapperData)
            => QueryDataSourceFactories(mapperData).Project(dsf => dsf.Create(mapperData)).ToArray();

        public IEnumerable<ConfiguredDataSourceFactory> QueryDataSourceFactories(IBasicMapperData mapperData)
            => _dataSourceFactories?.FindMatches(mapperData) ?? Enumerable<ConfiguredDataSourceFactory>.Empty;

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
            if (ignoredMember.ConfigInfo.TargetMemberIsUnmappable(
                ignoredMember.TargetMember,
                ci => QueryDataSourceFactories(ci.ToMapperData()),
                out var reason))
            {
                throw new MappingConfigurationException(
                    $"{ignoredMember.TargetMember.GetPath()} will not be mapped and does not need to be ignored ({reason})");
            }
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
                .FirstOrDefault(ci => ci.ConflictsWith(configuredItem));

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
            _objectFactories?.CloneItems().CopyTo(configurations.ObjectFactories);
            _identifiers?.CloneTo(configurations.Identifiers);
            _ignoredMembers?.CloneItems().CopyTo(configurations.IgnoredMembers);
            _enumPairings?.CopyTo(configurations._enumPairings);
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
            _mappedObjectCachingSettings?.Clear();
            _mapToNullConditions?.Clear();
            _nullCollectionsSettings?.Clear();
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