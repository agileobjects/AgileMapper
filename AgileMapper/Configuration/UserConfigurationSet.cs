namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;
    using Members;
    using ObjectPopulation;

    internal class UserConfigurationSet
    {
        private readonly List<ObjectTrackingMode> _trackingModeSettings;
        private readonly List<MapToNullCondition> _mapToNullConditions;
        private readonly List<NullCollectionsSetting> _nullCollectionSettings;
        private readonly List<ConfiguredObjectFactory> _objectFactories;
        private readonly List<ConfiguredIgnoredMember> _ignoredMembers;
        private readonly List<EnumMemberPair> _enumPairings;
        private readonly List<ConfiguredDataSourceFactory> _dataSourceFactories;
        private readonly List<MappingCallbackFactory> _mappingCallbackFactories;
        private readonly List<ObjectCreationCallbackFactory> _creationCallbackFactories;
        private readonly List<ExceptionCallback> _exceptionCallbackFactories;

        public UserConfigurationSet(MapperContext mapperContext)
        {
            _trackingModeSettings = new List<ObjectTrackingMode>();
            _mapToNullConditions = new List<MapToNullCondition>();
            _nullCollectionSettings = new List<NullCollectionsSetting>();
            _objectFactories = new List<ConfiguredObjectFactory>();
            Identifiers = new MemberIdentifierSet();
            _ignoredMembers = new List<ConfiguredIgnoredMember>();
            _enumPairings = new List<EnumMemberPair>();
            Dictionaries = new DictionarySettings(mapperContext);
            _dataSourceFactories = new List<ConfiguredDataSourceFactory>();
            _mappingCallbackFactories = new List<MappingCallbackFactory>();
            _creationCallbackFactories = new List<ObjectCreationCallbackFactory>();
            _exceptionCallbackFactories = new List<ExceptionCallback>();
            DerivedTypes = new DerivedTypePairSet();
        }

        #region Tracking Modes

        public void Add(ObjectTrackingMode trackingMode) => _trackingModeSettings.Add(trackingMode);

        public bool DisableObjectTracking(IBasicMapperData basicData)
        {
            if (_trackingModeSettings.None())
            {
                // Object tracking switched off by default:
                return true;
            }

            return _trackingModeSettings.All(tm => !tm.AppliesTo(basicData));
        }

        #endregion

        #region MapToNullConditions

        public void Add(MapToNullCondition condition)
        {
            ThrowIfConflictingItemExists(condition, _mapToNullConditions, (c, cC) => c.GetConflictMessage());

            _mapToNullConditions.Add(condition);
            _mapToNullConditions.Sort();
        }

        public Expression GetMapToNullConditionOrNull(IMemberMapperData mapperData)
            => _mapToNullConditions.FindMatch(mapperData)?.GetConditionOrNull(mapperData);

        #endregion

        #region Null Collections

        public void Add(NullCollectionsSetting setting) => _nullCollectionSettings.Add(setting);

        public bool MapToNullCollections(IBasicMapperData basicData)
            => _nullCollectionSettings.Any(s => s.AppliesTo(basicData));

        #endregion

        #region ObjectFactories

        public void Add(ConfiguredObjectFactory objectFactory)
        {
            ThrowIfConflictingItemExists(
                objectFactory,
                _objectFactories,
                (of1, of2) => $"An object factory for type {of1.ObjectTypeName} has already been configured");

            _objectFactories.AddSortFilter(objectFactory);
        }

        public IEnumerable<ConfiguredObjectFactory> GetObjectFactories(IBasicMapperData mapperData)
            => FindMatches(_objectFactories, mapperData).ToArray();

        #endregion

        public MemberIdentifierSet Identifiers { get; }

        #region Ignored Members

        public void Add(ConfiguredIgnoredMember ignoredMember)
        {
            ThrowIfConflictingIgnoredMemberExists(ignoredMember, (im, cIm) => im.GetConflictMessage(cIm));
            ThrowIfConflictingDataSourceExists(ignoredMember, (im, cDsf) => im.GetConflictMessage(cDsf));

            _ignoredMembers.Add(ignoredMember);
        }

        public ConfiguredIgnoredMember GetMemberIgnoreOrNull(IBasicMapperData mapperData)
            => _ignoredMembers.FindMatch(mapperData);

        #endregion

        #region Enum Pairing

        public void Add(EnumMemberPair enumPairing)
        {
            _enumPairings.Add(enumPairing);
        }

        public IEnumerable<EnumMemberPair> GetEnumPairingsFor(Type sourceType, Type targetType)
            => _enumPairings.Where(ep => ep.IsFor(sourceType, targetType));

        #endregion

        public DictionarySettings Dictionaries { get; }

        #region DataSources

        public IEnumerable<ConfiguredDataSourceFactory> DataSourceFactories => _dataSourceFactories;

        public void Add(ConfiguredDataSourceFactory dataSourceFactory)
        {
            ThrowIfConflictingIgnoredMemberExists(dataSourceFactory);
            ThrowIfConflictingDataSourceExists(dataSourceFactory, (dsf, cDsf) => dsf.GetConflictMessage(cDsf));

            _dataSourceFactories.AddSortFilter(dataSourceFactory);
        }

        public IEnumerable<IConfiguredDataSource> GetDataSources(IMemberMapperData mapperData)
            => FindMatches(_dataSourceFactories, mapperData).Select(dsf => dsf.Create(mapperData)).ToArray();

        #endregion

        #region Callbacks

        public void Add(MappingCallbackFactory callbackFactory) => _mappingCallbackFactories.Add(callbackFactory);

        public Expression GetCallbackOrNull(
            CallbackPosition position,
            IBasicMapperData basicData,
            IMemberMapperData mapperData)
        {
            if (_mappingCallbackFactories.None())
            {
                return null;
            }

            return _mappingCallbackFactories
                .FirstOrDefault(f => f.AppliesTo(position, basicData))?.Create(mapperData);
        }

        public void Add(ObjectCreationCallbackFactory callbackFactory) => _creationCallbackFactories.Add(callbackFactory);

        public Expression GetCreationCallbackOrNull(CallbackPosition position, IMemberMapperData mapperData)
        {
            if (_creationCallbackFactories.None())
            {
                return null;
            }

            return _creationCallbackFactories
                .FirstOrDefault(f => f.AppliesTo(position, mapperData))?
                .Create(mapperData);
        }

        #endregion

        #region ExceptionCallbacks

        public void Add(ExceptionCallback callback) => _exceptionCallbackFactories.Add(callback);

        public Expression GetExceptionCallbackOrNull(IBasicMapperData mapperData)
            => _exceptionCallbackFactories.FindMatch(mapperData)?.Callback;

        #endregion

        public DerivedTypePairSet DerivedTypes { get; }

        private static IEnumerable<TItem> FindMatches<TItem>(IEnumerable<TItem> items, IBasicMapperData mapperData)
            where TItem : UserConfiguredItemBase
            => items.Where(item => item.AppliesTo(mapperData)).OrderBy(im => im);

        #region Conflict Handling

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
            IEnumerable<TExistingItem> existingItems,
            Func<TConfiguredItem, TExistingItem, string> messageFactory)
            where TConfiguredItem : UserConfiguredItemBase
            where TExistingItem : UserConfiguredItemBase
        {
            var conflictingItem = existingItems
                .FirstOrDefault(dsf => dsf.ConflictsWith(configuredItem));

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
            configurations._trackingModeSettings.AddRange(_trackingModeSettings);
            configurations._mapToNullConditions.AddRange(_mapToNullConditions);
            configurations._nullCollectionSettings.AddRange(_nullCollectionSettings);
            configurations._objectFactories.AddRange(_objectFactories.SelectClones());
            configurations._ignoredMembers.AddRange(_ignoredMembers.SelectClones());
            configurations._enumPairings.AddRange(_enumPairings);
            Dictionaries.CloneTo(configurations.Dictionaries);
            configurations._dataSourceFactories.AddRange(_dataSourceFactories.SelectClones());
            configurations._mappingCallbackFactories.AddRange(_mappingCallbackFactories);
            configurations._creationCallbackFactories.AddRange(_creationCallbackFactories);
            configurations._exceptionCallbackFactories.AddRange(_exceptionCallbackFactories);
            DerivedTypes.CloneTo(configurations.DerivedTypes);
        }

        public void Reset()
        {
            _objectFactories.Clear();
            _ignoredMembers.Clear();
            _dataSourceFactories.Clear();
            _mappingCallbackFactories.Clear();
            _creationCallbackFactories.Clear();
            _exceptionCallbackFactories.Clear();
            DerivedTypes.Reset();
        }
    }
}