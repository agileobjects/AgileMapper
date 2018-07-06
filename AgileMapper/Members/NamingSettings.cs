namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Caching;
    using Configuration;
    using Extensions.Internal;

    internal class NamingSettings
    {
        private readonly ICache<TypeKey, Member> _idMemberCache;
        private readonly List<Func<Member, string>> _matchingNameFactories;
        private readonly List<Func<IEnumerable<string>, string>> _joinedNameFactories;
        private readonly List<Regex> _customNameMatchers;

        public NamingSettings(CacheSet mapperScopedCache)
        {
            _idMemberCache = mapperScopedCache.CreateScoped<TypeKey, Member>();

            _matchingNameFactories = new List<Func<Member, string>>
            {
                member => IsTypeIdentifier(member) ? "Id" : null,
                GetGetOrSetMethodName,
                GetIdentifierName
            };

            _joinedNameFactories = new List<Func<IEnumerable<string>, string>>
            {
                names => names.Join(string.Empty),
                names => names.Join(".")
            };

            _customNameMatchers = new List<Regex>();
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
            => AddNameMatchers(prefixes.Project(p => "^" + p + "(.+)$").ToArray());

        public void AddNameSuffixes(IEnumerable<string> suffixes)
            => AddNameMatchers(suffixes.Project(s => "^(.+)" + s + "$").ToArray());

        public void AddNameMatchers(IList<string> patterns)
        {
            ValidatePatterns(patterns);

            foreach (var pattern in patterns)
            {
                var nameMatcher = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

                _matchingNameFactories.Add(member => GetMemberName(nameMatcher.Match(member.Name)));

                _customNameMatchers.Add(nameMatcher);
            }
        }

        private static string GetMemberName(Match memberNameMatch)
            => memberNameMatch.Groups.Cast<Group>().ElementAtOrDefault(1)?.Value;

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

        private bool IsTypeIdentifier(Member member)
            => GetIdentifierOrNull(member.DeclaringType)?.Equals(member) == true;

        public Member GetIdentifierOrNull(Type type) => GetIdentifierOrNull(TypeKey.ForTypeId(type));

        public Member GetIdentifierOrNull(TypeKey typeIdKey)
        {
            return _idMemberCache.GetOrAdd(typeIdKey, key =>
            {
                var typeMembers = GlobalContext.Instance.MemberCache.GetSourceMembers(key.Type);

                return typeMembers.FirstOrDefault(IsIdentifier);
            });
        }

        #region GetIdentifier Helpers

        private bool IsIdentifier(Member member)
        {
            if (member.Name == "Id")
            {
                return true;
            }

            if (member.Name.IndexOf("id", StringComparison.OrdinalIgnoreCase) == -1)
            {
                return false;
            }

            var declaringTypeName = member.DeclaringType.Name;

            if (member.Name == declaringTypeName + "Id")
            {
                return true;
            }

            var potentialIds = new List<string> { "Identifier", declaringTypeName + "Identifier" };

            AddPotentialTypeIdsIfApplicable(potentialIds, declaringTypeName, "Dto");
            AddPotentialTypeIdsIfApplicable(potentialIds, declaringTypeName, "ViewModel");

            if (potentialIds.Contains(member.Name))
            {
                return true;
            }

            if (_customNameMatchers.None())
            {
                return false;
            }

            potentialIds.InsertRange(0, new[] { "Id", "Identifier" });

            return _customNameMatchers
                .Project(customNameMatcher => customNameMatcher.Match(member.Name))
                .Any(memberNameMatch =>
                    memberNameMatch.Success &&
                    potentialIds.Contains(GetMemberName(memberNameMatch)));
        }

        private static void AddPotentialTypeIdsIfApplicable(
            ICollection<string> potentialIds,
            string declaringTypeName,
            string suffix)
        {
            if (!declaringTypeName.EndsWith(suffix, StringComparison.Ordinal))
            {
                return;
            }

            var prefix = declaringTypeName.Substring(0, declaringTypeName.Length - suffix.Length);

            potentialIds.Add(prefix + "Id");
            potentialIds.Add(prefix + "Identifier");
        }

        #endregion

        public string[] GetMatchingNamesFor(Member member) => EnumerateMatchingNames(member).ToArray();

        private IEnumerable<string> EnumerateMatchingNames(Member member)
        {
            if (member.IsRoot)
            {
                yield return Constants.RootMemberName;
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

        public IList<string> GetJoinedNamesFor(string[][] matchingNameSets)
        {
            var joinedNames = default(IList<string>);

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

        public IList<string> ExtendJoinedNames(ICollection<string> parentJoinedNames, string[] names)
        {
            var firstParentJoinedName = parentJoinedNames.First();

            if (parentJoinedNames.HasOne() && (firstParentJoinedName == Constants.RootMemberName))
            {
                // Don't bother to prepend 'Root' as a joined name:
                return names;
            }

            var isElementMember = (names.Length == 1) && (names[0] == Constants.EnumerableElementName);
            var wasElementMember = firstParentJoinedName.EndsWith(Constants.EnumerableElementName, StringComparison.Ordinal);

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