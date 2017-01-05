namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class DictionarySettings
    {
        private readonly List<CustomDictionaryKey> _configuredFullKeys;
        private readonly List<CustomDictionaryKey> _configuredMemberKeys;
        private readonly List<JoiningNameFactory> _joiningNameFactories;
        private readonly List<ElementKeyPartFactory> _elementKeyPartFactories;

        public DictionarySettings(MapperContext mapperContext)
        {
            _configuredFullKeys = new List<CustomDictionaryKey>();
            _configuredMemberKeys = new List<CustomDictionaryKey>();

            _joiningNameFactories = new List<JoiningNameFactory>
            {
                JoiningNameFactory.Dotted(mapperContext)
            };

            _elementKeyPartFactories = new List<ElementKeyPartFactory>
            {
                ElementKeyPartFactory.SquareBracketedIndex(mapperContext)
            };
        }

        public void AddFullKey(CustomDictionaryKey configuredKey)
        {
            _configuredFullKeys.Add(configuredKey);
        }

        public Expression GetFullKeyOrNull(IBasicMapperData mapperData)
        {
            var matchingKey = FindKeyOrNull(
                _configuredFullKeys,
                mapperData.TargetMember.LeafMember,
                mapperData);

            return matchingKey?.Key.ToConstantExpression();
        }

        public void AddMemberKey(CustomDictionaryKey customKey)
        {
            _configuredMemberKeys.Add(customKey);
        }

        public string GetMemberKeyOrNull(Member member, IBasicMapperData mapperData)
            => FindKeyOrNull(_configuredMemberKeys, member, mapperData)?.Key;

        private static CustomDictionaryKey FindKeyOrNull(
            IEnumerable<CustomDictionaryKey> keys,
            Member member,
            IBasicMapperData mapperData)
            => keys.FirstOrDefault(k => k.AppliesTo(member, mapperData));

        public void Add(JoiningNameFactory joiningNameFactory)
        {
            ThrowIfConflictingJoiningNameFactoryExists(joiningNameFactory);

            _joiningNameFactories.Insert(0, joiningNameFactory);
        }

        private void ThrowIfConflictingJoiningNameFactoryExists(JoiningNameFactory joiningNameFactory)
        {
            if (_joiningNameFactories.HasOne())
            {
                return;
            }

            var conflictingJoiningName = _joiningNameFactories
                .FirstOrDefault(jnf => jnf.ConflictsWith(joiningNameFactory));

            if (conflictingJoiningName == null)
            {
                return;
            }

            throw new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "Member names are already configured {0} to be {1}",
                conflictingJoiningName.TargetScopeDescription,
                conflictingJoiningName.SeparatorDescription));
        }

        public Expression GetJoiningName(Member member, IMemberMapperData mapperData)
            => _joiningNameFactories.FindMatch(mapperData).GetJoiningName(member, mapperData);

        public void Add(ElementKeyPartFactory keyPartFactory)
        {
            _elementKeyPartFactories.Insert(0, keyPartFactory);
        }

        public IEnumerable<Expression> GetElementKeyParts(Expression index, IBasicMapperData mapperData)
            => _elementKeyPartFactories.FindMatch(mapperData).GetElementKeyParts(index);
    }
}
