namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;

    internal class DictionarySettings
    {
        private readonly List<CustomDictionaryKey> _configuredKeys;

        public DictionarySettings()
        {
            _configuredKeys = new List<CustomDictionaryKey>();
        }

        public Expression GetKeyOrNull(IBasicMapperData mapperData)
            => _configuredKeys.FirstOrDefault(k => k.AppliesTo(mapperData))?.KeyValue;

        public void Add(CustomDictionaryKey configuredKey)
        {
            _configuredKeys.Add(configuredKey);
        }
    }
}
