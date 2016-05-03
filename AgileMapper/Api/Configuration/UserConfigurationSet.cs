namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;

    internal class UserConfigurationSet
    {
        private readonly ICollection<ConfiguredIgnoredMember> _ignoredMembers;
        private readonly ICollection<ConfiguredDataSourceFactory> _dataSourceFactories;

        public UserConfigurationSet()
        {
            _ignoredMembers = new List<ConfiguredIgnoredMember>();
            _dataSourceFactories = new List<ConfiguredDataSourceFactory>();
        }

        public void Add(ConfiguredIgnoredMember ignoredMember)
        {
            _ignoredMembers.Add(ignoredMember);
        }

        public void Add(ConfiguredDataSourceFactory dataSourceFactory)
        {
            _dataSourceFactories.Add(dataSourceFactory);
        }

        public bool IsIgnored(IConfigurationContext context, out Expression ignoreCondition)
        {
            var matchingIgnoredMember = _ignoredMembers.FirstOrDefault(im => im.AppliesTo(context));

            ignoreCondition = matchingIgnoredMember?.GetCondition(context);

            return matchingIgnoredMember != null;
        }

        public IDataSource GetDataSourceOrNull(IConfigurationContext context)
        {
            var matchingDataSourceFactory = _dataSourceFactories
                .FirstOrDefault(ds => ds.AppliesTo(context));

            return matchingDataSourceFactory?.Create(context);
        }
    }
}