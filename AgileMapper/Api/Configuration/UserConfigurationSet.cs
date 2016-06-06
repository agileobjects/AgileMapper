namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataSources;
    using Members;
    using ObjectPopulation;

    internal class UserConfigurationSet
    {
        private readonly ICollection<ConfiguredObjectFactory> _objectFactories;
        private readonly ICollection<ConfiguredIgnoredMember> _ignoredMembers;
        private readonly ICollection<ConfiguredDataSourceFactory> _dataSourceFactories;
        private readonly ICollection<ObjectCreationCallbackFactory> _creationCallbackFactories;
        private readonly ICollection<ExceptionCallbackFactory> _exceptionCallbackFactories;
        private readonly ICollection<DerivedTypePair> _typePairs;

        public UserConfigurationSet()
        {
            _objectFactories = new List<ConfiguredObjectFactory>();
            Identifiers = new MemberIdentifierSet();
            _ignoredMembers = new List<ConfiguredIgnoredMember>();
            _dataSourceFactories = new List<ConfiguredDataSourceFactory>();
            _creationCallbackFactories = new List<ObjectCreationCallbackFactory>();
            _exceptionCallbackFactories = new List<ExceptionCallbackFactory>();
            _typePairs = new List<DerivedTypePair>();
        }

        public MemberIdentifierSet Identifiers { get; }

        #region ObjectFactories

        public void Add(ConfiguredObjectFactory objectFactory) => _objectFactories.Add(objectFactory);

        public IEnumerable<ConfiguredObjectFactory> GetObjectFactories(IMemberMappingContext context)
            => FindMatches(_objectFactories, context).ToArray();

        #endregion

        #region Ignored Members

        public void Add(ConfiguredIgnoredMember ignoredMember)
        {
            var conflictingIgnoredMember = _ignoredMembers
                .FirstOrDefault(im => im.ConflictsWith(ignoredMember));

            if (conflictingIgnoredMember != null)
            {
                throw new MappingConfigurationException(
                    "Member " + ignoredMember.TargetMemberPath + " is already ignored");
            }

            _ignoredMembers.Add(ignoredMember);
        }

        public ConfiguredIgnoredMember GetMemberIgnoreOrNull(IMemberMappingContext context)
            => FindMatch(_ignoredMembers, context);

        #endregion

        #region DataSources

        public void Add(ConfiguredDataSourceFactory dataSourceFactory) => _dataSourceFactories.Add(dataSourceFactory);

        public IEnumerable<IConfiguredDataSource> GetDataSources(IMemberMappingContext context)
            => FindMatches(_dataSourceFactories, context).Select((dsf, i) => dsf.Create(i, context)).ToArray();

        #endregion

        #region ObjectCreationCallbacks

        public void Add(ObjectCreationCallbackFactory callbackFactory) => _creationCallbackFactories.Add(callbackFactory);

        public ObjectCreationCallback GetCreationCallbackOrNull(IMemberMappingContext context)
            => FindMatch(_creationCallbackFactories, context)?.Create(context);

        #endregion

        #region ExceptionCallbacks

        public void Add(ExceptionCallbackFactory callbackFactory) => _exceptionCallbackFactories.Add(callbackFactory);

        public ExceptionCallback GetExceptionCallbackOrNull(IMemberMappingContext context)
            => FindMatch(_exceptionCallbackFactories, context)?.Create(context);

        #endregion

        #region DerivedTypePairs

        public void Add(DerivedTypePair typePair) => _typePairs.Add(typePair);

        public Type GetDerivedTypeOrNull(IMappingData data)
            => FindMatch(_typePairs, data)?.DerivedTargetType;

        #endregion

        private static TItem FindMatch<TItem>(IEnumerable<TItem> items, IMappingData data)
            where TItem : UserConfiguredItemBase
            => items.FirstOrDefault(im => im.AppliesTo(data));

        private static IEnumerable<TItem> FindMatches<TItem>(IEnumerable<TItem> items, IMappingData data)
            where TItem : UserConfiguredItemBase
            => items.Where(im => im.AppliesTo(data)).OrderByDescending(im => im.HasConfiguredCondition);

        public void Reset()
        {
            _objectFactories.Clear();
            _ignoredMembers.Clear();
            _dataSourceFactories.Clear();
            _creationCallbackFactories.Clear();
            _exceptionCallbackFactories.Clear();
            _typePairs.Clear();
        }
    }
}