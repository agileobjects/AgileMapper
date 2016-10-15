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
        private readonly ICollection<ConfiguredObjectFactory> _objectFactories;
        private readonly ICollection<ConfiguredIgnoredMember> _ignoredMembers;
        private readonly ICollection<ConfiguredDataSourceFactory> _dataSourceFactories;
        private readonly ICollection<MappingCallbackFactory> _mappingCallbackFactories;
        private readonly ICollection<ObjectCreationCallbackFactory> _creationCallbackFactories;
        private readonly ICollection<ExceptionCallback> _exceptionCallbackFactories;

        public UserConfigurationSet()
        {
            _objectFactories = new List<ConfiguredObjectFactory>();
            Identifiers = new MemberIdentifierSet();
            _ignoredMembers = new List<ConfiguredIgnoredMember>();
            _dataSourceFactories = new List<ConfiguredDataSourceFactory>();
            _mappingCallbackFactories = new List<MappingCallbackFactory>();
            _creationCallbackFactories = new List<ObjectCreationCallbackFactory>();
            _exceptionCallbackFactories = new List<ExceptionCallback>();
            DerivedTypePairs = new DerivedTypePairSet();
        }

        #region ObjectFactories

        public void Add(ConfiguredObjectFactory objectFactory) => _objectFactories.Add(objectFactory);

        public IEnumerable<ConfiguredObjectFactory> GetObjectFactories(MemberMapperData mapperData)
            => FindMatches(_objectFactories, mapperData).ToArray();

        #endregion

        public MemberIdentifierSet Identifiers { get; }

        #region Ignored Members

        public void Add(ConfiguredIgnoredMember ignoredMember)
        {
            ThrowIfConflictingIgnoredMemberExists(
                ignoredMember,
                () => "Member " + ignoredMember.TargetMember.GetPath() + " is already ignored");

            ThrowIfConflictingDataSourceExists(
                ignoredMember,
                () => "Ignored member " + ignoredMember.TargetMember.GetPath() + " has a configured data source");

            _ignoredMembers.Add(ignoredMember);
        }

        public ConfiguredIgnoredMember GetMemberIgnoreOrNull(IBasicMapperData mapperData)
            => FindMatch(_ignoredMembers, mapperData);

        #endregion

        #region DataSources

        public void Add(ConfiguredDataSourceFactory dataSourceFactory)
        {
            ThrowIfConflictingIgnoredMemberExists(
                dataSourceFactory,
                () => "Member " + dataSourceFactory.TargetMember.GetPath() + " has been ignored");

            ThrowIfConflictingDataSourceExists(
                dataSourceFactory,
                () => dataSourceFactory.TargetMember.GetPath() + " already has a configured data source");

            _dataSourceFactories.Add(dataSourceFactory);
        }

        public IEnumerable<IConfiguredDataSource> GetDataSources(MemberMapperData mapperData)
            => FindMatches(_dataSourceFactories, mapperData).Select((dsf, i) => dsf.Create(i, mapperData)).ToArray();

        #endregion

        #region Callbacks

        public void Add(MappingCallbackFactory callbackFactory) => _mappingCallbackFactories.Add(callbackFactory);

        public Expression GetCallbackOrNull(
            CallbackPosition position,
            IBasicMapperData basicData,
            MemberMapperData mapperData)
        {
            if (_mappingCallbackFactories.None())
            {
                return null;
            }

            return _mappingCallbackFactories
                .FirstOrDefault(f => f.AppliesTo(position, basicData))?.Create(mapperData);
        }

        public void Add(ObjectCreationCallbackFactory callbackFactory) => _creationCallbackFactories.Add(callbackFactory);

        public Expression GetCreationCallbackOrNull(CallbackPosition position, MemberMapperData mapperData)
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

        public Expression GetExceptionCallbackOrNull(MemberMapperData mapperData)
            => FindMatch(_exceptionCallbackFactories, mapperData)?.Callback;

        #endregion

        public DerivedTypePairSet DerivedTypePairs { get; }

        private static TItem FindMatch<TItem>(IEnumerable<TItem> items, IBasicMapperData mapperData)
            where TItem : UserConfiguredItemBase
            => items.FirstOrDefault(im => im.AppliesTo(mapperData));

        private static IEnumerable<TItem> FindMatches<TItem>(IEnumerable<TItem> items, IBasicMapperData mapperData)
            where TItem : UserConfiguredItemBase
            => items.Where(im => im.AppliesTo(mapperData)).OrderBy(im => im, UserConfiguredItemBase.SpecificityComparer);

        #region Conflict Handling

        private void ThrowIfConflictingIgnoredMemberExists(
            UserConfiguredItemBase configuredItem,
            Func<string> messageFactory)
        {
            var conflictingIgnoredMember = _ignoredMembers
                .FirstOrDefault(im => im.ConflictsWith(configuredItem));

            if (conflictingIgnoredMember != null)
            {
                throw new MappingConfigurationException(messageFactory.Invoke());
            }
        }

        private void ThrowIfConflictingDataSourceExists(
            UserConfiguredItemBase configuredItem,
            Func<string> messageFactory)
        {
            var conflictingDataSource = _dataSourceFactories
                .FirstOrDefault(dsf => dsf.ConflictsWith(configuredItem));

            if (conflictingDataSource != null)
            {
                throw new MappingConfigurationException(messageFactory.Invoke());
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
            DerivedTypePairs.Reset();
        }
    }
}