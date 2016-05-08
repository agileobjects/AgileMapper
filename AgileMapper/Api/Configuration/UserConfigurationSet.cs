namespace AgileObjects.AgileMapper.Api.Configuration
{
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
        private readonly ICollection<ObjectCreationCallback> _creationCallbacks;

        public UserConfigurationSet()
        {
            _ignoredMembers = new List<ConfiguredIgnoredMember>();
            _dataSourceFactories = new List<ConfiguredDataSourceFactory>();
            _creationCallbacks = new List<ObjectCreationCallback>();
        }

        public void Add(ConfiguredIgnoredMember ignoredMember)
        {
            _ignoredMembers.Add(ignoredMember);
        }

        public bool IsIgnored(IMemberMappingContext context, out Expression ignoreCondition)
        {
            var matchingIgnoredMember = _ignoredMembers.FirstOrDefault(im => im.AppliesTo(context));

            ignoreCondition = matchingIgnoredMember?.GetCondition(context);

            return matchingIgnoredMember != null;
        }

        public void Add(ConfiguredDataSourceFactory dataSourceFactory)
        {
            _dataSourceFactories.Add(dataSourceFactory);
        }

        public IDataSource GetDataSourceOrNull(IMemberMappingContext context)
        {
            var matchingDataSourceFactory = _dataSourceFactories
                .FirstOrDefault(ds => ds.AppliesTo(context));

            return matchingDataSourceFactory?.Create(context);
        }

        public void Add(ObjectCreationCallback callback)
        {
            _creationCallbacks.Add(callback);
        }

        public bool HasCreationCallback(IMemberMappingContext context, out Expression callback)
        {
            var matchingCallback = _creationCallbacks.FirstOrDefault(im => im.AppliesTo(context));

            callback = matchingCallback?.GetCallback(context);

            return matchingCallback != null;
        }
    }
}