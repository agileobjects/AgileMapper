namespace AgileObjects.AgileMapper.Configuration.Dictionaries
{
    using System.Collections.Generic;
    using System.Dynamic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Text.RegularExpressions;
#if FEATURE_DYNAMIC
    using Api.Configuration.Dynamics;
#endif
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

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
#if FEATURE_DYNAMIC
        public static ElementKeyPartFactory UnderscoredIndexForSourceDynamics(MapperContext mapperContext)
        {
            var sourceExpandoObject = new MappingConfigInfo(mapperContext)
                .ForAllSourceTypes()
                .ForTargetExpandoObject();

            return new ElementKeyPartFactory("_", null, sourceExpandoObject);
        }

        public static ElementKeyPartFactory UnderscoredIndexForTargetDynamics(MapperContext mapperContext)
        {
            var sourceExpandoObject = new MappingConfigInfo(mapperContext)
                .ForSourceExpandoObject()
                .ForAllTargetTypes();

            return new ElementKeyPartFactory("_", null, sourceExpandoObject);
        }
#endif
        public static ElementKeyPartFactory SquareBracketedIndex(MapperContext mapperContext)
        {
            return new ElementKeyPartFactory(
                   "[", "]",
                    MappingConfigInfo.AllRuleSetsAndSourceTypes(mapperContext).ForAllTargetTypes());
        }

        private static readonly Regex _patternMatcher = new Regex("^(?<Prefix>[^i]*)i{1}(?<Suffix>[^i]*)$"
#if !NETSTANDARD1_0
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
                    .Copy()
                    .ForSourceType(configInfo.SourceType.GetEnumerableElementType());
            }

            if ((configInfo.TargetType != typeof(object)) &&
                (configInfo.TargetType != typeof(ExpandoObject)) &&
                 configInfo.TargetType.IsEnumerable())
            {
                configInfo = configInfo
                    .Copy()
                    .ForTargetType(configInfo.TargetType.GetEnumerableElementType());
            }

            return new ElementKeyPartFactory(prefix, suffix, configInfo);
        }

        private static MappingConfigurationException InvalidPattern()
        {
            return new MappingConfigurationException(
                "An enumerable element key pattern must contain a single 'i' character " +
                "as a placeholder for the element index");
        }

        #endregion

        private string Pattern => _prefixString + "i" + _suffixString;

        public Expression GetElementKeyPartMatcher()
            => _keyPartMatcher ??= CreateKeyPartRegex().ToConstantExpression();

        private Regex CreateKeyPartRegex()
        {
            return new Regex(
                _prefixString + "[0-9]+" + _suffixString
#if !NETSTANDARD1_0
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

            yield return ConfigInfo.MapperContext.GetValueConversion(index, typeof(string));

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
                : SourceTypeName;

            var targetTypeName = TargetType == typeof(object)
                ? "All targets"
                : TargetTypeName;

            return $"{sourceType} -> {targetTypeName}: {Pattern}";
        }
    }
}