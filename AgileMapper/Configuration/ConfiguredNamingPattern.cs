namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Extensions.Internal;
    using Members;
    using static System.Text.RegularExpressions.RegexOptions;

    internal class ConfiguredNamingPattern : UserConfiguredItemBase
    {
        #region Default Instances

        public static readonly ConfiguredNamingPattern ForGetOrSetMethod = Global(GetGetOrSetMethodName);
        public static readonly ConfiguredNamingPattern ForIdentifierName = Global(GetIdentifierName);

        private static string GetGetOrSetMethodName(Member member)
        {
            if ((member.MemberType == MemberType.GetMethod) ||
                (member.MemberType == MemberType.SetMethod))
            {
                return member.Name.Substring(3);
            }

            return null;
        }

        private static string GetIdentifierName(Member member)
        {
            return member.Name.EndsWith("Identifier", StringComparison.Ordinal)
                ? member.Name.Substring(0, member.Name.Length - 8)
                : null;
        }

        #endregion

        private readonly Regex _nameMatcher;
        private readonly Func<Member, Regex, string> _matchingNameFactory;

        private ConfiguredNamingPattern(string pattern, MappingConfigInfo configInfo)
            : this(GetMemberNameFromRegex, configInfo)
        {
            _nameMatcher = new Regex(pattern, CultureInvariant | IgnoreCase);
        }

        private static string GetMemberNameFromRegex(Member member, Regex nameMatcher)
        {
            return nameMatcher
                .Match(member.Name)
                .Groups
                .Cast<Group>()
                .ElementAtOrDefault(1)?
                .Value;
        }

        private ConfiguredNamingPattern(
            Func<Member, Regex, string> matchingNameFactory,
            MappingConfigInfo configInfo)
            : base(configInfo)
        {
            _matchingNameFactory = matchingNameFactory;
        }

        #region Factory Methods

        public static ConfiguredNamingPattern Global(Func<Member, string> matchingNameFactory)
        {
            return new ConfiguredNamingPattern(
                (member, regex) => matchingNameFactory.Invoke(member),
                MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes);
        }

        public static IList<ConfiguredNamingPattern> Prefixes(
            IList<string> prefixes,
            MappingConfigInfo configInfo)
        {
            return Create(prefixes.ProjectToArray(prefix => "^" + prefix + "(.+)$"), configInfo);
        }

        public static IList<ConfiguredNamingPattern> Suffixes(
            IList<string> prefixes,
            MappingConfigInfo configInfo)
        {
            return Create(prefixes.ProjectToArray(suffix => "^(.+)" + suffix + "$"), configInfo);
        }

        public static IList<ConfiguredNamingPattern> Create(
            IList<string> patterns,
            MappingConfigInfo configInfo)
        {
            if (patterns.None())
            {
                throw new ArgumentException("No naming patterns supplied", nameof(patterns));
            }

            return patterns.ProjectToArray(
                configInfo,
                (ci, pattern) =>
                {
                    Validate(ref pattern);

                    return new ConfiguredNamingPattern(pattern, ci);
                });
        }

        private static void Validate(ref string pattern)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern), "Naming patterns cannot be null");
            }

            if (pattern.Contains(Environment.NewLine))
            {
                throw CreateConfigurationException(pattern);
            }

            if (!pattern.StartsWith('^'))
            {
                pattern = '^' + pattern;
            }

            if (!pattern.EndsWith('$'))
            {
                pattern += '$';
            }

            ThrowIfPatternIsInvalid(pattern);
        }

        private static readonly Regex _patternChecker =
            new Regex(@"^\^(?<Prefix>[^(]+){0,1}\(\.\+\)(?<Suffix>[^$]+){0,1}\$$");

        private static void ThrowIfPatternIsInvalid(string pattern)
        {
            var match = _patternChecker.Match(pattern);

            if (!match.Success)
            {
                throw CreateConfigurationException(pattern);
            }

            var prefix = match.Groups["Prefix"].Value;
            var suffix = match.Groups["Suffix"].Value;

            if (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(suffix))
            {
                throw CreateConfigurationException(pattern);
            }
        }

        private static Exception CreateConfigurationException(string pattern)
        {
            return new MappingConfigurationException(
                "Name pattern '" + pattern + "' is not valid. " +
                "Please specify a regular expression pattern in the format '^{prefix}(.+){suffix}$'");
        }

        #endregion

        public string GetMemberName(Member member)
            => _matchingNameFactory.Invoke(member, _nameMatcher);
    }
} 