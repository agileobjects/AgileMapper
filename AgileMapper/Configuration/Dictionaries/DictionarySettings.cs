namespace AgileObjects.AgileMapper.Configuration.Dictionaries
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Api.Configuration.Dictionaries;
    using Extensions.Internal;
    using Members;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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
                JoiningNameFactory.UnderscoredForSourceDynamics(mapperContext),
                JoiningNameFactory.UnderscoredForTargetDynamics(mapperContext),
                JoiningNameFactory.Dotted(mapperContext)
            };

            _elementKeyPartFactories = new List<ElementKeyPartFactory>
            {
                ElementKeyPartFactory.UnderscoredIndexForSourceDynamics(mapperContext),
                ElementKeyPartFactory.UnderscoredIndexForTargetDynamics(mapperContext),
                ElementKeyPartFactory.SquareBracketedIndex(mapperContext)
            };
        }

        public void AddFullKey(CustomDictionaryKey configuredKey)
        {
            if (configuredKey.SourceMember?.IsSimple != false)
            {
                _configuredFullKeys.Add(configuredKey);
                return;
            }

            var targetDictionaryTypes = configuredKey.ConfigInfo.TargetType.GetDictionaryTypes();

            if (!targetDictionaryTypes.Value.IsSimple())
            {
                _configuredFullKeys.Add(configuredKey);
                return;
            }

            throw new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "Use .{0}(\"{1}\") instead of .{2}(\"{1}\") for source member {3}, " +
                "because {3} is being flattened to a Dictionary with value type {4}, " +
                "and will only appear as part of its child member's entries.",
                nameof(CustomTargetDictionaryKeySpecifier<object, object>.ToMemberNameKey),
                configuredKey.Key,
                nameof(CustomTargetDictionaryKeySpecifier<object, object>.ToFullKey),
                configuredKey.SourceMember.GetPath(),
                targetDictionaryTypes.Value.GetFriendlyName()));
        }

        public Expression GetFullKeyOrNull(IMemberMapperData mapperData)
            => GetFullKeyValueOrNull(mapperData)?.ToConstantExpression();

        public string GetFullKeyValueOrNull(IMemberMapperData mapperData)
        {
            if (mapperData.TargetMember.IsCustom)
            {
                return null;
            }

            var matchingKey = FindKeyOrNull(
                _configuredFullKeys,
                mapperData.TargetMember.LeafMember,
                mapperData);

            return matchingKey?.Key;
        }

        public void AddMemberKey(CustomDictionaryKey customKey)
        {
            _configuredMemberKeys.Add(customKey);
        }

        public string GetMemberKeyOrNull(IMemberMapperData mapperData)
            => GetMemberKeyOrNull(mapperData.TargetMember.LeafMember, mapperData);

        public string GetMemberKeyOrNull(Member member, IMemberMapperData mapperData)
            => FindKeyOrNull(_configuredMemberKeys, member, mapperData)?.Key;

        private static CustomDictionaryKey FindKeyOrNull(
            IList<CustomDictionaryKey> keys,
            Member member,
            IMemberMapperData mapperData)
            => keys.FirstOrDefault(k => k.AppliesTo(member, mapperData));

        public void Add(JoiningNameFactory joiningNameFactory)
        {
            ThrowIfConflictingKeyPartFactoryExists(joiningNameFactory, _joiningNameFactories);

            _joiningNameFactories.Insert(0, joiningNameFactory);
        }

        public Expression GetSeparator(IMemberMapperData mapperData)
            => _joiningNameFactories.FindMatch(mapperData).Separator;

        public Expression GetJoiningName(Member member, IMemberMapperData mapperData)
            => _joiningNameFactories.FindMatch(mapperData).GetJoiningName(member, mapperData);

        public void Add(ElementKeyPartFactory keyPartFactory)
        {
            ThrowIfConflictingKeyPartFactoryExists(keyPartFactory, _elementKeyPartFactories);

            _elementKeyPartFactories.Insert(0, keyPartFactory);
        }

        private static void ThrowIfConflictingKeyPartFactoryExists<TKeyPartFactory>(
            TKeyPartFactory factory,
            IList<TKeyPartFactory> existingFactories)
            where TKeyPartFactory : DictionaryKeyPartFactoryBase
        {
            if (existingFactories.HasOne())
            {
                return;
            }

            var conflictingFactory = existingFactories
                .FirstOrDefault(kpf => kpf.ConflictsWith(factory));

            if (conflictingFactory == null)
            {
                return;
            }

            throw new MappingConfigurationException(conflictingFactory.GetConflictMessage());
        }

        public Expression GetElementKeyPartMatcher(IBasicMapperData mapperData)
            => _elementKeyPartFactories.FindMatch(mapperData).GetElementKeyPartMatcher();

        public Expression GetElementKeyPrefixOrNull(IBasicMapperData mapperData)
            => _elementKeyPartFactories.FindMatch(mapperData).GetElementKeyPrefixOrNull();

        public IList<Expression> GetElementKeyParts(Expression index, IBasicMapperData mapperData)
            => _elementKeyPartFactories.FindMatch(mapperData).GetElementKeyParts(index).ToArray();

        public void CloneTo(DictionarySettings dictionaries)
        {
            dictionaries._configuredFullKeys.AddRange(_configuredFullKeys);
            dictionaries._configuredMemberKeys.AddRange(_configuredMemberKeys);
            dictionaries._joiningNameFactories.InsertRange(0, GetNonDefaultJoiningNameFactories());
            dictionaries._elementKeyPartFactories.InsertRange(0, GetNonDefaultElementKeyPartFactories());
        }

        private IEnumerable<JoiningNameFactory> GetNonDefaultJoiningNameFactories()
        {
            const int DEFAULT_JOINING_NAME_FACTORY_LENGTH = 3;

            return _joiningNameFactories
                .Take(_joiningNameFactories.Count - DEFAULT_JOINING_NAME_FACTORY_LENGTH)
                .Reverse();
        }

        private IEnumerable<ElementKeyPartFactory> GetNonDefaultElementKeyPartFactories()
        {
            const int DEFAULT_ELEMENT_KEY_PART_FACTORY_LENGTH = 3;

            return _elementKeyPartFactories
                .Take(_elementKeyPartFactories.Count - DEFAULT_ELEMENT_KEY_PART_FACTORY_LENGTH)
                .Reverse();
        }

        public void Reset()
        {
            _configuredFullKeys.Clear();
            _configuredMemberKeys.Clear();
            _joiningNameFactories.Clear();
            _elementKeyPartFactories.Clear();
        }
    }
}
