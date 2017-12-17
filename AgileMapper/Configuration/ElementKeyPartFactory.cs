namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;
    using Extensions.Internal;
    using Members;
    using ReadableExpressions.Extensions;

    internal class ElementKeyPartFactory : UserConfiguredItemBase
    {
        private readonly ConstantExpression _prefix;
        private readonly ConstantExpression _suffix;
        private Expression _keyPartMatcher;

        private ElementKeyPartFactory(
            string prefix,
            string suffix,
            MappingConfigInfo configInfo)
            : base(configInfo)
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                _prefix = prefix.ToConstantExpression();
            }

            if (!string.IsNullOrEmpty(suffix))
            {
                _suffix = suffix.ToConstantExpression();
            }
        }

        #region Factory Methods

        public static ElementKeyPartFactory UnderscoredIndexForSourceDynamics(MapperContext mapperContext)
        {
            var sourceExpandoObject = new MappingConfigInfo(mapperContext)
                .ForAllRuleSets()
                .ForAllSourceTypes()
                .ForTargetType<ExpandoObject>();

            return new ElementKeyPartFactory("_", "_", sourceExpandoObject);
        }

        public static ElementKeyPartFactory UnderscoredIndexForTargetDynamics(MapperContext mapperContext)
        {
            var sourceExpandoObject = new MappingConfigInfo(mapperContext)
                .ForAllRuleSets()
                .ForSourceType<ExpandoObject>()
                .ForAllTargetTypes();

            return new ElementKeyPartFactory("_", "_", sourceExpandoObject);
        }

        public static ElementKeyPartFactory SquareBracketedIndex(MapperContext mapperContext)
            => new ElementKeyPartFactory("[", "]", MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(mapperContext));

        private static readonly Regex _patternMatcher = new Regex("^(?<Prefix>[^i]*)i{1}(?<Suffix>[^i]*)$"
#if !NET_STANDARD
            , RegexOptions.Compiled
#endif
            );

        public static ElementKeyPartFactory For(string pattern, MappingConfigInfo configInfo)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw InvalidPattern();
            }

            var patternMatch = _patternMatcher.Match(pattern);

            if (!patternMatch.Success)
            {
                throw InvalidPattern();
            }

            var prefix = patternMatch.Groups["Prefix"].Value;
            var suffix = patternMatch.Groups["Suffix"].Value;

            if (!configInfo.IsForAllSourceTypes() && !configInfo.SourceType.IsEnumerable())
            {
                configInfo = configInfo
                    .Clone()
                    .ForSourceType(typeof(IEnumerable<>).MakeGenericType(configInfo.SourceType));
            }

            if ((configInfo.TargetType != typeof(object)) && !configInfo.TargetType.IsEnumerable())
            {
                configInfo = configInfo
                    .Clone()
                    .ForTargetType(typeof(IEnumerable<>).MakeGenericType(configInfo.TargetType));
            }

            return new ElementKeyPartFactory(prefix, suffix, configInfo);
        }

        private static MappingConfigurationException InvalidPattern()
        {
            return new MappingConfigurationException(
                "An enumerable element key pattern must contain a single 'i' character " +
                "as a placeholder for the enumerable index");
        }

        #endregion

        public Expression GetElementKeyPartMatcher()
            => _keyPartMatcher ?? (_keyPartMatcher = CreateKeyPartRegex().ToConstantExpression());

        private Regex CreateKeyPartRegex()
        {
            return new Regex(
                _prefix?.Value + "[0-9]+" + _suffix?.Value
#if !NET_STANDARD
                , RegexOptions.Compiled
#endif
                );
        }

        public Expression GetElementKeyPrefixOrNull() => _prefix;

        public IEnumerable<Expression> GetElementKeyParts(Expression index)
        {
            if (_prefix != null)
            {
                yield return _prefix;
            }

            yield return ConfigInfo.MapperContext.ValueConverters.GetConversion(index, typeof(string));

            if (_suffix != null)
            {
                yield return _suffix;
            }
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString()
        {
            var sourceType = ConfigInfo.IsForAllSourceTypes()
                ? "All sources"
                : ConfigInfo.SourceType.GetFriendlyName();

            var targetTypeName = ConfigInfo.TargetType == typeof(object)
                ? "All targets"
                : TargetTypeName;

            return $"{sourceType} -> {targetTypeName}: {_prefix.Value}i{_suffix.Value}";
        }
    }
}