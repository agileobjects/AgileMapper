namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using ReadableExpressions.Extensions;

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
            => _configuredFullKeys.FindMatch(mapperData)?.Key.ToConstantExpression();

        public void AddMemberKey(CustomDictionaryKey customKey)
        {
            _configuredMemberKeys.Add(customKey);
        }

        public string GetMemberKeyOrNull(IBasicMapperData mapperData)
            => _configuredMemberKeys.FindMatch(mapperData)?.Key;

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
                .Where(jnf => !jnf.IsDefault && (jnf.IsGlobal == joiningNameFactory.IsGlobal))
                .FirstOrDefault(jnf => jnf.ConflictsWith(joiningNameFactory));

            if (conflictingJoiningName == null)
            {
                return;
            }

            var targetDescription = conflictingJoiningName.IsGlobal
                ? "globally"
                : "for target type " + joiningNameFactory.TargetType.GetFriendlyName();

            var separatorDescription = conflictingJoiningName.IsFlattened
                ? "flattened"
                : "separated with '" + conflictingJoiningName.Separator + "'";

            throw new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "Member names are already configured {0} to be {1}",
                targetDescription,
                separatorDescription));
        }

        public Expression GetJoiningName(string memberName, IMemberMapperData mapperData)
            => _joiningNameFactories.FindMatch(mapperData).GetJoiningName(memberName, mapperData);

        public void Add(ElementKeyPartFactory keyPartFactory)
        {
            _elementKeyPartFactories.Insert(0, keyPartFactory);
        }

        public IEnumerable<Expression> GetElementKeyParts(Expression index, IMemberMapperData mapperData)
            => _elementKeyPartFactories.FindMatch(mapperData).GetElementKeyParts(index, mapperData);
    }
}
