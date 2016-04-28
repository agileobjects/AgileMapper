namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using DataSources;

    internal class UserConfigurationSet
    {
        //private readonly ICollection<ConfiguredIgnoredMember> _ignoredMembers;
        private readonly ICollection<ConfiguredDataSource> _dataSources;

        public UserConfigurationSet()
        {
            //_ignoredMembers = new List<ConfiguredIgnoredMember>();
            _dataSources = new List<ConfiguredDataSource>();
        }

        //public void Add(ConfiguredIgnoredMember ignoredMember)
        //{
        //    _ignoredMembers.Add(ignoredMember);
        //}

        public void Add(ConfiguredDataSource dataSource)
        {
            _dataSources.Add(dataSource);
        }

        //public bool IsIgnored(IConfigurationContext context)
        //{
        //    return _ignoredMembers.Any(im => im.AppliesTo(context));
        //}

        public IDataSource GetConfiguredDataSourceOrNull(IConfigurationContext context)
        {
            var matchingDataSource = _dataSources
                .FirstOrDefault(ds => ds.AppliesTo(context));

            return matchingDataSource;
        }
    }
}