namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Configuration;
    using Extensions;

    internal class NamingSettings
    {
        public static readonly NamingSettings Default = new NamingSettings();

        private readonly List<Func<Member, string>> _matchingNameFactories;
        private readonly List<Func<IEnumerable<string>, string>> _joinedNameFactories;

        public NamingSettings()
        {
            _matchingNameFactories = new List<Func<Member, string>>
            {
                member => member.IsIdentifier ? "Id" : null,
                GetGetOrSetMethodName,
                GetIdentifierName
            };

            _joinedNameFactories = new List<Func<IEnumerable<string>, string>>
            {
                names => string.Join(string.Empty, names),
                names => string.Join(".", names)
            };
        }

        private static string GetIdentifierName(Member member)
        {
            return member.Name.EndsWith("Identifier", StringComparison.Ordinal)
                ? member.Name.Substring(0, member.Name.Length - 8)
                : null;
        }

        private static string GetGetOrSetMethodName(Member member)
        {
            if ((member.MemberType == MemberType.GetMethod) ||
                (member.MemberType == MemberType.SetMethod))
            {
                return member.Name.Substring(3);
            }

            return null;
        }

        public void AddNamePrefixes(IEnumerable<string> prefixes)
            => AddNameMatchers(prefixes.Select(p => "^" + p + "(.+)$").ToArray());

        public void AddNameSuffixes(IEnumerable<string> suffixes)
            => AddNameMatchers(suffixes.Select(s => "^(.+)" + s + "$").ToArray());

        public void AddNameMatchers(IList<string> patterns)
        {
            ValidatePatterns(patterns);

            foreach (var pattern in patterns)
            {
                var nameMatcher = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

                _matchingNameFactories.Add(member => nameMatcher
                    .Match(member.Name)
                    .Groups
                    .Cast<Group>()
                    .ElementAtOrDefault(1)?
                    .Value);
            }
        }

        private static void ValidatePatterns(IList<string> patterns)
        {
            if (patterns.None())
            {
                throw new ArgumentException("No naming patterns supplied", nameof(patterns));
            }

            for (var i = 0; i < patterns.Count; i++)
            {
                var pattern = patterns[i];

                if (pattern == null)
                {
                    throw new ArgumentNullException(nameof(patterns), "Naming patterns cannot be null");
                }

                if (pattern.Contains(Environment.NewLine))
                {
                    throw CreateConfigurationException(pattern);
                }

                if (!pattern.StartsWith('^'))
                {
                    patterns[i] = pattern = "^" + pattern;
                }

                if (!pattern.EndsWith('$'))
                {
                    patterns[i] = pattern = pattern + "$";
                }

                ThrowIfPatternIsInvalid(pattern);
            }
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

        public string[] GetMatchingNamesFor(Member member) => EnumerateMatchingNames(member).ToArray();

        private IEnumerable<string> EnumerateMatchingNames(Member member)
        {
            if (member.IsRoot)
            {
                yield return "Root";
                yield break;
            }

            var matchingName = default(string);

            if (_matchingNameFactories.Any(f => (matchingName = f.Invoke(member)) != null))
            {
                yield return matchingName;
            }

            if (member.Name != matchingName)
            {
                yield return member.Name;
            }
        }

        public ICollection<string> GetJoinedNamesFor(string[][] matchingNameSets)
        {
            var joinedNames = default(ICollection<string>);

            foreach (var matchingNameSet in matchingNameSets)
            {
                if (joinedNames == null)
                {
                    joinedNames = matchingNameSet;
                    continue;
                }

                joinedNames = ExtendJoinedNames(joinedNames, matchingNameSet);
            }

            return joinedNames;
        }

        public ICollection<string> ExtendJoinedNames(ICollection<string> parentJoinedNames, string[] names)
        {
            var isElementMember = (names.Length == 1) && (names[0] == Constants.EnumerableElementName);
            var wasElementMember = parentJoinedNames.First().EndsWith(Constants.EnumerableElementName, StringComparison.Ordinal);

            var numberOfExtendedJoinedNames = isElementMember
                ? parentJoinedNames.Count
                : wasElementMember
                    ? parentJoinedNames.Count * names.Length
                    : parentJoinedNames.Count * names.Length * _joinedNameFactories.Count;

            var extendedJoinedNames = new string[numberOfExtendedJoinedNames];
            var index = 0;

            foreach (var parentJoinedName in parentJoinedNames)
            {
                if (isElementMember)
                {
                    extendedJoinedNames[index] = parentJoinedName + Constants.EnumerableElementName;
                    ++index;
                    continue;
                }

                foreach (var name in names)
                {
                    if (wasElementMember)
                    {
                        extendedJoinedNames[index] = parentJoinedName + "." + name;
                        ++index;
                        continue;
                    }

                    foreach (var joinedNameFactory in _joinedNameFactories)
                    {
                        extendedJoinedNames[index] = joinedNameFactory.Invoke(new[] { parentJoinedName, name });
                        ++index;
                    }
                }
            }

            return extendedJoinedNames;
        }

        public void CloneTo(NamingSettings settings)
        {
            settings._matchingNameFactories.AddRange(_matchingNameFactories);
            settings._joinedNameFactories.AddRange(_joinedNameFactories);
        }
    }
}