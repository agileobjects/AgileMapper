namespace AgileObjects.AgileMapper.Configuration.Dictionaries
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Text.RegularExpressions;
    using Extensions.Internal;
    using Members;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ElementKeyPartFactory : DictionaryKeyPartFactoryBase
    {
        private readonly string _prefixString;
        private readonly ConstantExpression _prefix;
        private readonly string _suffixString;
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
                _prefixString = prefix;
                _prefix = prefix.ToConstantExpression();
            }

            if (!string.IsNullOrEmpty(suffix))
            {
                _suffixString = suffix;
                _suffix = suffix.ToConstantExpression();
            }
        }

        #region Factory Methods

        public static ElementKeyPartFactory UnderscoredIndexForSourceDynamics(MapperContext mapperContext)
        {
            var sourceExpandoObject = new MappingConfigInfo(mapperContext)
                .ForAllRuleSets()
                .ForAllSourceTypes()
                .ForTargetType<ExpandoObject>()
                .Set(DictionaryType.Expando);

            return new ElementKeyPartFactory("_", null, sourceExpandoObject);
        }

        public static ElementKeyPartFactory UnderscoredIndexForTargetDynamics(MapperContext mapperContext)
        {
            var sourceExpandoObject = new MappingConfigInfo(mapperContext)
                .ForAllRuleSets()
                .ForSourceType<ExpandoObject>()
                .ForAllTargetTypes()
                .Set(DictionaryType.Expando);

            return new ElementKeyPartFactory("_", null, sourceExpandoObject);
        }

        public static ElementKeyPartFactory SquareBracketedIndex(MapperContext mapperContext)
        {
            return new ElementKeyPartFactory(
                   "[", "]",
                    MappingConfigInfo.AllRuleSetsAndSourceTypes(mapperContext).ForAllTargetTypes());
        }

        private static readonly Regex _patternMatcher = new Regex("^(?<Prefix>[^i]*)i{1}(?<Suffix>[^i]*)$"
#if !NET_STANDARD
            , RegexOptions.Compiled
#endif
            );

        public static ElementKeyPartFactory For(string pattern, MappingConfigInfo configInfo)
        {
            if (pattern.IsNullOrWhiteSpace())
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

            if (!configInfo.IsForAllSourceTypes() &&
                (configInfo.SourceType != typeof(ExpandoObject)) &&
                 configInfo.SourceType.IsEnumerable())
            {
                configInfo = configInfo
                    .Clone()
                    .ForSourceType(configInfo.SourceType.GetEnumerableElementType());
            }

            if ((configInfo.TargetType != typeof(object)) &&
                (configInfo.TargetType != typeof(ExpandoObject)) &&
                 configInfo.TargetType.IsEnumerable())
            {
                configInfo = configInfo
                    .Clone()
                    .ForTargetType(configInfo.TargetType.GetEnumerableElementType());
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

        private string Pattern => _prefixString + "i" + _suffixString;

        public Expression GetElementKeyPartMatcher()
            => _keyPartMatcher ?? (_keyPartMatcher = CreateKeyPartRegex().ToConstantExpression());

        private Regex CreateKeyPartRegex()
        {
            return new Regex(
                _prefixString + "[0-9]+" + _suffixString
#if !NET_STANDARD
                , RegexOptions.Compiled
#endif
                );
        }

        public Expression GetElementKeyPrefixOrNull() => _prefix;

        public override bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
        {
            var otherFactory = ((ElementKeyPartFactory)otherConfiguredItem);

            if ((_prefixString != otherFactory._prefixString) || (_suffixString != otherFactory._suffixString) ||
                (ConfigInfo.Get<DictionaryType>() != otherConfiguredItem.ConfigInfo.Get<DictionaryType>()))
            {
                return false;
            }

            return base.ConflictsWith(otherConfiguredItem);
        }

        public override string GetConflictMessage()
            => $"Element keys are already configured {TargetScopeDescription} to be {Pattern}";

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

            return $"{sourceType} -> {targetTypeName}: {Pattern}";
        }
    }
}