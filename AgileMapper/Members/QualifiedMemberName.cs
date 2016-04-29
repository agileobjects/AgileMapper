namespace AgileObjects.AgileMapper.Members
{
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;

    internal class QualifiedMemberName
    {
        private readonly QualifiedMemberNamePart[] _nameParts;
        private readonly IEnumerable<string> _allJoinedNames;

        public QualifiedMemberName(QualifiedMemberNamePart[] nameParts)
        {
            _nameParts = nameParts;
            Joined = GetJoinedName(nameParts);

            _allJoinedNames = _nameParts
                .Select(np => np.AllNames)
                .CartesianProduct()
                .Select(p => string.Join(string.Empty, p))
                .ToArray();
        }

        private static string GetJoinedName(IEnumerable<QualifiedMemberNamePart> nameParts)
        {
            return string.Join(string.Empty, nameParts.Select(np => np.JoiningName));
        }

        public bool IsIdentifier => _nameParts.Last().IsIdentifier;

        public string Joined { get; }

        public bool Matches(QualifiedMemberName otherQualifiedName)
        {
            if (_allJoinedNames.Intersect(otherQualifiedName._allJoinedNames).Any())
            {
                return true;
            }

            if (_nameParts.Length != otherQualifiedName._nameParts.Length)
            {
                return false;
            }

            return !_nameParts
                .Where((t, i) => !t.Matches(otherQualifiedName._nameParts[i]))
                .Any();
        }
    }
}