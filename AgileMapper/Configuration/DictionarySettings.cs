namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;

    internal class DictionarySettings : ConfigurationSetBase
    {
        private readonly List<CustomDictionaryKey> _configuredFullKeys;
        private readonly List<CustomDictionaryKey> _configuredMemberKeys;
        private readonly List<JoiningNameFactory> _joiningNameFactories;

        public DictionarySettings(MapperContext mapperContext)
        {
            _configuredFullKeys = new List<CustomDictionaryKey>();
            _configuredMemberKeys = new List<CustomDictionaryKey>();

            _joiningNameFactories = new List<JoiningNameFactory>
            {
                JoiningNameFactory.Dotted(mapperContext)
            };
        }

        public void AddFullKey(CustomDictionaryKey configuredKey)
        {
            _configuredFullKeys.Add(configuredKey);
        }

        public Expression GetFullKeyOrNull(IBasicMapperData mapperData)
        {
            var matchingKey = FindMatch(_configuredFullKeys, mapperData);

            return (matchingKey != null) ? Expression.Constant(matchingKey.Key, typeof(string)) : null;
        }

        public void AddMemberKey(CustomDictionaryKey customKey)
        {
            _configuredMemberKeys.Add(customKey);
        }

        public string GetMemberKeyOrNull(IBasicMapperData mapperData)
            => FindMatch(_configuredMemberKeys, mapperData)?.Key;

        public void Add(JoiningNameFactory joiningNameFactory)
        {
            _joiningNameFactories.Insert(0, joiningNameFactory);
        }

        public Expression GetJoiningName(string memberName, IMemberMapperData mapperData)
            => FindMatch(_joiningNameFactories, mapperData).GetJoiningName(memberName, mapperData);
    }
}
