namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using AgileMapper.DataSources;
    using DataSources;
    using Dictionaries;
    using Extensions;
    using Extensions.Internal;
    using MemberIgnores;
    using MemberIgnores.SourceValueFilters;
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
        private List<ConfiguredSourceValueFilter> _sourceValueFilters;
        private List<ConfiguredSourceMemberIgnoreBase> _ignoredSourceMembers;
        private List<ConfiguredMemberIgnoreBase> _ignoredMembers;
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

        public ICollection<Type> AppliedConfigurationTypes => _appliedConfigurationTypes ??= new List<Type>();

        #region MappedObjectCachingSettings

        private List<MappedObjectCachingSetting> MappedObjectCachingSettings
            => _mappedObjectCachingSettings ??= new List<MappedObjectCachingSetting>();

        public void Add(MappedObjectCachingSetting setting)
        {
            ThrowIfConflictingItemExists(
                setting,
               _mappedObjectCachingSettings,
               (stn, conflicting) => conflicting.GetConflictMessage(stn));

            MappedObjectCachingSettings.AddThenSort(setting);
        }

        public MappedObjectCachingMode CacheMappedObjects(IQualifiedMemberContext context)
        {
            if (MappedObjectCachingSettings.None() || !context.TargetMember.IsComplex)
            {
                return MappedObjectCachingMode.AutoDetect;
            }

            var applicableSettings = _mappedObjectCachingSettings
                .FirstOrDefault(context, (ctx, tm) => tm.AppliesTo(ctx));

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
            => _mapToNullConditions ??= new List<MapToNullCondition>();

        public void Add(MapToNullCondition condition)
        {
            ThrowIfConflictingItemExists(
                condition,
               _mapToNullConditions,
               (cdn, conflicting) => cdn.GetConflictMessage());

            MapToNullConditions.AddThenSort(condition);
        }

        public Expression GetMapToNullConditionOrNull(IMemberMapperData mapperData)
            => _mapToNullConditions.FindMatch(mapperData)?.GetConditionOrNull(mapperData);

        #endregion

        #region NullCollectionSettings

        private List<NullCollectionsSetting> NullCollectionsSettings
            => _nullCollectionsSettings ??= new List<NullCollectionsSetting>();

        public void Add(NullCollectionsSetting setting) => NullCollectionsSettings.Add(setting);

        public bool MapToNullCollections(IQualifiedMemberContext context)
            => _nullCollectionsSettings?.Any(context, (ctx, s) => s.AppliesTo(ctx)) == true;

        #endregion

        #region EntityKeyMappingSettings

        private List<EntityKeyMappingSetting> EntityKeyMappingSettings
            => _entityKeyMappingSettings ??= new List<EntityKeyMappingSetting>();

        public void Add(EntityKeyMappingSetting setting)
        {
            ThrowIfConflictingKeyMappingSettingExists(setting);

            EntityKeyMappingSettings.AddThenSort(setting);
        }

        public bool MapEntityKeys(IQualifiedMemberContext context)
        {
            var applicableSetting = _entityKeyMappingSettings?
                .FirstOrDefault(context, (ctx, s) => s.AppliesTo(ctx))?
                .MapKeys;

            return (applicableSetting == true) ||
                   (context.RuleSet.Settings.AllowEntityKeyMapping && (applicableSetting != false));
        }

        #endregion

        #region ConfiguredDataSourceReversalSettings

        private List<DataSourceReversalSetting> DataSourceReversalSettings
            => _dataSourceReversalSettings ??= new List<DataSourceReversalSetting>();

        public void Add(DataSourceReversalSetting setting)
        {
            ThrowIfConflictingDataSourceReversalSettingExists(setting);

            DataSourceReversalSettings.AddThenSort(setting);
        }

        public void AddReverseDataSourceFor(ConfiguredDataSourceFactory dataSourceFactory)
            => AddReverse(dataSourceFactory, isAutoReversal: false);

        private void AddReverse(ConfiguredDataSourceFactory dataSourceFactory, bool isAutoReversal)
        {
            var reverseDataSourceFactory = dataSourceFactory.CreateReverseIfAppropriate(isAutoReversal);

            if (reverseDataSourceFactory != null)
            {
                DataSourceFactories.AddOrReplaceThenSort(reverseDataSourceFactory);
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
            => AutoDataSourceReversalEnabled(dataSourceFactory, dsf => dsf.ConfigInfo.ToMemberContext(dsf.TargetMember));

        public bool AutoDataSourceReversalEnabled(MappingConfigInfo configInfo)
            => AutoDataSourceReversalEnabled(configInfo, ci => ci.ToMemberContext());

        private bool AutoDataSourceReversalEnabled<T>(T dataItem, Func<T, IQualifiedMemberContext> memberContextFactory)
        {
            if (_dataSourceReversalSettings == null)
            {
                return false;
            }

            var memberContext = memberContextFactory.Invoke(dataItem);

            return _dataSourceReversalSettings
                .FirstOrDefault(memberContext, (mc, s) => s.AppliesTo(mc))?.Reverse == true;
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
            => _objectFactories ??= new List<ConfiguredObjectFactory>();

        public void Add(ConfiguredObjectFactory objectFactory)
        {
            ThrowIfConflictingItemExists(
                objectFactory,
               _objectFactories,
               (factory, conflictingFactory) => string.Format(
                   CultureInfo.InvariantCulture,
                   "{0} factory for type {1} has already been configured",
                   conflictingFactory.IsMappingFactory() ? "A mapping" : "An object",
                   factory.ObjectType.GetFriendlyName()));

            if (objectFactory.ObjectType.IsSimple())
            {
                HasSimpleTypeValueFactories = true;
            }

            if (objectFactory.IsMappingFactory())
            {
                HasMappingFactories = true;
            }

            ObjectFactories.AddOrReplaceThenSort(objectFactory);
        }

        public bool HasSimpleTypeValueFactories { get; private set; }

        public IEnumerable<ConfiguredObjectFactory> QueryObjectFactories(IQualifiedMemberContext context)
            => _objectFactories.FindMatches(context).Filter(of => !of.IsMappingFactory());

        public bool HasMappingFactories { get; private set; }

        public IEnumerable<ConfiguredObjectFactory> QueryMappingFactories(IQualifiedMemberContext context)
        {
            return HasMappingFactories
                ? _objectFactories.FindMatches(context).Filter(of => of.IsMappingFactory())
                : Enumerable<ConfiguredObjectFactory>.Empty;
        }

        #endregion

        public MemberIdentifierSet Identifiers => _identifiers ??= new MemberIdentifierSet(_mapperContext);

        #region SourceValueFilters

        public bool HasSourceValueFilters => _sourceValueFilters?.Any() == true;

        private List<ConfiguredSourceValueFilter> SourceValueFilters
            => _sourceValueFilters ??= new List<ConfiguredSourceValueFilter>();

        public void Add(ConfiguredSourceValueFilter sourceValueFilter)
        {
            ThrowIfConflictingItemExists(
                sourceValueFilter,
               _sourceValueFilters,
               (svf, conflicting) => svf.GetConflictMessage());

            SourceValueFilters.AddOrReplaceThenSort(sourceValueFilter);
        }

        public IList<ConfiguredSourceValueFilter> GetSourceValueFilters(IQualifiedMemberContext context, Type sourceValueType)
        {
            return HasSourceValueFilters
                ? _sourceValueFilters.FilterToArray(svf => svf.AppliesTo(sourceValueType, context))
                : Enumerable<ConfiguredSourceValueFilter>.EmptyArray;
        }

        #endregion

        #region MemberIgnores

        public bool HasSourceMemberIgnores => _ignoredSourceMembers?.Any() == true;

        private List<ConfiguredSourceMemberIgnoreBase> IgnoredSourceMembers
            => _ignoredSourceMembers ??= new List<ConfiguredSourceMemberIgnoreBase>();

        public void Add(ConfiguredSourceMemberIgnoreBase sourceMemberIgnore)
        {
            ThrowIfConflictingIgnoredSourceMemberExists(sourceMemberIgnore, (ism, cIsm) => ism.GetConflictMessage(cIsm));

            IgnoredSourceMembers.AddOrReplaceThenSort(sourceMemberIgnore);
        }

        public IList<ConfiguredSourceMemberIgnoreBase> GetRelevantSourceMemberIgnores(IQualifiedMemberContext context)
            => _ignoredSourceMembers.FindRelevantMatches(context);

        public ConfiguredSourceMemberIgnoreBase GetSourceMemberIgnoreOrNull(IQualifiedMemberContext context)
            => _ignoredSourceMembers.FindMatch(context);

        private List<ConfiguredMemberIgnoreBase> IgnoredMembers
            => _ignoredMembers ??= new List<ConfiguredMemberIgnoreBase>();

        public void Add(ConfiguredMemberIgnoreBase memberIgnore)
        {
            ThrowIfMemberIsUnmappable(memberIgnore);
            ThrowIfConflictingIgnoredMemberExists(memberIgnore, (im, cIm) => im.GetConflictMessage(cIm));
            ThrowIfConflictingDataSourceExists(memberIgnore, (im, cDsf) => im.GetConflictMessage(cDsf));

            IgnoredMembers.AddOrReplaceThenSort(memberIgnore);
        }

        public IList<ConfiguredMemberIgnoreBase> GetRelevantMemberIgnores(IQualifiedMemberContext context)
            => _ignoredMembers.FindRelevantMatches(context);

        #endregion

        #region EnumPairing

        private List<EnumMemberPair> EnumPairings => _enumPairings ??= new List<EnumMemberPair>();

        public void Add(EnumMemberPair enumPairing) => EnumPairings.Add(enumPairing);

        public IList<EnumMemberPair> GetEnumPairingsFor(Type sourceEnumType, Type targetEnumType)
            => _enumPairings?.FilterToArray(ep => ep.IsFor(sourceEnumType, targetEnumType)) ?? Enumerable<EnumMemberPair>.EmptyArray;

        #endregion

        public DictionarySettings Dictionaries => _dictionaries ??= new DictionarySettings(_mapperContext);

        #region DataSources

        private List<ConfiguredDataSourceFactory> DataSourceFactories
            => _dataSourceFactories ??= new List<ConfiguredDataSourceFactory>();

        public void Add(ConfiguredDataSourceFactory dataSourceFactory)
        {
            if (!dataSourceFactory.TargetMember.IsRoot)
            {
                ThrowIfConflictingIgnoredSourceMemberExists(dataSourceFactory, (dsf, cIsm) => cIsm.GetConflictMessage(dsf));
                ThrowIfConflictingIgnoredMemberExists(dataSourceFactory);
            }
                
            ThrowIfConflictingDataSourceExists(dataSourceFactory, (dsf, cDsf) => dsf.GetConflictMessage(cDsf));

            DataSourceFactories.AddOrReplaceThenSort(dataSourceFactory);

            if (dataSourceFactory.TargetMember.IsRoot)
            {
                HasToTargetDataSources = true;
                return;
            }

            if (AutoDataSourceReversalEnabled(dataSourceFactory))
            {
                AddReverse(dataSourceFactory, isAutoReversal: true);
            }
        }

        public ConfiguredDataSourceFactory GetDataSourceFactoryFor(MappingConfigInfo configInfo)
            => _dataSourceFactories.First(configInfo, (ci, dsf) => dsf.ConfigInfo == ci);

        public bool HasToTargetDataSources { get; private set; }

        public IList<ConfiguredDataSourceFactory> GetRelevantDataSourceFactories(IMemberMapperData mapperData)
            => _dataSourceFactories.FindRelevantMatches(mapperData);

        public IList<IConfiguredDataSource> GetDataSourcesForToTarget(IMemberMapperData mapperData, bool? sequential)
        {
            if (!HasToTargetDataSources)
            {
                return Enumerable<IConfiguredDataSource>.EmptyArray;
            }

            var toTargetDataSources = QueryDataSourceFactories(mapperData)
                .Filter(dsf => 
                    dsf.IsForToTargetDataSource && 
                   (dsf.IsSequential == sequential || !sequential.HasValue))
                .Project(mapperData, (md, dsf) => dsf.Create(md))
                .ToArray();

            return toTargetDataSources;
        }

        public IEnumerable<TFactory> QueryDataSourceFactories<TFactory>()
            where TFactory : ConfiguredDataSourceFactory
        {
            return _dataSourceFactories?.OfType<TFactory>() ?? Enumerable<TFactory>.Empty;
        }

        public IEnumerable<ConfiguredDataSourceFactory> QueryDataSourceFactories(IQualifiedMemberContext context)
            => _dataSourceFactories.FindMatches(context);

        #endregion

        #region MappingCallbacks

        private List<MappingCallbackFactory> MappingCallbackFactories
            => _mappingCallbackFactories ??= new List<MappingCallbackFactory>();

        public void Add(MappingCallbackFactory callbackFactory) => MappingCallbackFactories.Add(callbackFactory);

        public Expression GetCallbackOrNull(
            InvocationPosition position,
            IQualifiedMemberContext context,
            IMemberMapperData mapperData)
        {
            return _mappingCallbackFactories?.FirstOrDefault(f => f.AppliesTo(position, context))?.Create(mapperData);
        }

        private List<ObjectCreationCallbackFactory> CreationCallbackFactories
            => _creationCallbackFactories ??= new List<ObjectCreationCallbackFactory>();

        public void Add(ObjectCreationCallbackFactory callbackFactory) => CreationCallbackFactories.Add(callbackFactory);

        public Expression GetCreationCallbackOrNull(InvocationPosition position, IMemberMapperData mapperData)
            => _creationCallbackFactories?.FirstOrDefault(f => f.AppliesTo(position, mapperData))?.Create(mapperData);

        #endregion

        #region ExceptionCallbacks

        private List<ExceptionCallback> ExceptionCallbackFactories
            => _exceptionCallbackFactories ??= new List<ExceptionCallback>();

        public void Add(ExceptionCallback callback) => ExceptionCallbackFactories.Add(callback);

        public ExceptionCallback GetExceptionCallbackOrNull(IQualifiedMemberContext context)
            => _exceptionCallbackFactories.FindMatch(context);

        #endregion

        public DerivedTypePairSet DerivedTypes => _derivedTypes ??= new DerivedTypePairSet(_mapperContext);

        #region RecursionDepthSettings

        private List<RecursionDepthSettings> RecursionDepthSettings
            => _recursionDepthSettings ??= new List<RecursionDepthSettings>();

        public void Add(RecursionDepthSettings settings)
        {
            RecursionDepthSettings.Add(settings);
        }

        public bool ShortCircuitRecursion(IQualifiedMemberContext context)
        {
            if (_recursionDepthSettings == null)
            {
                return true;
            }

            return RecursionDepthSettings.FindMatch(context)?.IsBeyondDepth(context) != false;
        }

        #endregion

        #region Validation

        private void ThrowIfConflictingKeyMappingSettingExists(EntityKeyMappingSetting setting)
        {
            if ((_entityKeyMappingSettings == null) && !setting.MapKeys)
            {
                throw new MappingConfigurationException("Entity key mapping is disabled by default");
            }

            ThrowIfConflictingItemExists(
                setting,
               _entityKeyMappingSettings,
               (stn, conflicting) => conflicting.GetConflictMessage(stn));
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
               (stn, conflicting) => conflicting.GetConflictMessage(stn));
        }

        private void ThrowIfMemberIsUnmappable(ConfiguredMemberIgnoreBase memberIgnore)
        {
            if (memberIgnore.ConfigInfo.ToMemberContext().TargetMemberIsUnmappable(
                memberIgnore.TargetMember,
                QueryDataSourceFactories,
                this,
                out var reason))
            {
                throw new MappingConfigurationException(
                    $"{memberIgnore.TargetMember.GetPath()} will not be mapped and does not need to be ignored ({reason})");
            }
        }

        private void ThrowIfConflictingIgnoredSourceMemberExists<TConfiguredItem>(
            TConfiguredItem configuredItem,
            Func<TConfiguredItem, ConfiguredSourceMemberIgnoreBase, string> messageFactory)
            where TConfiguredItem : UserConfiguredItemBase
        {
            ThrowIfConflictingItemExists(
                configuredItem, 
                _ignoredSourceMembers, 
                messageFactory);
        }

        internal void ThrowIfConflictingIgnoredMemberExists<TConfiguredItem>(TConfiguredItem configuredItem)
            where TConfiguredItem : UserConfiguredItemBase
        {
            ThrowIfConflictingIgnoredMemberExists(configuredItem, (ci, im) => im.GetConflictMessage(ci));
        }

        private void ThrowIfConflictingIgnoredMemberExists<TConfiguredItem>(
            TConfiguredItem configuredItem,
            Func<TConfiguredItem, ConfiguredMemberIgnoreBase, string> messageFactory)
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
            _sourceValueFilters?.CloneItems().CopyTo(configurations.SourceValueFilters);
            _ignoredSourceMembers?.CloneItems().CopyTo(configurations.IgnoredSourceMembers);
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
            _sourceValueFilters?.Clear();
            _ignoredSourceMembers?.Clear();
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