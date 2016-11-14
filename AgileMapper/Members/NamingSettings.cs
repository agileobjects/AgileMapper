namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Extensions;

    internal class NamingSettings
    {
        public static readonly NamingSettings Default = new NamingSettings();

        private readonly ICollection<Func<Member, string>> _matchingNameFactories;
        private readonly IEnumerable<Func<Member, IEnumerable<string>>> _alternateNameFactories;
        private readonly ICollection<Func<IEnumerable<string>, string>> _joinedNameFactories;

        public NamingSettings()
        {
            _matchingNameFactories = new List<Func<Member, string>>
            {
                member => member.IsIdentifier ? "Id" : null,
                GetGetOrSetMethodName,
                GetIdentifierName
            };

            _alternateNameFactories = new Func<Member, IEnumerable<string>>[]
            {
                GetIdentifierNames,
                member => new [] { GetGetOrSetMethodName(member) }
            };

            _joinedNameFactories = new Func<IEnumerable<string>, string>[]
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

        private static IEnumerable<string> GetIdentifierNames(Member member)
        {
            if (member.IsIdentifier)
            {
                yield return "Id";
                yield return member.DeclaringType.Name + "Id";
                yield return "Identifier";
                yield return member.DeclaringType.Name + "Identifier";
            }
        }

        public void AddNameMatchers(IEnumerable<string> patterns)
        {
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

        public IEnumerable<string> GetAlternateNamesFor(Member member)
        {
            yield return member.Name;

            foreach (var alternateName in _alternateNameFactories.SelectMany(f => f.Invoke(member)).WhereNotNull())
            {
                yield return alternateName;
            }
        }

        public IEnumerable<string> GetJoinedNamesFor(IEnumerable<string> names)
        {
            return names.Any()
                ? _joinedNameFactories.Select(f => f.Invoke(names)).ToArray()
                : Constants.EmptyStringArray;
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
    }
}