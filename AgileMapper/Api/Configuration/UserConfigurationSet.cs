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
        private readonly ICollection<ConfiguredIgnoredMember> _ignoredMembers;
        private readonly ICollection<ConfiguredDataSourceFactory> _dataSourceFactories;
        private readonly ICollection<ObjectCreationCallbackFactory> _creationCallbackFactories;
        private readonly ICollection<ExceptionCallbackFactory> _exceptionCallbackFactories;
        private readonly ICollection<DerivedTypePair> _typePairs;

        public UserConfigurationSet()
        {
            Identifiers = new MemberIdentifierSet();
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

        public ConfiguredIgnoredMember GetMemberIgnoreOrNull(IMemberMappingContext context)
            => FindMatch(_ignoredMembers, context);

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

        public ObjectCreationCallback GetCreationCallbackOrNull(IMemberMappingContext context)
            => FindMatch(_creationCallbackFactories, context)?.Create(context);

        public void Add(ExceptionCallbackFactory callbackFactory)
        {
            _exceptionCallbackFactories.Add(callbackFactory);
        }

        public ExceptionCallback GetExceptionCallbackOrNull(IMemberMappingContext context)
            => FindMatch(_exceptionCallbackFactories, context)?.Create(context);

        public void Add(DerivedTypePair typePair)
        {
            _typePairs.Add(typePair);
        }

        public Type GetDerivedTypeOrNull(IMappingData data)
            => FindMatch(_typePairs, data)?.DerivedTargetType;

        private static TItem FindMatch<TItem>(IEnumerable<TItem> items, IMappingData data)
            where TItem : UserConfiguredItemBase
            => items.FirstOrDefault(im => im.AppliesTo(data));
    }
}