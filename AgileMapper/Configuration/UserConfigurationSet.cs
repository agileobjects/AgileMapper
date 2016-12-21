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
        private readonly ICollection<ObjectTrackingMode> _trackingModeSettings;
        private readonly ICollection<NullCollectionsSetting> _nullCollectionSettings;
        private readonly ICollection<ConfiguredObjectFactory> _objectFactories;
        private readonly ICollection<ConfiguredIgnoredMember> _ignoredMembers;
        private readonly ICollection<EnumMemberPair> _enumPairings;
        private readonly ICollection<ConfiguredDataSourceFactory> _dataSourceFactories;
        private readonly ICollection<MappingCallbackFactory> _mappingCallbackFactories;
        private readonly ICollection<ObjectCreationCallbackFactory> _creationCallbackFactories;
        private readonly ICollection<ExceptionCallback> _exceptionCallbackFactories;

        public UserConfigurationSet(MapperContext mapperContext)
        {
            _trackingModeSettings = new List<ObjectTrackingMode>();
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

        #region Null Collections

        public void Add(NullCollectionsSetting setting) => _nullCollectionSettings.Add(setting);

        public bool MapToNullCollections(IBasicMapperData basicData)
            => _nullCollectionSettings.Any(s => s.AppliesTo(basicData));

        #endregion

        #region ObjectFactories

        public void Add(ConfiguredObjectFactory objectFactory) => _objectFactories.Add(objectFactory);

        public IEnumerable<ConfiguredObjectFactory> GetObjectFactories(IBasicMapperData mapperData)
            => FindMatches(_objectFactories, mapperData).ToArray();

        #endregion

        public MemberIdentifierSet Identifiers { get; }

        #region Ignored Members

        public void Add(ConfiguredIgnoredMember ignoredMember)
        {
            ThrowIfDuplicateIgnoredMemberExists(ignoredMember);

            ThrowIfConflictingDataSourceExists(
                ignoredMember,
                im => "Ignored member " + im.TargetMember.GetPath() + " has a configured data source");

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

        public void Add(ConfiguredDataSourceFactory dataSourceFactory)
        {
            ThrowIfConflictingIgnoredMemberExists(dataSourceFactory);

            ThrowIfConflictingDataSourceExists(
                dataSourceFactory,
                dsf => dsf.TargetMember.GetPath() + " already has a configured data source");

            _dataSourceFactories.Add(dataSourceFactory);
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

            return _creationCallbackFactories.FirstOrDefault(f => f.AppliesTo(position, mapperData))?.Create(mapperData);
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
            => items.Where(im => im.AppliesTo(mapperData)).OrderBy(im => im, UserConfiguredItemBase.SpecificityComparer);

        #region Conflict Handling

        private void ThrowIfDuplicateIgnoredMemberExists(ConfiguredIgnoredMember ignoredMember)
        {
            ThrowIfConflictingIgnoredMemberExists(
                ignoredMember,
                im => "Member " + im.TargetMember.GetPath() + " is already ignored");
        }

        internal void ThrowIfConflictingIgnoredMemberExists<TConfiguredItem>(TConfiguredItem configuredItem)
            where TConfiguredItem : UserConfiguredItemBase
        {
            ThrowIfConflictingIgnoredMemberExists(
                configuredItem,
                ci => "Member " + ci.TargetMember.GetPath() + " has been ignored");
        }

        private void ThrowIfConflictingIgnoredMemberExists<TConfiguredItem>(
            TConfiguredItem configuredItem,
            Func<TConfiguredItem, string> messageFactory)
            where TConfiguredItem : UserConfiguredItemBase
        {
            var conflictingIgnoredMember = _ignoredMembers
                .FirstOrDefault(im => im.ConflictsWith(configuredItem));

            if (conflictingIgnoredMember != null)
            {
                throw new MappingConfigurationException(messageFactory.Invoke(configuredItem));
            }
        }

        private void ThrowIfConflictingDataSourceExists<TConfiguredItem>(
            TConfiguredItem configuredItem,
            Func<TConfiguredItem, string> messageFactory)
            where TConfiguredItem : UserConfiguredItemBase
        {
            var conflictingDataSource = _dataSourceFactories
                .FirstOrDefault(dsf => dsf.ConflictsWith(configuredItem));

            if (conflictingDataSource != null)
            {
                throw new MappingConfigurationException(messageFactory.Invoke(configuredItem));
            }
        }

        #endregion

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