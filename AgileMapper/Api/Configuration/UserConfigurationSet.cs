namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;
    using Members;
    using ObjectPopulation;

    internal class UserConfigurationSet
    {
        private readonly ICollection<ConfiguredIgnoredMember> _ignoredMembers;
        private readonly ICollection<ConfiguredDataSourceFactory> _dataSourceFactories;
        private readonly ICollection<ObjectCreationCallbackFactory> _creationCallbackFactories;
        private readonly ICollection<ExceptionCallbackFactory> _exceptionCallbackFactories;
        private readonly ICollection<DerivedTypePair> _typePairs;

        public UserConfigurationSet(MemberFinder memberFinder)
        {
            Identifiers = new MemberIdentifierSet(memberFinder);
            _ignoredMembers = new List<ConfiguredIgnoredMember>();
            _dataSourceFactories = new List<ConfiguredDataSourceFactory>();
            _creationCallbackFactories = new List<ObjectCreationCallbackFactory>();
            _exceptionCallbackFactories = new List<ExceptionCallbackFactory>();
            _typePairs = new List<DerivedTypePair>();
        }

        public MemberIdentifierSet Identifiers { get; }

        public void Add(ConfiguredIgnoredMember ignoredMember)
        {
            _ignoredMembers.Add(ignoredMember);
        }

        public bool IsIgnored(IMemberMappingContext context, out Expression ignoreCondition)
        {
            var matchingIgnoredMember = FindMatchingItem(_ignoredMembers, context);

            ignoreCondition = matchingIgnoredMember?.GetCondition(context);

            return matchingIgnoredMember != null;
        }

        public void Add(ConfiguredDataSourceFactory dataSourceFactory)
        {
            _dataSourceFactories.Add(dataSourceFactory);
        }

        public IEnumerable<IConfiguredDataSource> GetDataSources(IMemberMappingContext context)
        {
            var matchingDataSources = _dataSourceFactories
                .Where(dsf => dsf.AppliesTo(context))
                .Select((dsf, i) => dsf.Create(i, context))
                .ToArray();

            return matchingDataSources;
        }

        public void Add(ObjectCreationCallbackFactory callbackFactory)
        {
            _creationCallbackFactories.Add(callbackFactory);
        }

        public bool HasCreationCallback(IMemberMappingContext context, out ObjectCreationCallback callback)
        {
            var matchingCallbackFactory = FindMatchingItem(_creationCallbackFactories, context);

            callback = matchingCallbackFactory?.Create(context);

            return matchingCallbackFactory != null;
        }

        public void Add(ExceptionCallbackFactory callbackFactory)
        {
            _exceptionCallbackFactories.Add(callbackFactory);
        }

        public ExceptionCallback GetExceptionCallbackOrNull(IMemberMappingContext context)
        {
            var matchingCallbackFactory = FindMatchingItem(_exceptionCallbackFactories, context);

            return matchingCallbackFactory?.Create(context);
        }

        public void Add(DerivedTypePair typePair)
        {
            _typePairs.Add(typePair);
        }

        public Type GetDerivedTypeOrNull(IMappingData data)
        {
            var matchingTypePair = FindMatchingItem(_typePairs, data);

            return matchingTypePair?.DerivedTargetType;
        }

        private static TItem FindMatchingItem<TItem>(IEnumerable<TItem> items, IMappingData data)
            where TItem : UserConfiguredItemBase
            => items.FirstOrDefault(im => im.AppliesTo(data));
    }
}