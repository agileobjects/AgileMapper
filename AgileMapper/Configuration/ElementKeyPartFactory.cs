namespace AgileObjects.AgileMapper.Configuration
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;
    using Extensions;
    using Members;

    internal class ElementKeyPartFactory : UserConfiguredItemBase
    {
        private readonly Expression _prefix;
        private readonly Expression _suffix;

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

        public static ElementKeyPartFactory SquareBracketedIndex(MapperContext mapperContext)
            => new ElementKeyPartFactory("[", "]", MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(mapperContext));

        private static readonly Regex _patternMatcher = new Regex("(?<Prefix>[^i]*)i(?<Suffix>[^i]*)"
#if !NET_STANDARD
            , RegexOptions.Compiled
#endif
            );

        public static ElementKeyPartFactory For(string pattern, MappingConfigInfo configInfo)
        {
            var patternMatch = _patternMatcher.Match(pattern);
            var prefix = patternMatch.Groups["Prefix"].Value;
            var suffix = patternMatch.Groups["Suffix"].Value;

            return new ElementKeyPartFactory(prefix, suffix, configInfo);
        }

        #endregion

        public IEnumerable<Expression> GetElementKeyParts(Expression index, IMemberMapperData mapperData)
        {
            if (_prefix != null)
            {
                yield return _prefix;
            }

            yield return mapperData.MapperContext.ValueConverters.GetConversion(index, typeof(string));

            if (_suffix != null)
            {
                yield return _suffix;
            }
        }
    }
}