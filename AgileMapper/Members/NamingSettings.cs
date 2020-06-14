namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Caching;
    using Configuration;
    using Extensions;
    using Extensions.Internal;

    internal class NamingSettings
    {
        public static readonly string[] RootMatchingNames = { Constants.RootMemberName };
        private static readonly string[] _defaultIdMemberNames = { "Id", "Identifier" };

        private readonly ICache<TypeKey, Member> _idMemberCache;
        private readonly List<ConfiguredNamingPattern> _matchingNameFactories;
        private readonly List<Func<IEnumerable<string>, string>> _joinedNameFactories;
        private List<ConfiguredNamingPattern> _customNameMatchers;

        public NamingSettings(CacheSet mapperScopedCache)
        {
            _idMemberCache = mapperScopedCache.CreateScoped<TypeKey, Member>(default(HashCodeComparer<TypeKey>));

            _matchingNameFactories = new List<ConfiguredNamingPattern>
            {
                ConfiguredNamingPattern.Global(member => IsTypeIdentifier(member) ? "Id" : null),
                ConfiguredNamingPattern.ForGetOrSetMethod,
                ConfiguredNamingPattern.ForIdentifierName
            };

            _joinedNameFactories = new List<Func<IEnumerable<string>, string>>
            {
                names => names.Join(string.Empty),
                names => names.Join(".")
            };
        }

        private List<ConfiguredNamingPattern> CustomNameMatchers
            => _customNameMatchers ?? (_customNameMatchers = new List<ConfiguredNamingPattern>());

        public void Add(IList<ConfiguredNamingPattern> patterns)
        {
            _matchingNameFactories.AddRange(patterns);
            CustomNameMatchers.AddRange(patterns);
        }

        private bool IsTypeIdentifier(Member member)
            => GetIdentifierOrNull(member.DeclaringType)?.Equals(member) == true;

        public Member GetIdentifierOrNull(Type type)
        {
            return _idMemberCache.GetOrAdd(TypeKey.ForTypeId(type), key =>
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

            if (_customNameMatchers.NoneOrNull())
            {
                return false;
            }

            potentialIds.InsertRange(0, _defaultIdMemberNames);

            return _customNameMatchers
                .Project(member, (m, customNameMatcher) => customNameMatcher.GetMemberName(m))
                .WhereNotNull()
                .Any(potentialIds.Contains);
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

        public string[] GetMatchingNamesFor(Member member, IQualifiedMemberContext context)
        {
            return member.IsRoot
                ? RootMatchingNames
                : EnumerateMatchingNames(member, context).ToArray();
        }

        private IEnumerable<string> EnumerateMatchingNames(Member member, IQualifiedMemberContext context)
        {
            var matchingName = default(string);

            for (var i = 0; i < _matchingNameFactories.Count; ++i)
            {
                var factory = _matchingNameFactories[i];

                if (!factory.AppliesTo(context))
                {
                    continue;
                }

                matchingName = factory.GetMemberName(member);

                if (matchingName != null)
                {
                    yield return matchingName;
                    break;
                }
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
                    extendedJoinedNames[index++] = parentJoinedName + Constants.EnumerableElementName;
                    continue;
                }

                foreach (var name in names)
                {
                    if (wasElementMember)
                    {
                        extendedJoinedNames[index++] = parentJoinedName + "." + name;
                        continue;
                    }

                    foreach (var joinedNameFactory in _joinedNameFactories)
                    {
                        extendedJoinedNames[index++] = joinedNameFactory.Invoke(new[] { parentJoinedName, name });
                    }
                }
            }

            return extendedJoinedNames;
        }

        public void CloneTo(NamingSettings settings)
        {
            settings._matchingNameFactories.AddRange(_matchingNameFactories.Skip(3));
            settings._joinedNameFactories.AddRange(_joinedNameFactories.Skip(2));
            _customNameMatchers?.CopyTo(settings.CustomNameMatchers);
        }
    }
}