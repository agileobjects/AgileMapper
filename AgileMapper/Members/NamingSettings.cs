namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;

    internal class NamingSettings
    {
        public static readonly NamingSettings Default = new NamingSettings();

        private readonly IEnumerable<Func<Member, string>> _matchingNameFactories;
        private readonly IEnumerable<Func<Member, IEnumerable<string>>> _alternateNameFactories;
        private readonly IEnumerable<Func<IEnumerable<string>, string>> _joinedNameFactories;

        public NamingSettings()
        {
            _matchingNameFactories = new List<Func<Member, string>>
            {
                member => member.IsRoot ? "Root" : null,
                member => member.IsIdentifier ? "Id" : null,
                GetGetOrSetMethodName,
                GetIdentifierName
            };

            _alternateNameFactories = new List<Func<Member, IEnumerable<string>>>
            {
                GetIdentifierNames,
                member => new [] { GetGetOrSetMethodName(member) }
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

        public string GetMatchingNameFor(Member member)
        {
            var matchingName = default(string);

            return _matchingNameFactories
                .Any(f => (matchingName = f.Invoke(member)) != null)
                    ? matchingName : member.Name;
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
            => names.Any() ? _joinedNameFactories.Select(f => f.Invoke(names)).ToArray() : null;
    }
}